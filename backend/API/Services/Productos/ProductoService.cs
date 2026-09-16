using API.Data;
using API.DTO.Request.Productos;
using API.DTO.Response.Productos;
using API.Exceptions;
using API.Models;
using API.Services.Auditoria;
using API.Services.Common;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text;
using System.Text.Json;

namespace API.Services.Productos
{
    public class ProductoService : IProductoService
    {
        private readonly AppDbContext _context;
        private readonly ICurrentUserService _currentUser;
        private readonly IAuditoriaService _auditoriaService;

        public ProductoService(AppDbContext context, ICurrentUserService currentUser, IAuditoriaService auditoriaService)
        {
            _context = context;
            _currentUser = currentUser;
            _auditoriaService = auditoriaService;
        }

        public async Task<ProductoListResponse> GetAllAsync(string? busqueda = null)
        {
            var query = _context.Productos
                .Where(p => p.Id_negocio == _currentUser.NegocioId)
                .Include(p => p.Categoria)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                query = query.Where(p => 
                    p.Nombre.Contains(busqueda) || 
                    p.CodigoBusqueda.Contains(busqueda));
            }

            // Post-ToList gating para evitar traducción EF Core bool ternaria (PC-01)
            // Traemos entidades completas, luego proyectamos con filtro por rol
            var entities = await query.ToListAsync();

            // PC-01: Dueño||Gerente see cost, Empleado null; tenant already filtered above
            var isEmpleado = _currentUser.IsEmpleado;
            var productos = entities.Select(p => new ProductoResponse
            {
                Id = p.Id,
                Nombre = p.Nombre,
                CodigoBusqueda = p.CodigoBusqueda,
                Descripcion = p.Descripcion,
                PrecioCompra = isEmpleado ? null : p.PrecioCompra,
                PrecioVenta = p.PrecioVenta,
                StockActual = p.StockActual,
                StockMinimo = p.StockMinimo,
                ImagenURL = p.ImagenURL,
                EsServicio = p.EsServicio,
                Activo = p.Activo,
                IdCategoria = p.IdCategoria,
                NombreCategoria = p.Categoria != null ? p.Categoria.Nombre : null
            }).ToList();

            return new ProductoListResponse
            {
                Productos = productos,
                Total = productos.Count
            };
        }

        public async Task<ProductoResponse?> GetByIdAsync(int id)
        {
            var producto = await _context.Productos
                .Where(p => p.Id == id && p.Id_negocio == _currentUser.NegocioId)
                .Include(p => p.Categoria)
                .FirstOrDefaultAsync();

            if (producto == null) return null;

            return new ProductoResponse
            {
                Id = producto.Id,
                Nombre = producto.Nombre,
                CodigoBusqueda = producto.CodigoBusqueda,
                Descripcion = producto.Descripcion,
                PrecioCompra = _currentUser.IsEmpleado ? null : producto.PrecioCompra,
                PrecioVenta = producto.PrecioVenta,
                StockActual = producto.StockActual,
                StockMinimo = producto.StockMinimo,
                ImagenURL = producto.ImagenURL,
                EsServicio = producto.EsServicio,
                Activo = producto.Activo,
                IdCategoria = producto.IdCategoria,
                NombreCategoria = producto.Categoria != null ? producto.Categoria.Nombre : null
            };
        }

        public async Task<ProductoResponse> CreateAsync(CrearProductoRequest request)
        {
            // Validar que la categoría exista y esté activa si se especifica
            if (request.IdCategoria.HasValue)
            {
                var categoria = await _context.Categorias
                    .FirstOrDefaultAsync(c => c.Id == request.IdCategoria.Value && 
                                            c.Id_negocio == _currentUser.NegocioId && 
                                            c.Activo);

                if (categoria == null)
                {
                    throw new InvalidOperationException("La categoría especificada no existe o está inactiva");
                }
            }

            // Validar unicidad de CodigoBusqueda si se especifica
            if (!string.IsNullOrWhiteSpace(request.CodigoBusqueda))
            {
                var existeCodigo = await _context.Productos
                    .AnyAsync(p => p.Id_negocio == _currentUser.NegocioId && 
                                   p.CodigoBusqueda == request.CodigoBusqueda);

                if (existeCodigo)
                {
                    throw new InvalidOperationException("Ya existe un producto con este código de búsqueda");
                }
            }

            var producto = new Producto
            {
                Id_negocio = _currentUser.NegocioId,
                IdUsuarioCreador = _currentUser.UserId,
                Nombre = request.Nombre,
                CodigoBusqueda = request.CodigoBusqueda,
                Descripcion = request.Descripcion,
                PrecioCompra = request.PrecioCompra,
                PrecioVenta = request.PrecioVenta,
                StockActual = request.StockActual,
                StockMinimo = request.StockMinimo,
                ImagenURL = request.ImagenURL,
                EsServicio = request.EsServicio,
                IdCategoria = request.IdCategoria,
                Activo = true
            };

            _context.Productos.Add(producto);
            await _context.SaveChangesAsync();

            // Registrar auditoría
            await _auditoriaService.RegistrarAsync(
                "Producto",
                producto.Id,
                "CREATE",
                null,
                new
                {
                    producto.Nombre,
                    producto.CodigoBusqueda,
                    producto.Descripcion,
                    producto.PrecioCompra,
                    producto.PrecioVenta,
                    producto.StockActual,
                    producto.StockMinimo,
                    producto.IdCategoria,
                    producto.Activo
                });

            return await GetByIdAsync(producto.Id);
        }

        public async Task<ProductoResponse?> UpdateAsync(int id, ActualizarProductoRequest request)
        {
            var producto = await _context.Productos
                .Where(p => p.Id == id && p.Id_negocio == _currentUser.NegocioId)
                .Include(p => p.Categoria)
                .FirstOrDefaultAsync();

            if (producto == null) return null;

            // Capturar estado antes de modificar para auditoría
            var datosAnteriores = new
            {
                producto.Nombre,
                producto.CodigoBusqueda,
                producto.Descripcion,
                producto.PrecioCompra,
                producto.PrecioVenta,
                producto.StockActual,
                producto.StockMinimo,
                producto.IdCategoria,
                producto.Activo,
                producto.ImagenURL,
                producto.EsServicio
            };

            // Validar que la categoría exista y esté activa si se especifica
            if (request.IdCategoria.HasValue)
            {
                var categoria = await _context.Categorias
                    .FirstOrDefaultAsync(c => c.Id == request.IdCategoria.Value && 
                                            c.Id_negocio == _currentUser.NegocioId && 
                                            c.Activo);

                if (categoria == null)
                {
                    throw new InvalidOperationException("La categoría especificada no existe o está inactiva");
                }
            }

            // Validar unicidad de CodigoBusqueda si se especifica y cambió
            if (!string.IsNullOrWhiteSpace(request.CodigoBusqueda) && 
                request.CodigoBusqueda != producto.CodigoBusqueda)
            {
                var existeCodigo = await _context.Productos
                    .AnyAsync(p => p.Id_negocio == _currentUser.NegocioId && 
                                   p.Id != id && 
                                   p.CodigoBusqueda == request.CodigoBusqueda);

                if (existeCodigo)
                {
                    throw new InvalidOperationException("Ya existe un producto con este código de búsqueda");
                }
            }

            producto.Nombre = request.Nombre;
            producto.CodigoBusqueda = request.CodigoBusqueda;
            producto.Descripcion = request.Descripcion;
            
            // Solo Admin puede modificar PrecioCompra
            if (_currentUser.IsAdmin && request.PrecioCompra.HasValue)
            {
                producto.PrecioCompra = request.PrecioCompra.Value;
            }
            
            producto.PrecioVenta = request.PrecioVenta;

            // Bug A: Dueño can edit StockActual directly via PUT with audit (Gerente blocked at controller)
            if (producto.StockActual != request.StockActual)
            {
                if (!_currentUser.IsAdmin)
                    throw new DomainException("No se puede modificar StockActual directamente. Use POST {id}/ajuste-stock con motivo.");

                if (request.StockActual < 0)
                    throw new DomainException("El stock no puede ser negativo");

                var stockAnterior = producto.StockActual;
                var stockNuevo = request.StockActual;
                var delta = stockNuevo - stockAnterior;

                var movimiento = new MovimientoStock
                {
                    IdProducto = producto.Id,
                    IdUsuario = _currentUser.UserId,
                    Id_negocio = _currentUser.NegocioId,
                    FechaMovimiento = DateTime.UtcNow,
                    Cantidad = delta,
                    TipoMovimiento = Models.Enums.TipoMovimiento.AjusteManual,
                    StockAnterior = stockAnterior,
                    StockNuevo = stockNuevo,
                    Motivo = "Edicion directa Dueno (PUT)"
                };
                _context.MovimientosStock.Add(movimiento);
                producto.StockActual = stockNuevo;
            }

            producto.StockMinimo = request.StockMinimo;
            producto.ImagenURL = request.ImagenURL;
            producto.EsServicio = request.EsServicio;
            producto.Activo = request.Activo;
            producto.IdCategoria = request.IdCategoria;

            // Assign modifier user
            producto.IdUsuarioModificador = _currentUser.UserId;

            await _context.SaveChangesAsync();

            // Registrar auditoría
            await _auditoriaService.RegistrarAsync(
                "Producto",
                producto.Id,
                "UPDATE",
                datosAnteriores,
                new
                {
                    producto.Nombre,
                    producto.CodigoBusqueda,
                    producto.Descripcion,
                    producto.PrecioCompra,
                    producto.PrecioVenta,
                    producto.StockActual,
                    producto.StockMinimo,
                    producto.IdCategoria,
                    producto.Activo,
                    producto.ImagenURL,
                    producto.EsServicio
                });

            return await GetByIdAsync(producto.Id);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var producto = await _context.Productos
                .FirstOrDefaultAsync(p => p.Id == id && p.Id_negocio == _currentUser.NegocioId);

            if (producto == null) return false;

            // Capturar estado antes de modificar para auditoría
            var datosAnteriores = new
            {
                producto.Nombre,
                producto.CodigoBusqueda,
                producto.Descripcion,
                producto.PrecioCompra,
                producto.PrecioVenta,
                producto.StockActual,
                producto.StockMinimo,
                producto.IdCategoria,
                producto.Activo
            };

            // Soft delete: desactivar en lugar de eliminar
            producto.Activo = false;
            producto.IdUsuarioModificador = _currentUser.UserId;
            await _context.SaveChangesAsync();

            // Registrar auditoría
            await _auditoriaService.RegistrarAsync(
                "Producto",
                producto.Id,
                "SOFT_DELETE",
                datosAnteriores,
                new { producto.Activo });

            return true;
        }

        public async Task<bool> ActivarAsync(int id)
        {
            var producto = await _context.Productos
                .FirstOrDefaultAsync(p => p.Id == id && p.Id_negocio == _currentUser.NegocioId);

            if (producto == null) return false;

            // Reactivar el producto
            producto.Activo = true;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<ProductoResponse> DuplicateAsync(int id, string nuevoNombre)
        {
            var productoOriginal = await _context.Productos
                .Where(p => p.Id == id && p.Id_negocio == _currentUser.NegocioId)
                .Include(p => p.Categoria)
                .FirstOrDefaultAsync();

            if (productoOriginal == null)
            {
                throw new InvalidOperationException("Producto no encontrado o no pertenece al negocio");
            }

            // Validar que el nuevo nombre no esté vacío
            if (string.IsNullOrWhiteSpace(nuevoNombre))
            {
                throw new InvalidOperationException("El nombre del producto duplicado no puede estar vacío");
            }

            var productoDuplicado = new Producto
            {
                Id_negocio = _currentUser.NegocioId,
                IdUsuarioCreador = _currentUser.UserId,
                Nombre = nuevoNombre.Trim(),
                CodigoBusqueda = null, // Se deja nulo para que el usuario lo asigne manualmente si lo desea
                Descripcion = productoOriginal.Descripcion,
                PrecioCompra = productoOriginal.PrecioCompra,
                PrecioVenta = productoOriginal.PrecioVenta,
                StockActual = productoOriginal.StockActual,
                StockMinimo = productoOriginal.StockMinimo,
                ImagenURL = productoOriginal.ImagenURL,
                EsServicio = productoOriginal.EsServicio,
                IdCategoria = productoOriginal.IdCategoria,
                Activo = true
            };

            _context.Productos.Add(productoDuplicado);
            await _context.SaveChangesAsync();

            // Registrar auditoría
            await _auditoriaService.RegistrarAsync(
                "Producto",
                productoDuplicado.Id,
                "CREATE",
                null,
                new
                {
                    productoDuplicado.Nombre,
                    productoDuplicado.CodigoBusqueda,
                    productoDuplicado.Descripcion,
                    productoDuplicado.PrecioCompra,
                    productoDuplicado.PrecioVenta,
                    productoDuplicado.StockActual,
                    productoDuplicado.StockMinimo,
                    productoDuplicado.IdCategoria,
                    productoDuplicado.Activo
                });

            return await GetByIdAsync(productoDuplicado.Id);
        }

        /// <summary>
        /// Obtiene productos con stock bajo del mínimo (StockActual &lt;= StockMinimo)
        /// </summary>
        public async Task<List<ProductoAlertaResponse>> GetProductosConStockBajoAsync(int idNegocio, CancellationToken ct = default)
        {
            var productos = await _context.Productos
                .Include(p => p.Categoria)
                .Where(p => p.Id_negocio == idNegocio 
                            && p.Activo 
                            && !p.EsServicio 
                            && p.StockActual <= p.StockMinimo)
                .OrderBy(p => p.StockMinimo - p.StockActual) // más críticos primero
                .Select(p => new ProductoAlertaResponse
                {
                    Id = p.Id,
                    Nombre = p.Nombre,
                    StockActual = p.StockActual,
                    StockMinimo = p.StockMinimo,
                    Diferencia = p.StockMinimo - p.StockActual,
                    CategoriaNombre = p.Categoria != null ? p.Categoria.Nombre : "Sin categoría"
                })
                .ToListAsync(ct);
            
            return productos;
        }

        /// <summary>
        /// Obtiene la cantidad de productos con stock bajo
        /// </summary>
        public async Task<int> GetContadorStockBajoAsync(int idNegocio, CancellationToken ct = default)
        {
            return await _context.Productos
                .Where(p => p.Id_negocio == idNegocio 
                            && p.Activo 
                            && !p.EsServicio 
                            && p.StockActual <= p.StockMinimo)
                .CountAsync(ct);
        }

        /// <summary>
        /// Obtiene el historial paginado de movimientos de stock — IA-01
        /// Strict tenant filter Id_negocio==NegocioId (drop legacy null), page/pageSize validated, PageSize max 100.
        /// Returns MovimientoStockListResponse { Movimientos, Total, Page, PageSize } ordered FechaMovimiento DESC.
        /// </summary>
        public async Task<MovimientoStockListResponse> GetMovimientosStockAsync(int productoId, int idNegocio, int page, int pageSize, CancellationToken ct = default)
        {
            // Validate and clamp — spec IA-01 Max pageSize 100
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 100) pageSize = 100;

            var baseQuery = _context.MovimientosStock
                .Where(m => m.IdProducto == productoId && m.Id_negocio == idNegocio);

            var total = await baseQuery.CountAsync(ct);

            var movimientos = await baseQuery
                .OrderByDescending(m => m.FechaMovimiento)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(m => new MovimientoStockResponse
                {
                    Id = m.Id,
                    FechaMovimiento = m.FechaMovimiento,
                    TipoMovimiento = m.TipoMovimiento.ToString(),
                    Cantidad = m.Cantidad,
                    StockAnterior = m.StockAnterior,
                    StockNuevo = m.StockNuevo,
                    Motivo = m.Motivo ?? "",
                    IdUsuario = m.IdUsuario
                })
                .ToListAsync(ct);

            return new MovimientoStockListResponse
            {
                Movimientos = movimientos,
                Total = total,
                Page = page,
                PageSize = pageSize
            };
        }

        /// <summary>
        /// Ajusta el stock de un producto con creación de MovimientoStock AjusteManual (auditable) — SA-01/SA-02.
        /// Distinct errors: NotFoundException (404 tenant isolation) vs DomainException/StockInsuficiente (422 no mutation).
        /// </summary>
        public async Task<ProductoResponse?> AjustarStockAsync(int id, AjusteStockRequest request, CancellationToken ct = default)
        {
            // Tenant-isolated load — Id_negocio from JWT, never from body
            var producto = await _context.Productos
                .FirstOrDefaultAsync(p => p.Id == id && p.Id_negocio == _currentUser.NegocioId, ct);

            if (producto == null)
                throw new NotFoundException("Producto", id);

            var stockAnterior = producto.StockActual;
            var stockNuevo = stockAnterior + request.CantidadDelta;

            // 422 — no mutation, no MovimientoStock, no SaveChanges
            if (stockNuevo < 0)
                throw new StockInsuficienteException(stockAnterior, request.CantidadDelta, stockNuevo);

            // Motivo trimmed 5..500 (validated by FluentValidation Delta!=0 + Motivo length; trim here for persistence)
            var motivoTrimmed = (request.Motivo ?? string.Empty).Trim();

            using var dbTransaction = await _context.Database.BeginTransactionAsync(ct);

            try
            {
                // Insert-only MovimientoStock AjusteManual (AGENTS 4.16 insert-only)
                var movimientoStock = new MovimientoStock
                {
                    IdProducto = producto.Id,
                    IdUsuario = _currentUser.UserId,
                    Id_negocio = _currentUser.NegocioId,
                    FechaMovimiento = DateTime.UtcNow,
                    Cantidad = request.CantidadDelta,
                    TipoMovimiento = Models.Enums.TipoMovimiento.AjusteManual,
                    StockAnterior = stockAnterior,
                    StockNuevo = stockNuevo,
                    Motivo = motivoTrimmed
                };

                _context.MovimientosStock.Add(movimientoStock);

                // Update stock + audit user
                producto.StockActual = stockNuevo;
                producto.IdUsuarioModificador = _currentUser.UserId;

                await _context.SaveChangesAsync(ct);

                await dbTransaction.CommitAsync(ct);

                // Return updated ProductoResponse with gated PrecioCompra (Dueño||Gerente vs Empleado)
                var updated = await GetByIdAsync(producto.Id);
                return updated ?? throw new NotFoundException("Producto", id);
            }
            catch
            {
                await dbTransaction.RollbackAsync(ct);
                throw;
            }
        }

        /// <summary>
        /// Importar productos desde CSV streaming CsvHelper 31.x — CI-01/CI-02
        /// Guards: 5MB/1000 rows, auto ,; BOM, escape =+-@, category Active+tenant, CodigoBusqueda unique scoped tenant+file, batch insert.
        /// </summary>
        public async Task<ImportCsvResponse> ImportarCsvAsync(IFormFile file, CancellationToken ct = default)
        {
            const long maxBytes = 5L * 1024 * 1024;
            const int maxRows = 1000;
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is required");
            if (file.Length > maxBytes)
                throw new InvalidOperationException("Size exceeds 5MB");

            var negocioId = _currentUser.NegocioId;
            var userId = _currentUser.UserId;
            var response = new ImportCsvResponse();
            var errors = new List<ImportError>();
            var validProductos = new List<Producto>();

            // Preload existing CodigoBusqueda scoped tenant for uniqueness
            var existingCodes = new HashSet<string>(
                await _context.Productos.Where(p => p.Id_negocio == negocioId && p.CodigoBusqueda != null)
                    .Select(p => p.CodigoBusqueda!.Trim().ToLowerInvariant()).ToListAsync(ct),
                StringComparer.OrdinalIgnoreCase);
            var fileCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // Preload categories Active+tenant for fast lookup
            var categoriasByName = await _context.Categorias
                .Where(c => c.Id_negocio == negocioId && c.Activo)
                .ToDictionaryAsync(c => c.Nombre.Trim().ToLowerInvariant(), c => c.Id, ct);
            var categoriaIds = new HashSet<int>(await _context.Categorias
                .Where(c => c.Id_negocio == negocioId && c.Activo).Select(c => c.Id).ToListAsync(ct));

            var cfg = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                DetectDelimiter = true,
                HasHeaderRecord = true,
                TrimOptions = TrimOptions.Trim,
                PrepareHeaderForMatch = args => args.Header.Trim().ToLowerInvariant(),
                BadDataFound = null,
                MissingFieldFound = null,
            };

            using var stream = file.OpenReadStream();
            using var reader = new StreamReader(stream, new UTF8Encoding(true), detectEncodingFromByteOrderMarks: true);
            using var csv = new CsvReader(reader, cfg);

            if (!await csv.ReadAsync())
                return new ImportCsvResponse { TotalRows = 0, Created = 0, Skipped = 0, Errors = errors };
            csv.ReadHeader();
            var header = csv.HeaderRecord ?? Array.Empty<string>();
            bool Has(string name) => header.Any(h => string.Equals(h?.Trim(), name, StringComparison.OrdinalIgnoreCase));

            int rowNum = 1; // header is row 1
            while (await csv.ReadAsync())
            {
                ct.ThrowIfCancellationRequested();
                rowNum++;
                if (response.TotalRows >= maxRows)
                {
                    throw new InvalidOperationException("Row limit 1000 exceeded");
                }
                response.TotalRows++;

                string Get(string name)
                {
                    if (!Has(name)) return string.Empty;
                    try { return (csv.GetField<string>(name) ?? string.Empty).Trim(); } catch { return string.Empty; }
                }

                var nombreRaw = Get("nombre");
                var precioVentaRaw = Get("precioventa");
                var precioCompraRaw = Get("preciocompra");
                var stockActualRaw = Get("stockactual");
                var stockMinimoRaw = Get("stockminimo");
                var codigoRaw = Get("codigobusqueda");
                var descripcionRaw = Get("descripcion");
                var nombreCategoriaRaw = Get("nombrecategoria");
                var idCategoriaRaw = Get("idcategoria");
                var esServicioRaw = Get("esservicio");
                var activoRaw = Get("activo");

                string Escape(string? v)
                {
                    if (string.IsNullOrEmpty(v)) return v ?? string.Empty;
                    var t = v.Trim();
                    if (t.Length > 0 && (t[0] == '=' || t[0] == '+' || t[0] == '-' || t[0] == '@'))
                        return "'" + t;
                    return t;
                }

                bool TryAddError(string col, string msg)
                {
                    errors.Add(new ImportError { Row = rowNum, Column = col, Message = msg });
                    return false;
                }

                // Nombre required
                if (string.IsNullOrWhiteSpace(nombreRaw))
                { TryAddError("Nombre", "Nombre is required"); continue; }
                var nombre = Escape(nombreRaw);
                if (nombre.Length > 200) { TryAddError("Nombre", "Nombre max 200"); continue; }

                // PrecioVenta >0
                decimal precioVenta;
                if (!decimal.TryParse(precioVentaRaw, NumberStyles.Any, CultureInfo.InvariantCulture, out precioVenta) &&
                    !decimal.TryParse(precioVentaRaw, NumberStyles.Any, new CultureInfo("es-AR"), out precioVenta))
                { TryAddError("PrecioVenta", "PrecioVenta required >0"); continue; }
                if (precioVenta <= 0) { TryAddError("PrecioVenta", "PrecioVenta must be >0"); continue; }

                // PrecioCompra >=0 optional
                decimal precioCompra = 0;
                if (!string.IsNullOrWhiteSpace(precioCompraRaw))
                {
                    if (!decimal.TryParse(precioCompraRaw, NumberStyles.Any, CultureInfo.InvariantCulture, out precioCompra) &&
                        !decimal.TryParse(precioCompraRaw, NumberStyles.Any, new CultureInfo("es-AR"), out precioCompra))
                    { TryAddError("PrecioCompra", "PrecioCompra must be >=0"); continue; }
                    if (precioCompra < 0) { TryAddError("PrecioCompra", "PrecioCompra must be >=0"); continue; }
                }

                // StockActual >=0 default 0
                int stockActual = 0;
                if (!string.IsNullOrWhiteSpace(stockActualRaw) && (!int.TryParse(stockActualRaw, out stockActual) || stockActual < 0))
                { TryAddError("StockActual", "StockActual must be >=0"); continue; }
                int stockMinimo = 0;
                if (!string.IsNullOrWhiteSpace(stockMinimoRaw) && (!int.TryParse(stockMinimoRaw, out stockMinimo) || stockMinimo < 0))
                { TryAddError("StockMinimo", "StockMinimo must be >=0"); continue; }

                var codigo = string.IsNullOrWhiteSpace(codigoRaw) ? null : Escape(codigoRaw.Trim());
                if (codigo != null && codigo.Length > 50) { TryAddError("CodigoBusqueda", "CodigoBusqueda max 50"); continue; }
                if (codigo != null)
                {
                    var key = codigo.Trim().ToLowerInvariant();
                    if (existingCodes.Contains(key) || fileCodes.Contains(key))
                    { TryAddError("CodigoBusqueda", "Duplicate CodigoBusqueda"); continue; }
                }

                // Category lookup Active+tenant, IdCategoria override
                int? idCategoria = null;
                if (!string.IsNullOrWhiteSpace(idCategoriaRaw) && int.TryParse(idCategoriaRaw, out var parsedCatId))
                {
                    if (!categoriaIds.Contains(parsedCatId))
                    { TryAddError("IdCategoria", "Categoria not found or inactive"); continue; }
                    idCategoria = parsedCatId;
                }
                else if (!string.IsNullOrWhiteSpace(nombreCategoriaRaw))
                {
                    var catKey = nombreCategoriaRaw.Trim().ToLowerInvariant();
                    if (!categoriasByName.TryGetValue(catKey, out var catId))
                    { TryAddError("NombreCategoria", "Categoria not found or inactive"); continue; }
                    idCategoria = catId;
                }

                bool esServicio = true;
                if (!string.IsNullOrWhiteSpace(esServicioRaw) && bool.TryParse(esServicioRaw, out var b1)) esServicio = b1;
                else if (esServicioRaw == "1" || esServicioRaw.Equals("true", StringComparison.OrdinalIgnoreCase)) esServicio = true;
                else if (esServicioRaw == "0" || esServicioRaw.Equals("false", StringComparison.OrdinalIgnoreCase)) esServicio = false;

                bool activo = true;
                if (!string.IsNullOrWhiteSpace(activoRaw) && bool.TryParse(activoRaw, out var b2)) activo = b2;
                else if (activoRaw == "0" || activoRaw.Equals("false", StringComparison.OrdinalIgnoreCase)) activo = false;

                var descripcion = string.IsNullOrWhiteSpace(descripcionRaw) ? null : Escape(descripcionRaw.Trim());
                if (descripcion != null && descripcion.Length > 500) descripcion = descripcion[..500];

                var producto = new Producto
                {
                    Id_negocio = negocioId,
                    IdUsuarioCreador = userId,
                    Nombre = nombre,
                    CodigoBusqueda = codigo,
                    Descripcion = descripcion,
                    PrecioCompra = precioCompra,
                    PrecioVenta = precioVenta,
                    StockActual = stockActual,
                    StockMinimo = stockMinimo,
                    EsServicio = esServicio,
                    Activo = activo,
                    IdCategoria = idCategoria
                };
                validProductos.Add(producto);
                if (codigo != null) fileCodes.Add(codigo.Trim().ToLowerInvariant());
            }

            if (response.TotalRows > maxRows)
                throw new InvalidOperationException("Row limit 1000 exceeded");

            if (validProductos.Count > 0)
            {
                _context.Productos.AddRange(validProductos);
                await _context.SaveChangesAsync(ct);
                // track saved codes for future imports consistency not needed
            }

            response.Created = validProductos.Count;
            response.Skipped = response.TotalRows - response.Created;
            response.Errors = errors;
            return response;
        }

        /// <summary>
        /// Actualización masiva de precios de productos por categoría (CU-010)
        /// </summary>
        public async Task<ActualizacionMasivaPreciosResponse> ActualizarPreciosPorCategoriaAsync(ActualizacionMasivaPreciosCategoriaRequest request)
        {
            // Validar RB-010 según TipoActualizacion
            switch (request.TipoActualizacion)
            {
                case Models.Enums.TipoActualizacion.Porcentaje:
                    if (!request.Porcentaje.HasValue)
                    {
                        throw new InvalidOperationException("Porcentaje requerido para tipo PORCENTAJE");
                    }
                    break;
                case Models.Enums.TipoActualizacion.PrecioFijo:
                    if (!request.PrecioFijo.HasValue)
                    {
                        throw new InvalidOperationException("PrecioFijo requerido para tipo PRECIO_FIJO");
                    }
                    break;
                case Models.Enums.TipoActualizacion.Incremento:
                    if (!request.Incremento.HasValue)
                    {
                        throw new InvalidOperationException("Incremento requerido para tipo INCREMENTO");
                    }
                    break;
            }

            // RB-003: Verificar categoría existe y pertenece al negocio
            var categoria = await _context.Categorias
                .FirstOrDefaultAsync(c => c.Id == request.IdCategoria && 
                                        c.Id_negocio == _currentUser.NegocioId && 
                                        c.Activo);

            if (categoria == null)
            {
                throw new InvalidOperationException("La categoría no existe");
            }

            // Cargar productos de la categoría
            var productos = await _context.Productos
                .Where(p => p.Id_negocio == _currentUser.NegocioId && 
                            p.IdCategoria == request.IdCategoria &&
                            p.Activo)
                .ToListAsync();

            return await ProcesarActualizacionPreciosAsync(productos, request.TipoActualizacion, 
                request.Porcentaje, request.PrecioFijo, request.Incremento, request.CampoPrecio);
        }

        /// <summary>
        /// Actualización masiva de precios de productos específicos (CU-010)
        /// </summary>
        public async Task<ActualizacionMasivaPreciosResponse> ActualizarPreciosPorProductosAsync(ActualizacionMasivaPreciosProductosRequest request)
        {
            // Validar RB-010 según TipoActualizacion
            switch (request.TipoActualizacion)
            {
                case Models.Enums.TipoActualizacion.Porcentaje:
                    if (!request.Porcentaje.HasValue)
                    {
                        throw new InvalidOperationException("Porcentaje requerido para tipo PORCENTAJE");
                    }
                    break;
                case Models.Enums.TipoActualizacion.PrecioFijo:
                    if (!request.PrecioFijo.HasValue)
                    {
                        throw new InvalidOperationException("PrecioFijo requerido para tipo PRECIO_FIJO");
                    }
                    break;
                case Models.Enums.TipoActualizacion.Incremento:
                    if (!request.Incremento.HasValue)
                    {
                        throw new InvalidOperationException("Incremento requerido para tipo INCREMENTO");
                    }
                    break;
            }

            // Validar que hay productos seleccionados
            if (request.IdsProductos == null || !request.IdsProductos.Any())
            {
                throw new InvalidOperationException("Debe especificar al menos un producto a actualizar");
            }

            // RB-004: Verificar todos los productos existen y pertenecen al negocio
            var productos = await _context.Productos
                .Where(p => request.IdsProductos.Contains(p.Id) && 
                            p.Id_negocio == _currentUser.NegocioId &&
                            p.Activo)
                .ToListAsync();

            if (productos.Count != request.IdsProductos.Count)
            {
                var encontrados = productos.Select(p => p.Id).ToList();
                var noEncontrados = request.IdsProductos.Where(id => !encontrados.Contains(id)).ToList();
                throw new InvalidOperationException($"Los siguientes productos no fueron encontrados: {string.Join(", ", noEncontrados)}");
            }

            return await ProcesarActualizacionPreciosAsync(productos, request.TipoActualizacion, 
                request.Porcentaje, request.PrecioFijo, request.Incremento, request.CampoPrecio);
        }

        /// <summary>
        /// Lógica común para procesar actualización de precios
        /// </summary>
        private async Task<ActualizacionMasivaPreciosResponse> ProcesarActualizacionPreciosAsync(
            List<Producto> productos,
            Models.Enums.TipoActualizacion tipoActualizacion,
            decimal? porcentaje,
            decimal? precioFijo,
            decimal? incremento,
            Models.Enums.CampoPrecio campoPrecio)
        {
            var detalles = new List<DetalleActualizacion>();
            var preciosInvalidos = new List<string>();

            using (var dbTransaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // Procesar cada producto
                    foreach (var producto in productos)
                    {
                        // Capturar valores anteriores para auditoría
                        var precioVentaAnterior = producto.PrecioVenta;
                        var precioCompraAnterior = producto.PrecioCompra;

                        // Calcular nuevos precios según tipo de actualización
                        decimal nuevoPrecioVenta = precioVentaAnterior;
                        decimal nuevoPrecioCompra = precioCompraAnterior;

                        switch (campoPrecio)
                        {
                            case Models.Enums.CampoPrecio.PrecioVenta:
                                nuevoPrecioVenta = tipoActualizacion switch
                                {
                                    Models.Enums.TipoActualizacion.Porcentaje => Math.Round(precioVentaAnterior * (1 + porcentaje!.Value / 100), 2, MidpointRounding.AwayFromZero),
                                    Models.Enums.TipoActualizacion.PrecioFijo => Math.Round(precioFijo!.Value, 2, MidpointRounding.AwayFromZero),
                                    Models.Enums.TipoActualizacion.Incremento => Math.Round(precioVentaAnterior + incremento!.Value, 2, MidpointRounding.AwayFromZero),
                                    _ => nuevoPrecioVenta
                                };
                                break;
                            case Models.Enums.CampoPrecio.PrecioCompra:
                                nuevoPrecioCompra = tipoActualizacion switch
                                {
                                    Models.Enums.TipoActualizacion.Porcentaje => Math.Round(precioCompraAnterior * (1 + porcentaje!.Value / 100), 2, MidpointRounding.AwayFromZero),
                                    Models.Enums.TipoActualizacion.PrecioFijo => Math.Round(precioFijo!.Value, 2, MidpointRounding.AwayFromZero),
                                    Models.Enums.TipoActualizacion.Incremento => Math.Round(precioCompraAnterior + incremento!.Value, 2, MidpointRounding.AwayFromZero),
                                    _ => nuevoPrecioCompra
                                };
                                break;
                        }

                        // RB-005: Validar precios finales > 0
                        var precioVentaValido = campoPrecio != Models.Enums.CampoPrecio.PrecioVenta || nuevoPrecioVenta > 0;
                        var precioCompraValido = campoPrecio != Models.Enums.CampoPrecio.PrecioCompra || nuevoPrecioCompra > 0;

                        if (!precioVentaValido || !precioCompraValido)
                        {
                            preciosInvalidos.Add(producto.Nombre);
                            continue;
                        }

                        // Actualizar precios
                        if (campoPrecio == Models.Enums.CampoPrecio.PrecioVenta)
                        {
                            producto.PrecioVenta = nuevoPrecioVenta;
                        }
                        if (campoPrecio == Models.Enums.CampoPrecio.PrecioCompra)
                        {
                            producto.PrecioCompra = nuevoPrecioCompra;
                        }

                        // Asignar usuario modificador
                        producto.IdUsuarioModificador = _currentUser.UserId;

                        // Registrar auditoría por cada producto (Tarea 4.1-4.3)
                        await _auditoriaService.RegistrarAsync(
                            "Productos",
                            producto.Id,
                            "UPDATE",
                            new
                            {
                                PrecioVenta = precioVentaAnterior,
                                PrecioCompra = precioCompraAnterior
                            },
                            new
                            {
                                PrecioVenta = nuevoPrecioVenta,
                                PrecioCompra = nuevoPrecioCompra,
                                Operacion = "ActualizacionMasivaPrecios"
                            });

                        detalles.Add(new DetalleActualizacion
                        {
                            IdProducto = producto.Id,
                            NombreProducto = producto.Nombre,
                            PrecioVentaAnterior = precioVentaAnterior,
                            PrecioVentaNuevo = nuevoPrecioVenta,
                            PrecioCompraAnterior = precioCompraAnterior,
                            PrecioCompraNuevo = nuevoPrecioCompra
                        });
                    }

                    // RB-005: Si hay precios inválidos, rechazar toda la operación
                    if (preciosInvalidos.Any())
                    {
                        throw new InvalidOperationException($"La operación resultaría en precio inválido para los siguientes productos: {string.Join(", ", preciosInvalidos)}");
                    }

                    // Guardar cambios
                    await _context.SaveChangesAsync();

                    // Confirmar transacción
                    await dbTransaction.CommitAsync();

                    return new ActualizacionMasivaPreciosResponse
                    {
                        TotalProductosActualizados = detalles.Count,
                        TotalProductosVerificados = productos.Count,
                        Detalles = detalles,
                        FechaActualizacion = DateTime.UtcNow
                    };
                }
                catch
                {
                    // Rollback en caso de error
                    await dbTransaction.RollbackAsync();
                    throw;
                }
            }
        }
    }
}