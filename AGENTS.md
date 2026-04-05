# RoKey MANAGEMENT
This file contains the business context and coding rules that the AI must follow in every interaction with the **RoKey MANAGEMENT** project. Generated from: SRS IEEE 830, Use Cases (UC-001-UC-019), ER model and project folder.

## 1. Description System
**RoKey MANAGEMENT** is a multi-tenant SaaS ERP/POS system designed for small and medium-sized businesses (SMBs) in the **hardware store** and **locksmith sector**, or a **combination of both**.
Each **Business** is an independent tenant: All its data (products, sales, users, etc) is isolated by Id_negocio. Data is never mixed between different businesses.
The system offers:
- Internal management: sales, purchasses, inventory, budgets, invoices, cash, and auditing.
- Public catalog interface (hardware store only): shopping cart + order via WhatsApp.
- Dashboard with financial and performance metrics by role.

## 2. Technology Stack
- **Backend**: ASP.NET Core (.NET 8.0+) - Clean Architecture
- **ORM**: Entity Framework Core with Repository pattern (DbSet)
- **CQRS**: MediatR (Commands + Queries + Pipeline Behaviours)
- **Mapping**: Mapster (DON'T use AutoMapper - it is a paid service)
- **Validation**: FluentValidation or Data Annotations, whichever is better
- **Authentication**: JWT Bearer (Microsoft.AspNetCore.Authentication.JwtBearer)
- **Documentation**: Swagger/OpenAPI with Bearer token support.
- **Database**: PostgreSQL

## 3. Architecture - Clean Architecture
src/
├── Domain/           → Entities, domain exceptions, repository interfaces
├── Application/      → Use cases (Commands/Queries), DTOs, validators, mappers
├── Infrastructure/   → EF Core, repositories, JWT, email, WhatsApp API
└── API/              → Controllers, DI registration, middleware
**Dependency rule**: API -> Application -> Domain. Infrastructure implements interfaces defined in Domain. Application never imports Infrastructure directly.

## 4. Domain model - Entities and Attributes
### 4.1 Negocio (root tenant)
Id, Nombre (Razón social), CUIT, Dirección, Logo_Url,
Estado: Activo | Inactivo,
TipoNegocio: Ferreteria | Cerrajeria | Ambos,
Ingresos_brutos, Inicio_actividades, Punto_de_venta, Condicion_de_venta

- CRITICAL: All system entities must include Id_negocio and validate that the authenticated user belongs to the same business before of reading or writing data.
- The 'Inactivo' state blocks full access to the system (business witout an active payment/subscription)
- The business type controls which interfaces and funtionalities are available.
### 4.2 Usuario
Id, Id_negocio, Nombre, Apellido, Email (único por negocio), Contraseña (hash), Rol

- Role: Admin (Owner) | Manager | Seller 
- The email cannot be duplicated within the same business.
- Each user belongs to exactly one business.
- The password is stored as a hash (bcrypt or equivalent). Never in plain text.
### 4.3 Producto
Id, Id_negocio, Id_usuario_creador, Id_categoria,
Nombre, Codigo_busqueda, Descripcion,
Precio_compra, Precio_venta,
Stock_actual, Stock_minimo,
Foto, esServicio (bool)

- If Stock_actual <= Stock_minimo, the system triggers a low stock alert.
- esServicio = true: No stock is deducted when selling
- Precio_compra is a sensitive data: Only Admins can see it. Sellers cannot.
- Supports product duplication (for color/size variants, especially for hardware store).
### 4.4 Categoría
Id, Id_negocio, Id_usuario, Nombre, Descripcion, Activo (bool)

- It is deactivated (Activo = false) instead of being deleted to preserve historical integrity.
- A product requires an existing active category.
### 4.5 Venta
Id, Id_negocio, Id_usuario, Id_cliente (nullable), Fecha, Total_venta

- It is not deleted, only canceled. The cancellation reverses the stock via a MovimientoStock.
- If the sales has official invoice, the cancellation requires issuing a credit note first.
- Requires an open register at the time of recording.
### 4.6 Detalle_Venta
Id, Id_venta, Id_producto, Cantidad_vendida, Precio_unitario

- The price is recorded at the time of the sale (historical price, not reference to the product)
### 4.7 Pago
Id, Id_venta, Metodo_pago, Monto

- Metodo_pago: Cash | Credit | Debit | Bank Transfer
- A sale can have multiple payments (mixed payment). The sum of the amounts must equal Total_venta.
### 4.8 Factura
Id, Id_negocio, Id_venta, CUIT_cliente,
Fecha_realizada, Tipo_comprobante, Numero_comprobante,
CAE, Vencimiento_CAE, QR, Condicion_venta

- It can be official (with CAE issued by ARCA) or proforma (without fiscal validity)
- Must be linked to one sale only.
### 4.9 Compra
Id, Id_negocio, Id_proveedor, Id_usuario, Fecha, Total_gasto, Nro_factura_original

- Nro_factura_original is required (supplier invoices number)
- When recorded, it automatically increase the stock of the product in the details.
- It isn't deleted, only canceled. The cancellation generates a negative MovimientoStock.
### 4.10 Detalle_Compra
Id, Id_compra, Id_producto, Cantidad_comprada, Costo_unitario
### 4.11 Presupuesto
Id, Id_negocio, Id_usuario, Id_cliente (nullable),
Fecha_emision, Fecha_vencimiento,
Estado: Pendiente | Aceptado | Vencido, Total

- The state is automatically calculated: if Fecha_vencimiento < hoy and it remains Pendiente -> Vencido.
- When converted to a sale: estado -> Aceptado; records are created in Venta and Detalle_Venta; stock is automatically deducted.
### 4.12 Detalle_Presupuesto
Id, Id_presupuesto, Id_producto, Cantidad, Precio_pactado

- The agreed price can differ from the current selling price (negotiated price).
### 4.13 Proveedor
Id, Id_negocio, Nombre (Razón social), Telefono, Email

- It is required before recording a purchase.
### 4.14 Cliente
Id, Id_negocio, Nombre_o_RazonSocial, Apellido,
Documento (DNI | CUIT | CUIL), Condicion_Iva,
Telefono, Email, Direccion, Fecha_alta

- Optional in sales and quotes (there can be sale without an identified customer).
- The current account is obtained by filtering sales by Id_cliente.
### 4.15 Carrito (temporary, external customers)
Id, Id_sesion (cookie), Id_producto, Cantidad, Precio_acumulado, Fecha_creacion

- Only available for businesses of type Hardware Strore or Both.
- Persists through a session cookie (does not require a customer login).
- It can be converted into a Presupuesto or Venta.
- At the end, it build a message for the WhatsApp API with the details and total.
### 4.16 MovimientoStock (inventory audit)
Id, Id_producto, Id_usuario, Id_venta (nullable), Id_compra (nullable),
Fecha, Cantidad, Tipo_movimiento

- Tipo_movimiento: VentaSalida | VentaAnulacion | CompraEntrada | CompraAnulacion | AjusteManual
- It is automatically generated in each operation that modifies the stock.
- It is NEVER modified manually from application code (insert-only).

## 5. Critical Business Rules
### 5.1 Multi-tenancy (ABSOLUTE RULE)
- Data is NEVER returned without filtering by the Id_negocio of the authenticated user.
- Id_negocio is NEVER accepted from the body/query: it is always extracted from the JWT claim.
- Every handler/repository must receive and apply the context's Id_negocio.
### 5.2 Business state
- If Negocio.Estado == Inactivo -> block authentication with a 403 and a clear message.
- Verify business status in the authentication middleware, not in each endpoint.
### 5.3 Roles and Permissions by Module
| Module / Action | Admin (Owner) | Seller |
| :---: | :---: | :---: |
| View performance reports | YES	| NO |
| View product purchase price | YES | NO |
| Cancel sales / purchases | YES | NO |
| Manage users | YES | NO |
| Open and close the cash register | YES | NO |
| Bulk price update | YES | NO |
| Manage suppliers | YES | NO |
| Register/list customers | YES | VIEW ONLY |
| Delete/modify products | YES | NO |
| Register products | YES | YES |
| Record sales | YES | YES |
| Record purchases | YES | YES |
| Manage quotes | YES | YES |
| Issue invoices | YES | YES |
| Receive stock alerts | YES | YES |
| View audit (StockMovement) | YES | NO |
### 5.4 Stock
- Stock is ALWAYS updated automatically when recording/canceling a sale or purchase.
- If esServicio == true, DO NOT deduct stock when selling.
- If Stock_actual <= Stock_minimo → issue a notification/alert (do not block the operation).
- Every stock modification generates a record in MovimientoStock.
- NEVER modify Stock_actual directly without going through the movement flow.
### 5.5 Sales
- Sales are not deleted. They are only canceled (cancellation field or status + opposite MovimientoStock).
- Requires an open cash register to record.
- Mixed payment allowed: multiple records in the Pago table, suma == Total_venta.
- Cancellation with an official invoice: a Credit Note must exist before completing the cancellation.
- The price recorded in Detalle_Venta is historical (it does not update if the product price changes).
### 5.6 Purchases
- The supplier must exist before a purchase can be recorded.
- Nro_factura_original is REQUIRED.
- Upon recording, increase stock according to Detalle_Compra.
- Cancellation reduces stock. It generates MovimientoStock of type PurchaseCancellation.
- Do not delete purchases, only cancel them.
### 5.7 Quotes
- Initial state always: Pendiente.
- the system evaluates Fecha_vencimiento on each query: if it is expired and still Pendiente → Vencido.
- Conversion to Sale:
  1. Change Estado → Aceptado.
  2. Create Sale with the quote data.
  3. Create Detalle_Venta by copying each Detalle_Presupuesto.
  4. Generate a MovimientoStock for each product (if it isn't a service).
- The agreed-upon price in the detail may differ from the current Precio_venta of the product.
### 5.8 Invoices
- An official invoice requires CAE (validated with ARCA).
- An invoice is linked to exactly one sale.
- Both Admin and Vendedor can issue invoices when the customer requests them.
- The cancellation of a sale with an official invoice BLOCKS the process until a Credit Note is issued.
### 5.9 Type of Business — Differentiated Behaviors
**Hardware Store (TipoNegocio == Ferreteria || Ambos)**
 - Enable the public catalog interface (without customer login).
 - Enable the Cart with persistence via a session cookie.
 - Enable the order completion endpoint → build a WhatsApp API message.
 - Display retail price. Purchase price and stock: Admin only.
 - Enable the product duplication function (color/size variants).
**Locksmith (TipoNegocio == Cerrajeria || Ambos)**
 - The public catalog is configurable by the owner (it can be hidden or visible).
 - If the catalog is visible: hide the purchase price and exact stock from the public.
 - Allow registering products with esServicio = true (jobs outside the premises).
 - Do not enable the customer cart or WhatsApp integration for pure locksmith services.
### 5.10 Categories
- Deactivate (Activo = false) before deleting. Do not delete if it has associated products.
- A product MUST have an active category.
- The bulk price update is performed by filtering by Id_categoria.
### 5.11 Audit
- Every relevant entity must save the Id_usuario of the last modifier.
- MovimientoStock records who changed the stock, when, how much, and why.
- Only Admins can view the audit and MovimientoStock.

## 6. Use Cases — Quick Reference
| Código | Nombre | Actores permitidos |
| :---: | :---: | :---: |
| CU-001 | Iniciar Sesión | Admin, Vendedor |
| CU-002 | Gestión de Contraseñas | Admin, Vendedor |
| CU-003 | Gestión de Usuarios | Admin |
| CU-004 | Gestionar Cajas | Admin |
| CU-005 | Registrar Venta | Admin, Vendedor |
| CU-006 | Anular Venta | Admin |
| CU-007 | Gestionar Presupuesto | Admin, Vendedor |
| CU-008 | Gestionar Productos | Admin (full), Vendedor (registrar/listar) |
| CU-009 | Control y Alerta de Stock | Sistema → Admin, Vendedor |
| CU-010 | Actualización Masiva de Precios | Admin |
| CU-011 | Gestionar Categorías | Admin |
| CU-012 | Registrar Compra | Admin, Vendedor |
| CU-013 | Gestionar Proveedores | Admin |
| CU-014 | Anular Compra | Admin |
| CU-015 | Navegación y Carrito | Cliente (público, Ferretería) |
| CU-016 | Finalizar Pedido WhatsApp | Cliente (requiere carrito) |
| CU-017 | Gestionar Clientes / Cta Cte | Admin (full), Vendedor (solo ver) |
| CU-018 | Auditoría | Admin |
| CU-019 | Visualizar Informes | Admin (Dueño) únicamente |

## 7. REST API Conventions
**Routes**
- ALWAYS plural and versioned: api/v1/{resource}
- ALWAYS hierarchical for nested resources
Correct examples:
  GET    api/v1/productos
  GET    api/v1/productos/{id}
  POST   api/v1/productos
  PUT    api/v1/productos/{id}
  DELETE api/v1/productos/{id}
  GET    api/v1/ventas/{id}/detalles
  GET    api/v1/ventas/{id}/pagos
  GET    api/v1/compras/{id}/detalles
  GET    api/v1/presupuestos/{id}/detalles
  POST   api/v1/ventas/{id}/anular
  POST   api/v1/presupuestos/{id}/convertir
  GET    api/v1/productos/{id}/movimientos
- DO NOT use verbs in the path: /canularVenta, /getProducto, /crearFactura.
- DO NOT use singular: /product, /sale, /user
**HTTP Verbs y Status Codes**
GET    → 200 OK | 404 Not Found
POST   → 201 Created (con Location header) | 400 Bad Request | 422 Unprocessable
PUT    → 200 OK | 404 Not Found | 400 Bad Request
PATCH  → 200 OK | 404 Not Found
DELETE → 204 No Content | 404 Not Found
POST (acción) → 200 OK | 404 Not Found | 409 Conflict

## 8. CQRS structure with MediatR
Application/
├── Productos/
│   ├── Commands/
│   │   ├── CreateProducto/
│   │   │   ├── CreateProductoCommand.cs
│   │   │   ├── CreateProductoHandler.cs
│   │   │   └── CreateProductoValidator.cs
│   │   ├── UpdateProducto/
│   │   └── DeleteProducto/
│   └── Queries/
│       ├── GetProductoById/
│       │   ├── GetProductoByIdQuery.cs
│       │   └── GetProductoByIdHandler.cs
│       └── GetAllProductos/
├── Ventas/
├── Compras/
├── Presupuestos/
├── Facturas/
├── Usuarios/
├── Clientes/
├── Proveedores/
├── Categorias/
├── Caja/
├── Carrito/
└── Behaviours/
    ├── ValidationBehaviour.cs
    └── AuthorizationBehaviour.cs
**Rules of Commands/Queries:**
```csharp
// ✅ The Id_negocio NEVER comes from the body — it is extracted from the JWT in the handler
public record CreateProductoCommand(
    string Nombre,
    string Descripcion,
    decimal PrecioCompra,
    decimal PrecioVenta,
    int StockActual,
    int StockMinimo,
    int IdCategoria,
    bool EsServicio
) : IRequest<ProductoResponse>;

// ✅ Handler extracts business from context
public class CreateProductoHandler(
    IUnitOfWork uow,
    ICurrentUserService currentUser,  // extracts Id_negocio and Role from the JWT
    IMapper mapper)
    : IRequestHandler<CreateProductoCommand, ProductoResponse>
{
    public async Task<ProductoResponse> Handle(CreateProductoCommand cmd, CancellationToken ct)
    {
        var negocio = await uow.Negocios.GetByIdAsync(currentUser.NegocioId, ct)
            ?? throw new NotFoundException(nameof(Negocio), currentUser.NegocioId);

        if (negocio.Estado == EstadoNegocio.Inactivo)
            throw new NegocioInactivoException();

        var categoria = await uow.Categorias.GetByIdAsync(cmd.IdCategoria, ct)
            ?? throw new NotFoundException(nameof(Categoria), cmd.IdCategoria);

        if (categoria.IdNegocio != currentUser.NegocioId)
            throw new UnauthorizedAccessException();

        var producto = Producto.Create(
            currentUser.NegocioId, currentUser.UserId,
            cmd.Nombre, cmd.PrecioCompra, cmd.PrecioVenta,
            cmd.StockActual, cmd.StockMinimo, cmd.IdCategoria, cmd.EsServicio);

        await uow.Productos.AddAsync(producto, ct);
        await uow.SaveChangesAsync(ct);
        return mapper.Map<ProductoResponse>(producto);
    }
}
```

## 9. Validations with FluentValidation
- A validator per Command/Query (same namespace).
- No Data Annotations in Commands, DTOs or domain entities.
- MediatR's ValidationBehaviour validates BEFORE the handler executes.
- If validation fails → throw ValidationException → GlobalExceptionHandler → 400 Bad Request.
```csharp
// ✅ Example of a validator for a business domain
public class CreateVentaCommandValidator : AbstractValidator<CreateVentaCommand>
{
    public CreateVentaCommandValidator()
    {
        RuleFor(x => x.Detalles)
            .NotEmpty().WithMessage("La venta debe tener al menos un producto.");

        RuleForEach(x => x.Detalles).ChildRules(detalle =>
        {
            detalle.RuleFor(d => d.IdProducto).GreaterThan(0);
            detalle.RuleFor(d => d.Cantidad).GreaterThan(0)
                .WithMessage("La cantidad debe ser mayor a cero.");
            detalle.RuleFor(d => d.PrecioUnitario).GreaterThan(0);
        });

        RuleFor(x => x.Pagos)
            .NotEmpty().WithMessage("Debe registrarse al menos un método de pago.");

        RuleFor(x => x.Pagos)
            .Must((cmd, pagos) => pagos.Sum(p => p.Monto) == cmd.Total)
            .WithMessage("La suma de pagos debe coincidir con el total de la venta.");
    }
}
```

## 10. Error Handling — GlobalExceptionHandler
```csharp
// ✅ Domain exceptions defined in Domain/Exceptions/
public class NotFoundException(string entidad, object clave)
    : Exception($"{entidad} con clave '{clave}' no fue encontrado.");

public class DomainException(string message) : Exception(message);

public class NegocioInactivoException()
    : Exception("El negocio se encuentra inactivo. Contacte al administrador.");

public class StockInsuficienteException(string producto)
    : DomainException($"Stock insuficiente para el producto: {producto}.");

public class CajaNoAbiertaException()
    : DomainException("Debe abrir la caja antes de registrar una venta.");
```
- GlobalExceptionHandler maps exceptions to standard RFC 9457 ProblemDetails.
- Include traceId in the error response for log correlation.
- Always log in with iLogger before replying.
- NEVER expose stack traces in production.
Mapa de excepciones → HTTP status:
  NotFoundException           → 404 Not Found
  DomainException             → 422 Unprocessable Entity
  NegocioInactivoException    → 403 Forbidden
  ValidationException         → 400 Bad Request
  UnauthorizedAccessException → 401 Unauthorized
  Exception (cualquier otro)  → 500 Internal Server Error

## 11. JWT Authentication
```json
// ✅ Claims required in the token
{
  "sub":       "uuid-del-usuario",
  "email":     "user@example.com",
  "rol":       "Admin | Vendedor",
  "negocioId": "id-del-negocio",
  "exp":       1234567890
}
```
```csharp
// ✅ ICurrentUserService — available throughout the Application layer
public interface ICurrentUserService
{
    int    UserId    { get; }
    int    NegocioId { get; }
    string Rol       { get; }
    bool   IsAdmin   => Rol == "Admin";
}
```
- Verify Negocio.Estado == Activo upon authentication.
- Use [Authorize] on all private controllers.
- Use [Authorize(Roles = "Admin")] for owner-only endpoints.
- [AllowAnonymous] only in: Login, public catalog (hardware store), carrito and pedido de WhatsApp.
- NEVER include Id_negocio in the body of an authenticated request.
- NEVER trust the Id_negocio sent by the client. Always use the one from the JWT.

## 12. Swagger
```csharp
// ✅ Minimum configuration required
options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
{
    Name         = "Authorization",
    Type         = SecuritySchemeType.Http,
    Scheme       = "Bearer",
    BearerFormat = "JWT",
    In           = ParameterLocation.Header,
    Description  = "Ingrese el token JWT. Ejemplo: eyJhbG..."
});
// Apply globally to all endpoints
options.AddSecurityRequirement(...);
```

## 13. Pipeline order in Program.cs
```csharp
// ✅ Order matters — always respect it
app.UseExceptionHandler();     // 1. Capture all errors
app.UseHttpsRedirection();     // 2. Force HTTPS
app.UseAuthentication();       // 3. Who are you?
app.UseAuthorization();        // 4. What can you do?
app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();
```

## 14. Anti-Patterns — Forbidden
- Business logic in Controllers — always in Application handlers.
- Accessing DbContext directly from Application — only through interfaces/repositories.
- Return domain entities (EF entities) directly from endpoints — always map to DTO.
- Using .Result or .Wait() in async code — always await + CancellationToken.
- Use Data Annotations in commands or DTOs — FluentValidation only.
- Use AutoMapper — use Mapster (it's free).
- Unique or versionless routes: /product, /api/getSale.
- Hardcode Id_negocio in code — always from JWT claim.
- To remove sales or purchases from the database — always cancel.
- Modify Stock_actual directly without generating MovimientoStock.
- Display Precio_compra to users with the Vendedor role.
- Enable public shopping carrito/catalog for businesses like pure locksmithing.

## 15. Code Generation Rules
- When you generate code for this project:
 1. Always create Command/Query + Handler + Validator in the same namespace and folder.
 2. The handler always receives ICurrentUserService to obtain Id_negocio and Rol.
 3. Always verify that the entity belongs to the same Id_negocio as the authenticated user.
 4. Always use async/await with CancellationToken ct in all I/O methods.
 5. Always map entities to DTOs with Mapster before returning from the handler.
 6. Always throw typed domain exceptions (never throw new Exception("message")).
 7. Always use IUnitOfWork to persist changes, never use DbContext directly in Application.
 8. Whenever stock is modified, generate a record in MovimientoStock.
 9. Always decorate endpoints with [ProducesResponseType] to document status codes.
 10. Never generate automatic migrations — propose the migration and wait for confirmation.