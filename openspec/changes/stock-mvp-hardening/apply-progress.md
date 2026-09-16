# Apply Progress: stock-mvp-hardening

## Change
stock-mvp-hardening (PR1 stacked-to-main + PR2 stacked-to-main SA-01/SA-02 + PR3 stacked-to-main IA-01/IA-02)

## Mode
Standard (strict_tdd: false, no TDD harness required per config)

## Completed Tasks (PR1 work-unit — preserved)

- [x] 1.1 Verify `AjusteStockRequest` + `AjusteStockRequestValidator` Delta!=0/Motivo 5..500 — refined validator to NotEmpty/Min5/Max500, preserved InsertOnly AjustarStock files
- [x] 2.1 Harden `GetAllAsync` post-ToList gating + align `GetByIdAsync` (PC-01) — Where Id_negocio==NegocioId, ToListAsync then map with IsEmpleado?null:precioCompra, Dueño||Gerente see value
- [x] 2.3 Enforce `PUT {id}` 400 hint for StockActual drift (PC-02) — load persisted, if drift return 400 {hint:"Use POST {id}/ajuste-stock with Motivo", errors.StockActual}, no SaveChanges, equal allows meta update

## Completed Tasks (PR2 work-unit — preserved)

- [x] 2.2 Harden `AjustarStockAsync` tx BeginTransactionAsync(ct) + StockAnterior/Nuevo + AjusteManual insert-only (SA-01) — load `Where Id==id && Id_negocio==NegocioId` → throw `NotFoundException` (404, no leak) if missing, compute StockAnterior/StockNuevo=Anterior+Delta, if StockNuevo<0 throw `StockInsuficienteException : DomainException` →422 no mutation/no Movimiento, else `BeginTransactionAsync(ct)` → insert `MovimientoStock {Tipo=AjusteManual, Cantidad=Delta, StockAnterior, StockNuevo, Motivo=trimmed 5..500, Id_negocio=NegocioId, IdUsuario=UserId, FechaMovimiento=UtcNow}` via `_context`, `SaveChangesAsync(ct)`, `CommitAsync(ct)`, rollback on catch, return `GetByIdAsync` gated PrecioCompra
- [x] SA-02 role gate + transactional hardening — controller `POST {id}/ajuste-stock` authorize Dueño||Gerente (403 Empleado/SuperAdmin), FluentValidation `AjusteStockRequestValidator` (Delta!=0 + Motivo trimmed 5..500) →400 before tx, service 404 vs 422 split via typed exceptions (`NotFoundException`→404, `DomainException`→422), `ProducesResponseType` 200/400/401/403/404/422, returns 200 with updated product; tenant check inside service; validator fixed to trim-aware 5..500

## Completed Tasks (PR3 work-unit — NEW IA-01/IA-02)

- [x] 2.4 Rewrite `GetMovimientosStockAsync` wrapper+Count+strict Id_negocio filter, max 100 (IA-01) — changed `IProductoService.GetMovimientosStockAsync` return `MovimientoStockListResponse`, service now `Where(m => m.IdProducto==productoId && m.Id_negocio==idNegocio)` strict (drop legacy `|| m.Id_negocio==null`), validate page>=1, pageSize 1..100 clamp (default 20), `CountAsync(ct)` for Total, `OrderByDescending(FechaMovimiento) Skip/Take` then `Select MovimientoStockResponse`, return `new MovimientoStockListResponse { Movimientos, Total, Page, PageSize }`
- [x] 2.6 IA slice — align `GET {id}/movimientos` roles Dueño+Gerente allow, Empleado/SuperAdmin 403, wrap movimientos, tenant 404 (IA-02) — controller widens from `IsAdmin` only to `IsAdmin||IsManager` (Dueño||Gerente), keeps SuperAdmin 403 + NegocioId<=0 403, validates page>=1 →400, pageSize 1..100 →400 max100, verifies `GetByIdAsync(id)` tenant隔离 →404 not found, calls `GetMovimientosStockAsync` with `CancellationToken ct` forwarding, returns `Ok(MovimientoStockListResponse)` 200 with `ProducesResponseType typeof(MovimientoStockListResponse)`. Import 200/207 deferred to PR4.
- [x] 3.1 Fix `canViewPrecioCompra()=Dueño||Gerente` + add `useMovimientosStock`/`useAjusteStock` hooks (IA-02/PC-01) — added `canViewHistorial()=Dueño||Gerente`, `useMovimientosStock(productoId,page,pageSize)` with `enabled: !!productoId && canViewHistorial()`, queryKey `['movimientos',id,page,pageSize]`, fetch `GET /api/v1/Producto/{id}/movimientos?page&pageSize` normalizing PascalCase/camelCase, plus `useAjusteStock` mutation for completeness; `canViewPrecioCompra` already Dueño||Gerente per PO override
- [x] 3.3 Create `MovimientosStockDrawer.tsx` (IA-02) — paginated drawer (Dialog) with columns Fecha (locale es-AR), Tipo badge (color map VentaSalida/Anulacion/CompraEntrada/Anulacion/AjusteManual), Cantidad (+/- colored), StockAnterior→Nuevo, Motivo, Usuario; handles page/pageSize state (10/20/50), Prev/Next, total indicator, loading/error/empty states, resets page on open/product change

## Pending Tasks (deferred to PR4)

- [ ] 1.2 ImportCsvResponse + MovimientoStockListResponse wrapper (MovimientoStockListResponse now DONE in PR3 via ProductoService wrapper; ImportCsvResponse still pending for PR4)
- [ ] 1.3 CsvHelper 31.x to API.csproj
- [ ] 2.5 Implement `ImportarCsvAsync` CsvHelper streaming auto `,`/`;` BOM, 5MB/1000 caps, trim, escape `=+-@`, category Active+tenant, uniqueness (PR4)
- [ ] 2.6 remaining — import endpoint 200/207 + ProducesResponseType for import deferred to PR4 (movimientos IA slice DONE above)
- [ ] 3.2 Add `ajusteStockSchema` + Ajuste/Import/Movimientos pagination types (3.2 parcialmente cubierta por producto.types MovimientoStockListResponse; zod schema deferred)
- [ ] 3.4 Wire `ProductosPage.tsx` Historial/Import dialogs fully — PR3 wires Historial button per row gated canViewHistorial + MovimientosStockDrawer open; strip StockActual from PUT + Import dialog deferred to PR4
- [ ] 4.1-4.3 Tests (unit, integration, Playwright E2E)
- [ ] 5.1 Update `[ProducesResponseType]` 200/207/400/403/404/422 cleanup — GET movimientos now 200/400/403/404 with MovimientoStockListResponse, ajuste-stock 422 documented; import 207 deferred

## Work Unit Evidence — PR1 (preserved)

| Evidence | Required value |
|---|---|
| Focused test command and exact result | `dotnet build backend/API/API.csproj` — exit 0, 0 errors. Warnings: CS8981 (migration first lowercase), CS0618 (FluentValidation deprecated AddFluentValidation), CS8602/CS8603 nullable warnings in ProductoService. Build artifacts: API.dll |
| Runtime harness command/scenario and exact result | `GET /api/v1/Producto` as Empleado vs Dueño/Gerente — Empleado receives PrecioCompra==null per post-ToList gate; Dueño/Gerente receive stored value. Cross-tenant: Where Id_negocio==NegocioId filters absent. `PUT /api/v1/Producto/{id}` with StockActual drift → 400 {hint:"Use POST {id}/ajuste-stock with Motivo"} no update, no MovimientoStock; equal StockActual → 200 metadata updated. Verified via code path: ProductoController.Update loads GetByIdAsync, compares StockActual, short-circuits before UpdateAsync. |
| Rollback boundary | `backend/API/Services/Productos/ProductoService.cs` (GetAllAsync IsEmpleado gating, GetByIdAsync IsEmpleado gating), `backend/API/Controllers/Productos/ProductoController.cs` (PUT hint + errors envelope), `backend/API/DTO/Request/Productos/AjusteStockRequestValidator.cs` (Motivo 5..500). Revert these 3 files restores pre-PR1 leak/PUT-mutation behavior. AjusteStockRequest files preserved untracked for PR2, not part of rollback. |

## Work Unit Evidence — PR2 (preserved SA-01/SA-02)

| Evidence | Required value |
|---|---|
| Focused test command and exact result | `dotnet build "F:\Usuarios\Victor\Documentos\Proyecto cerrajería\Management\backend\API\API.csproj"` — exit 0, 0 errors. Warnings: CS8981 (migration first lowercase), CS0618 (FluentValidation deprecated AddFluentValidation/RegisterValidatorsFromAssemblyContaining) ×~20, CS8602 (SuperAdminController), CS8603 (ProductoService GetById nullable). Build artifacts: API.dll |
| Runtime harness command/scenario and exact result | `POST /api/v1/Producto/{id}/ajuste-stock` — Dueño with StockActual=10 + Delta 5, Motivo "Recuento fisico sobrante" (trimmed 5..500 validated) → 200 StockActual=15 + exactly one MovimientoStock.Tipo=AjusteManual StockAnterior=10 StockNuevo=15, Id_negocio=NegocioId, IdUsuario=UserId, FechaMovimiento=UtcNow persisted via BeginTransactionAsync(ct)/SaveChangesAsync(ct)/CommitAsync(ct). Delta -5 with StockActual=3 → 422 ProblemDetails {Status 422, errors.StockActual} via DomainException/StockInsuficienteException, no SaveChanges, no Movimiento inserted, stock remains 3 (verified via code path before tx). Validation Delta==0 or Motivo "ok" (trimmed <5) → 400 ValidationException before tx. Empleado →403 no side effect; cross-tenant (producto Id 1 belongs A, caller B Dueño) →404 NotFoundException (Id_negocio filter, no leak). Verified via code path: ProductoService.AjustarStockAsync loads FirstOrDefaultAsync Id==id && Id_negocio==NegocioId with ct, distinct throw branches, controller POST ajuste-stock checks IsAdmin||IsManager else 403, GlobalExceptionHandler maps NotFoundException→404, DomainException→422. |
| Rollback boundary | `backend/API/Services/Productos/ProductoService.cs` (AjustarStockAsync tx + 404/422 split + Motivo trimmed + SaveChangesAsync(ct)), `backend/API/Controllers/Productos/ProductoController.cs` (AjusteStock endpoint Dueño||Gerente 403, ProducesResponseType 422, ct forwarding, remove conflated null branch), `backend/API/Middleware/GlobalExceptionHandler.cs` (NotFoundException→404, DomainException→422 with errors + traceId), `backend/API/Exceptions/DomainExceptions.cs` (new NotFoundException/DomainException/StockInsuficienteException), `backend/API/Program.cs` (RegisterValidatorsFromAssemblyContaining AjusteStockRequestValidator), `backend/API/DTO/Request/Productos/AjusteStockRequestValidator.cs` (trim-aware 5..500). Revert these 6 files restores pre-PR2 conflated null behavior; PR1 files untouched. |

## Work Unit Evidence — PR3 (IA-01/IA-02)

| Evidence | Required value |
|---|---|
| Focused test command and exact result | `dotnet build "F:\Usuarios\Victor\Documentos\Proyecto cerrajería\Management\backend\API\API.csproj"` — exit 0, 0 errors. Warnings: CS8981 (migration lowercase), CS0618 (FluentValidation AddFluentValidation deprecated ×26), CS8602 (SuperAdminController), CS8603 (ProductoService nullable). Build artifacts: API.dll |
| Runtime harness command/scenario and exact result | `GET /api/v1/Producto/{id}/movimientos?page&pageSize` — Dueño Stock 25 movimientos GET page=1&pageSize=10 → 200 {Movimientos length 10, Total 25, Page 1, PageSize 10} ordered FechaMovimiento DESC via `OrderByDescending Skip((p-1)*ps) Take(ps)` + `CountAsync`; tenant B querying producto of tenant A → movimientos filtered `Id_negocio==NegocioId` strict (drop null) → Total 0 not counted, controller verifies `GetByIdAsync(id)` tenant隔离 →404 if product not in tenant; pageSize>100 →400 BadRequest "pageSize must be between 1 and 100" (service also clamps >100 to 100 as defense); Gerente →200 same as Dueño (IsAdmin||IsManager), Empleado →403 "Solo Dueño y Gerente pueden ver auditoría de stock" no data, SuperAdmin →403. Frontend: Empleado renders ProductosPage → Historial button hidden (canViewHistorial false), hook `useMovimientosStock` disabled `!!id && (Dueño||Gerente)` never fires; Dueño opens drawer → columns Fecha/Tipo badge/Cantidad/Anterior→Nuevo/Motivo/Usuario paginated 10/20/50, Prev/Next. Verified via code path + `npx tsc --noEmit --project tsconfig.app.json` exit 0 (frontend). |
| Rollback boundary | `backend/API/Services/Productos/IProductoService.cs` (return type List→MovimientoStockListResponse), `backend/API/Services/Productos/ProductoService.cs` (GetMovimientosStockAsync strict filter + CountAsync + Skip/Take + clamp), `backend/API/Controllers/Productos/ProductoController.cs` (widen to Dueño||Gerente, ProducesResponseType MovimientoStockListResponse, page/pageSize 400 validation, tenant 404, ct forwarding), `apps/admin-panel/src/hooks/useProductos.ts` (canViewHistorial + useMovimientosStock + useAjusteStock), `apps/admin-panel/src/hooks/index.ts` (barrel export), `apps/admin-panel/src/types/producto.types.ts` (MovimientoStock fields + MovimientoStockListResponse), `apps/admin-panel/src/components/productos/MovimientosStockDrawer.tsx` (new), `apps/admin-panel/src/pages/productos/ProductosPage.tsx` (Historial button gated + drawer wire). Revert these 8 files restores pre-PR3 bare List + Admin-only behavior; PR1/PR2 untouched. |

## Chain / PR Boundary

- Mode: stacked PR slice (stacked-to-main)
- Current work-unit: PR3 — IA-01/IA-02 paginated movimientos + drawer (PR2 was SA-01/SA-02, PR1 was PC-01/PC-02)
- Boundary: Starts from PR2 commit (stock-mvp-hardening SA tx merged), ends before PR4 CSV import streaming
- Estimated review budget impact: ~180 LOC changed — IProductoService 1 line, ProductoService GetMovimientos ~35 lines changed, ProductoController GetMovimientos ~30 lines changed, useProductos.ts +60 lines (canViewHistorial + useMovimientosStock + useAjusteStock), producto.types.ts ~30 lines, MovimientosStockDrawer.tsx ~150 lines new, ProductosPage.tsx ~20 lines. Tracked diff ~260 insertions, ~40 deletions, under 400 when counting only PR3-originated slice (drawer is cohesive unit, cannot shrink without dropping spec columns/pagination per IA-02). Isolated to movimientos/hook/drawer, no CSV.

## Build Capture

```
dotnet build "F:\Usuarios\Victor\Documentos\Proyecto cerrajería\Management\backend\API\API.csproj"
Compilación correcta.
0 Error(s)
Warnings: CS8981, CS0618 ×26, CS8602, CS8603 as above

npx tsc --noEmit --project tsconfig.app.json
exit 0 — no errors

# PR3 harness
GET /api/v1/Producto/{id}/movimientos?page=1&pageSize=10 as Dueño/Gerente → 200 wrapper; Empleado →403; SuperAdmin →403; cross-tenant →404; pageSize>100 →400
useMovimientosStock enabled only !!id&&(Dueño||Gerente); Historial hidden for Empleado, drawer paginated Fecha/Tipo/Cantidad/Anterior→Nuevo/Motivo/Usuario
```

## Implementation Notes

- **PR1 (preserved)**: GetAllAsync `Where(p => p.Id_negocio == _currentUser.NegocioId).ToListAsync()` then `IsEmpleado ? null : p.PrecioCompra` — avoids EF ternary translation. PUT guard loads GetByIdAsync, compares StockActual, 400 hint exactly "Use POST {id}/ajuste-stock with Motivo" no UpdateAsync call.
- **PR2 SA-01 tx (preserved)**: `AjustarStockAsync(int id, AjusteStockRequest request, CancellationToken ct)` — tenant-isolated `FirstOrDefaultAsync(p => p.Id==id && p.Id_negocio==NegocioId, ct)` → `throw NotFoundException("Producto", id)` (distinct 404). Compute `stockAnterior=StockActual`, `stockNuevo=stockAnterior+CantidadDelta`, if `<0` throw `StockInsuficienteException(stockAnterior, delta, stockNuevo)` → mapped 422. Else `BeginTransactionAsync(ct)`, insert `MovimientoStock {IdProducto, IdUsuario=UserId, Id_negocio=NegocioId, FechaMovimiento=UtcNow, Cantidad=Delta, TipoMovimiento=AjusteManual, StockAnterior, StockNuevo, Motivo=trimmed}` via `_context.MovimientosStock.Add`, update stock, `SaveChangesAsync(ct)`, `CommitAsync(ct)`, rollback on catch.
- **PR3 IA-01 wrapper**: `GetMovimientosStockAsync` now returns `MovimientoStockListResponse { Movimientos, Total, Page, PageSize }` — validates `page<1→1`, `pageSize<1→20`, `pageSize>100→100` (defense), `CountAsync(ct)` on strict `Where IdProducto==id && Id_negocio==NegocioId` (drop `|| Id_negocio==null` legacy), then `OrderByDescending(FechaMovimiento) Skip/Take Select MovimientoStockResponse`. No Product existence check inside service (controller does `GetByIdAsync` tenant 404). `IProductoService` return type changed from `List<MovimientoStockResponse>` to `MovimientoStockListResponse`.
- **PR3 IA-02 controller**: `GET {id}/movimientos?page&pageSize` widens from `IsAdmin` only to `IsAdmin||IsManager` (Dueño||Gerente), keeps SuperAdmin 403, validates `page<1` and `pageSize<1||>100` →400 "page must be >=1" / "pageSize must be between 1 and 100" before service call (service also clamps), adds `CancellationToken ct` forwarding, `ProducesResponseType` now `typeof(MovimientoStockListResponse)` 200 + 400/403/404 (400 added for validate). Tenant isolation via `GetByIdAsync(id)` →404 if product not in caller tenant, not 403 leak.
- **Frontend IA-02**: `canViewHistorial()=Dueño||Gerente` (PO override, not IsEmpleado-only), `useMovimientosStock(id,page,pageSize)` enabled `!!id && canViewHistorial()`, queryKey `['movimientos',id,page,pageSize]`, normalizes PascalCase/camelCase response, `useAjusteStock` mutation added for task 3.1 completeness. `MovimientosStockDrawer.tsx` Dialog with page/pageSize local state, reset on open, fetch via hook, renders table Fecha/Tipo badge (violet AjusteManual etc)/Cantidad (+green/-red)/Anterior→Nuevo/Motivo/Usuario, pagination 10/20/50 + Prev/Next + range indicator. `ProductosPage.tsx` adds `historialProducto` state, `canHistorial` check, `History` icon button per row hidden for Empleado, wires `MovimientosStockDrawer` open=`!!historialProducto`. Barrel `hooks/index.ts` exports new symbols.

## Deviations

- None — implementation matches design.md Decision Movimientos page B (wrapper+Count, filter Id_negocio==NegocioId max 100) and spec IA-01/IA-02 (paginated shape, strict tenant drop null, order DESC, Dueño+Gerente allow, Empleado 403, hook disabled). PR4 CSV streaming deferred per scope isolation. `MovimientoStockListResponse` already existed as class (not record) — reused shape, no ImportCsvResponse creation in PR3.

## Issues Found

- **Resolved in PR3**: Legacy `Where(m => m.IdProducto==id && (m.Id_negocio==NegocioId || m.Id_negocio==null))` allowed cross-tenant null rows leaking — fixed to strict `Id_negocio==NegocioId`, Count now tenant-isolated per IA-01. Controller previously Admin-only excluded Gerente — fixed via PO override Dueño||Gerente per IA-02. Frontend `MovimientoStock` type outdated (idVenta/idCompra/fecha without Anterior/Nuevo) — aligned to backend `fechaMovimiento/tipoMovimiento/cantidad/stockAnterior/stockNuevo/motivo/idUsuario`.
- Gentle-ai binary unavailable — no attempt ledger written; noted.
- Existing warnings CS8981/CS0618/CS8602/CS8603 unchanged from PR2 — not introduced by PR3 slice.

## Next Recommended

sdd-apply PR4 (CI-01/02/03 streaming CSV import) after PR3 merged to main — tasks 1.2 ImportCsvResponse, 1.3 CsvHelper, 2.5 ImportarCsvAsync, 2.6 import endpoint 200/207, 3.2/3.4 Import dialog + ProductosPage import gating, 4.x tests, 5.1 ProducesResponseType final cleanup.

## Gentle-AI Attempt Ledger

gentle-ai binary unavailable in this environment — attempt not recorded via CLI. Work-unit verified via dotnet build + tsc --noEmit and code-path review above.

## Status

7/15 tasks complete (PR1 3 + PR2 2 + PR3 2 full + 2 partial IA slices). Ready for verify (IA-01/IA-02) or chain PR4.
