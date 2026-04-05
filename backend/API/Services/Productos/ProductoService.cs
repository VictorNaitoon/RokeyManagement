using API.Data;
using API.DTO.Request.Productos;
using API.DTO.Response.Productos;
using API.Models;
using API.Services.Auditoria;
using API.Services.Common;
using Microsoft.EntityFrameworkCore;
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

            var productos = await query
                .Select(p => new ProductoResponse
                {
                    Id = p.Id,
                    Nombre = p.Nombre,
                    CodigoBusqueda = p.CodigoBusqueda,
                    Descripcion = p.Descripcion,
                    PrecioCompra = p.PrecioCompra,
                    PrecioVenta = p.PrecioVenta,
                    StockActual = p.StockActual,
                    StockMinimo = p.StockMinimo,
                    ImagenURL = p.ImagenURL,
                    EsServicio = p.EsServicio,
                    Activo = p.Activo,
                    IdCategoria = p.IdCategoria,
                    NombreCategoria = p.Categoria != null ? p.Categoria.Nombre : null
                })
                .ToListAsync();

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
                PrecioCompra = _currentUser.IsAdmin ? producto.PrecioCompra : null,
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
            producto.StockActual = request.StockActual;
            producto.StockMinimo = request.StockMinimo;
            producto.ImagenURL = request.ImagenURL;
            producto.EsServicio = request.EsServicio;
            producto.Activo = request.Activo;
            producto.IdCategoria = request.IdCategoria;

            // Asignar usuario modificador
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
        /// Obtiene el historial de movimientos de stock de un producto
        /// </summary>
        public async Task<List<MovimientoStockResponse>> GetMovimientosStockAsync(int productoId, int idNegocio, int page, int pageSize, CancellationToken ct = default)
        {
            var movimientos = await _context.MovimientosStock
                .Where(m => m.IdProducto == productoId && (m.Id_negocio == idNegocio || m.Id_negocio == null))
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
            
            return movimientos;
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