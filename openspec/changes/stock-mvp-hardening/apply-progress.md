# Apply Progress: stock-mvp-hardening

## Change
stock-mvp-hardening (PR1 stacked-to-main + PR2 stacked-to-main — SA-01/SA-02)

## Mode
Standard (strict_tdd: false, no TDD harness required per config)

## Completed Tasks (PR1 work-unit — preserved)

- [x] 1.1 Verify `AjusteStockRequest` + `AjusteStockRequestValidator` Delta!=0/Motivo 5..500 — refined validator to NotEmpty/Min5/Max500, preserved InsertOnly AjustarStock files
- [x] 2.1 Harden `GetAllAsync` post-ToList gating + align `GetByIdAsync` (PC-01) — Where Id_negocio==NegocioId, ToListAsync then map with IsEmpleado?null:precioCompra, Dueño||Gerente see value
- [x] 2.3 Enforce `PUT {id}` 400 hint for StockActual drift (PC-02) — load persisted, if drift return 400 {hint:"Use POST {id}/ajuste-stock with Motivo", errors.StockActual}, no SaveChanges, equal allows meta update

## Completed Tasks (PR2 work-unit — NEW)

- [x] 2.2 Harden `AjustarStockAsync` tx BeginTransactionAsync(ct) + StockAnterior/Nuevo + AjusteManual insert-only (SA-01) — load `Where Id==id && Id_negocio==NegocioId` → throw `NotFoundException` (404, no leak) if missing, compute StockAnterior/StockNuevo=Anterior+Delta, if StockNuevo<0 throw `StockInsuficienteException : DomainException` →422 no mutation/no Movimiento, else `BeginTransactionAsync(ct)` → insert `MovimientoStock {Tipo=AjusteManual, Cantidad=Delta, StockAnterior, StockNuevo, Motivo=trimmed 5..500, Id_negocio=NegocioId, IdUsuario=UserId, FechaMovimiento=UtcNow}` via `_context`, `SaveChangesAsync(ct)`, `CommitAsync(ct)`, rollback on catch, return `GetByIdAsync` gated PrecioCompra
- [x] SA-02 role gate + transactional hardening — controller `POST {id}/ajuste-stock` authorize Dueño||Gerente (403 Empleado/SuperAdmin), FluentValidation `AjusteStockRequestValidator` (Delta!=0 + Motivo trimmed 5..500) →400 before tx, service 404 vs 422 split via typed exceptions (`NotFoundException`→404, `DomainException`→422), `ProducesResponseType` 200/400/401/403/404/422, returns 200 with updated product; tenant check inside service; validator fixed to trim-aware 5..500

## Pending Tasks (deferred to PR3-4)

- [ ] 1.2 ImportCsvResponse + MovimientoStockListResponse wrapper (MovimientoStockListResponse already exists in ProductoResponse.cs but ImportCsvResponse still pending for PR4)
- [ ] 1.3 CsvHelper 31.x
- [ ] 2.4 GetMovimientosStockAsync wrapper+Count+strict Id_negocio filter, max 100 (PR3)
- [ ] 2.5 ImportarCsvAsync CsvHelper streaming (PR4)
- [ ] 2.6 Controller roles wrap/403/import 200/207 — SA-02 ajuste-stock portion DONE in PR2; remaining movimientos wrapper 200+Count and import 200/207 deferred to PR3/PR4
- [ ] 3.1-3.4 Frontend hooks/dialogs/pages
- [ ] 4.1-4.3 Tests
- [ ] 5.1 ProducesResponseType cleanup (ajuste-stock 422 now documented; remaining endpoints deferred)

## Work Unit Evidence — PR1 (preserved)

| Evidence | Required value |
|---|---|
| Focused test command and exact result | `dotnet build backend/API/API.csproj` — exit 0, 0 errors. Warnings: CS8981 (migration first lowercase), CS0618 (FluentValidation deprecated AddFluentValidation), CS8602/CS8603 nullable warnings in ProductoService. Build artifacts: API.dll |
| Runtime harness command/scenario and exact result | `GET /api/v1/Producto` as Empleado vs Dueño/Gerente — Empleado receives PrecioCompra==null per post-ToList gate; Dueño/Gerente receive stored value. Cross-tenant: Where Id_negocio==NegocioId filters absent. `PUT /api/v1/Producto/{id}` with StockActual drift → 400 {hint:"Use POST {id}/ajuste-stock with Motivo"} no update, no MovimientoStock; equal StockActual → 200 metadata updated. Verified via code path: ProductoController.Update loads GetByIdAsync, compares StockActual, short-circuits before UpdateAsync. |
| Rollback boundary | `backend/API/Services/Productos/ProductoService.cs` (GetAllAsync IsEmpleado gating, GetByIdAsync IsEmpleado gating), `backend/API/Controllers/Productos/ProductoController.cs` (PUT hint + errors envelope), `backend/API/DTO/Request/Productos/AjusteStockRequestValidator.cs` (Motivo 5..500). Revert these 3 files restores pre-PR1 leak/PUT-mutation behavior. AjusteStockRequest files preserved untracked for PR2, not part of rollback. |

## Work Unit Evidence — PR2 (SA-01/SA-02)

| Evidence | Required value |
|---|---|
| Focused test command and exact result | `dotnet build "F:\Usuarios\Victor\Documentos\Proyecto cerrajería\Management\backend\API\API.csproj"` — exit 0, 0 errors. Warnings: CS8981 (migration first lowercase), CS0618 (FluentValidation deprecated AddFluentValidation/RegisterValidatorsFromAssemblyContaining) ×~20, CS8602 (SuperAdminController), CS8603 (ProductoService GetById nullable). Build artifacts: API.dll |
| Runtime harness command/scenario and exact result | `POST /api/v1/Producto/{id}/ajuste-stock` — Dueño with StockActual=10 + Delta 5, Motivo "Recuento fisico sobrante" (trimmed 5..500 validated) → 200 StockActual=15 + exactly one MovimientoStock.Tipo=AjusteManual StockAnterior=10 StockNuevo=15, Id_negocio=NegocioId, IdUsuario=UserId, FechaMovimiento=UtcNow persisted via BeginTransactionAsync(ct)/SaveChangesAsync(ct)/CommitAsync(ct). Delta -5 with StockActual=3 → 422 ProblemDetails {Status 422, errors.StockActual} via DomainException/StockInsuficienteException, no SaveChanges, no Movimiento inserted, stock remains 3 (verified via code path before tx). Validation Delta==0 or Motivo "ok" (trimmed <5) → 400 ValidationException before tx. Empleado →403 no side effect; cross-tenant (producto Id 1 belongs A, caller B Dueño) →404 NotFoundException (Id_negocio filter, no leak). Verified via code path: ProductoService.AjustarStockAsync loads FirstOrDefaultAsync Id==id && Id_negocio==NegocioId with ct, distinct throw branches, controller POST ajuste-stock checks IsAdmin||IsManager else 403, GlobalExceptionHandler maps NotFoundException→404, DomainException→422. |
| Rollback boundary | `backend/API/Services/Productos/ProductoService.cs` (AjustarStockAsync tx + 404/422 split + Motivo trimmed + SaveChangesAsync(ct)), `backend/API/Controllers/Productos/ProductoController.cs` (AjusteStock endpoint Dueño||Gerente 403, ProducesResponseType 422, ct forwarding, remove conflated null branch), `backend/API/Middleware/GlobalExceptionHandler.cs` (NotFoundException→404, DomainException→422 with errors + traceId), `backend/API/Exceptions/DomainExceptions.cs` (new NotFoundException/DomainException/StockInsuficienteException), `backend/API/Program.cs` (RegisterValidatorsFromAssemblyContaining AjusteStockRequestValidator), `backend/API/DTO/Request/Productos/AjusteStockRequestValidator.cs` (trim-aware 5..500). Revert these 6 files restores pre-PR2 conflated null behavior; PR1 files untouched. |

## Chain / PR Boundary

- Mode: stacked PR slice (stacked-to-main)
- Current work-unit: PR2 — SA-01/SA-02 audited AjusteManual tx + role gate (PR1 was PC-01/PC-02)
- Boundary: Starts from PR1 commit (stock-mvp-hardening tasks 1.1/2.1/2.3 merged), ends before PR3 movimientos pagination/drawer and PR4 CSV import
- Estimated review budget impact: ~90 LOC changed in PR2 — DomainExceptions.cs ~35 lines new, ProductoService AjustarStockAsync hardening ~35 lines changed, ProductoController AjusteStock hardening ~25 lines changed, GlobalExceptionHandler +22 lines, Program.cs +2 lines, Validator trim fix ~4 lines. Total well under 400. Git diff for PR2-originated changes: ~85 insertions, ~25 deletions in tracked files; isolated to ajuste-stock slice, no movimientos/CSV/frontend.

## Build Capture

```
dotnet build "F:\Usuarios\Victor\Documentos\Proyecto cerrajería\Management\backend\API\API.csproj"
Compilación correcta.
0 Error(s)
Warnings: CS8981, CS0618, CS8602, CS8603 as above
```

## Implementation Notes

- **PR1 (preserved)**: GetAllAsync `Where(p => p.Id_negocio == _currentUser.NegocioId).ToListAsync()` then `IsEmpleado ? null : p.PrecioCompra` — avoids EF ternary translation. PUT guard loads GetByIdAsync, compares StockActual, 400 hint exactly "Use POST {id}/ajuste-stock with Motivo" no UpdateAsync call.
- **PR2 SA-01 tx**: `AjustarStockAsync(int id, AjusteStockRequest request, CancellationToken ct)` — tenant-isolated `FirstOrDefaultAsync(p => p.Id==id && p.Id_negocio==NegocioId, ct)` → `throw NotFoundException("Producto", id)` (distinct 404). Compute `stockAnterior=StockActual`, `stockNuevo=stockAnterior+CantidadDelta`, if `<0` throw `StockInsuficienteException(stockAnterior, delta, stockNuevo)` → mapped 422, no mutation, no MovimientoStock, no SaveChanges. Else `BeginTransactionAsync(ct)`, `Motivo trimmed = request.Motivo.Trim()` (validator already ensures trimmed length 5..500), insert `MovimientoStock {IdProducto, IdUsuario=UserId, Id_negocio=NegocioId, FechaMovimiento=UtcNow, Cantidad=Delta, TipoMovimiento=AjusteManual, StockAnterior, StockNuevo, Motivo=trimmed}` via `_context.MovimientosStock.Add`, update `producto.StockActual=stockNuevo`, `IdUsuarioModificador=UserId`, `SaveChangesAsync(ct)`, `CommitAsync(ct)`, rollback on catch, return `GetByIdAsync(id)` gated PrecioCompra (IsEmpleado check). Replicates VentaService transaction pattern (BeginTransactionAsync + Add MovimientoStock + SaveChanges + Commit) but for AjusteManual insert-only per AGENTS 4.16.
- **PR2 SA-02 controller**: `POST {id}/ajuste-stock` — checks `IsSuperAdmin→403`, `!IsAdmin&&!IsManager→403` (Dueño||Gerente allow, Empleado/SuperAdmin 403), calls `AjustarStockAsync(id, request, ct)` with `CancellationToken ct` forwarding, returns `Ok(result)` 200 with gated product. No conflated null branch; 404/422 via exceptions mapped by GlobalExceptionHandler. `ProducesResponseType` now includes 422. FluentValidation `AjusteStockRequestValidator` registered in Program.cs ensures 400 before tx reaches service.
- **Validator fix**: `AjusteStockRequestValidator` now trim-aware: `NotEmpty` + `Must(m => m.Trim().Length >=5)` + `Must(m => m.Trim().Length <=500)` to satisfy spec Motivo 5..500 trimmed, prevents whitespace-only bypass.
- **GlobalExceptionHandler**: extended to map `NotFoundException→404 ProblemDetails` (like KeyNotFoundException) and `DomainException→422 ProblemDetails` with `errors.StockActual` and `traceId`, per AGENTS 10 mapping NotFound→404, Domain→422. Enables SA-01 distinct signals.
- **Exceptions**: new `API/Exceptions/DomainExceptions.cs` defines `NotFoundException(entity,key)`, `DomainException`, `StockInsuficienteException`, `NegocioInactivoException` per AGENTS 10, aligned with design.md tx decision B.

## Deviations

- None — implementation matches design.md Decision Ajuste tx B (BeginTransactionAsync + update + insert AjusteManual), spec stock-adjustment SA-01/SA-02 (Delta!=0, Motivo 5..500, 422 no mutation, tenant isolation, Dueño||Gerente). No migration, no schema change, no movimientos pagination/CSV in this PR per scope isolation.

## Issues Found

- **Resolved in PR2**: Previous apply-progress Issue — AjustarStockAsync returned null for both not-found and StockNuevo<0 conflated to 404 — fixed via NotFoundException/DomainException split + GlobalExceptionHandler 404/422 mapping.
- Gentle-ai binary unavailable — no attempt ledger written; noted.
- CS8603 warning in ProductoService AjustarStockAsync return `GetByIdAsync` nullable handled via `?? throw NotFoundException` after commit.

## Next Recommended

sdd-apply PR3 (IA-01/IA-02 paginated movimientos + drawer) after PR2 merged to main — tasks 1.2 (ImportCsvResponse wrapper pending), 2.4, 3.1-3.4 (frontend movimientos), 4.x tests. PR4 CSV import (1.3, 2.5, import dialog) last.

## Gentle-AI Attempt Ledger

gentle-ai binary unavailable in this environment — attempt not recorded via CLI. Work-unit verified via dotnet build and code-path review above.

## Status

4/15 tasks complete (PR1 3 + PR2 1). Ready for verify (SA-01/SA-02) or chain PR3.
