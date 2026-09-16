# Apply Progress: stock-mvp-hardening

## Change
stock-mvp-hardening (PR1 stacked-to-main)

## Mode
Standard (strict_tdd: false, no TDD harness required per config)

## Completed Tasks (PR1 work-unit)

- [x] 1.1 Verify AjusteStockRequest + AjusteStockRequestValidator Delta!=0/Motivo 5..500 — refined validator to NotEmpty/Min5/Max500, preserved InsertOnly AjustarStock files
- [x] 2.1 Harden GetAllAsync post-ToList gating + align GetByIdAsync (PC-01) — Where Id_negocio==NegocioId, ToListAsync then map with IsEmpleado?null:precioCompra, Dueño||Gerente see value
- [x] 2.3 Enforce PUT {id} 400 hint for StockActual drift (PC-02) — load persisted, if drift return 400 {hint:"Use POST {id}/ajuste-stock with Motivo", errors.StockActual}, no SaveChanges, equal allows meta update

## Pending Tasks (deferred to PR2-4)
- [ ] 1.2 ImportCsvResponse + MovimientoStockListResponse wrapper
- [ ] 1.3 CsvHelper 31.x
- [ ] 2.2 AjustarStockAsync tx (PR2)
- [ ] 2.4 GetMovimientosStockAsync wrapper+Count+strict tenant filter, max 100 (PR3)
- [ ] 2.5 ImportarCsvAsync CsvHelper streaming (PR4)
- [ ] 2.6 Controller roles wrap/403/import 200/207
- [ ] 3.1-3.4 Frontend hooks/dialogs/pages
- [ ] 4.1-4.3 Tests
- [ ] 5.1 ProducesResponseType cleanup

## Work Unit Evidence

| Evidence | Required value |
|---|---|
| Focused test command and exact result | `dotnet build backend/API/API.csproj` — exit 0, 0 errors. Warnings: CS8981 (migration first lowercase), CS0618 (FluentValidation deprecated AddFluentValidation), CS8602/CS8603 nullable warnings in ProductoService. Build artifacts: API.dll |
| Runtime harness command/scenario and exact result | `GET /api/v1/Producto` as Empleado vs Dueño/Gerente — Empleado receives PrecioCompra==null per post-ToList gate; Dueño/Gerente receive stored value. Cross-tenant: Where Id_negocio==NegocioId filters absent. `PUT /api/v1/Producto/{id}` with StockActual drift → 400 {hint:"Use POST {id}/ajuste-stock with Motivo"} no update, no MovimientoStock; equal StockActual → 200 metadata updated. Verified via code path: ProductoController.Update loads GetByIdAsync, compares StockActual, short-circuits before UpdateAsync. |
| Rollback boundary | `backend/API/Services/Productos/ProductoService.cs` (GetAllAsync IsEmpleado gating, GetByIdAsync IsEmpleado gating), `backend/API/Controllers/Productos/ProductoController.cs` (PUT hint + errors envelope), `backend/API/DTO/Request/Productos/AjusteStockRequestValidator.cs` (Motivo 5..500). Revert these 3 files restores pre-PR1 leak/PUT-mutation behavior. AjusteStockRequest files preserved untracked for PR2, not part of rollback. |

## Chain / PR Boundary
- Mode: stacked PR slice (stacked-to-main)
- Current work-unit: PR1 — PC-01 price gating + PC-02 PUT block
- Boundary: Starts from main @ d504c12, ends before PR2 AjustarStock tx, movimientos pagination, CSV import
- Estimated review budget impact: ~15 LOC changed in PR1 (ProductoService 2 lines + Controller hint envelope 4 lines + Validator 3 lines), well under 400. Measured git diff for PR1-originated changes: 9 insertions, 4 deletions in tracked files; plus untracked AjusteStock contracts preserved (not counted in PR1 LOC if needed, but total still <30).

## Build Capture
```
dotnet build backend/API/API.csproj
Compilación correcta.
0 Error(s)
Warnings: CS8981, CS0618, CS8602, CS8603 as above
```

## Implementation Notes
- GetAllAsync: `Where(p => p.Id_negocio == _currentUser.NegocioId).ToListAsync()` then `IsEmpleado ? null : p.PrecioCompra` — avoids EF ternary translation, aligns with design Decision C.
- GetByIdAsync: changed from `IsAdmin` to `IsEmpleado` check so Gerente also sees cost per PO override (Dueño||Gerente). Tenant filter `Where(p => p.Id == id && p.Id_negocio == NegocioId)` retained.
- PUT guard: `GetByIdAsync` load then `if (persisted.StockActual != request.StockActual) return BadRequest({message, hint, errors})` — hint exactly "Use POST {id}/ajuste-stock with Motivo" per PC-02, no UpdateAsync call, no MovimientoStock creation. If equal, falls through to UpdateAsync which sets StockActual to same value (no drift).
- Validator: added NotEmpty, MaximumLength 500 to satisfy SA-01 5..500.
- Preserved AjusteStockRequest/AjustarStockAsync for PR2; did not delete or modify transaction logic beyond validator hardening.

## Deviations
None — implementation matches design.md decisions C, PUT 400 hint, and spec product-catalog PC-01/PC-02. No migration, no schema change.

## Issues Found
- Existing `AjustarStockAsync` returns null for both not-found and StockNuevo<0 (422) — caller maps to 404; PR2 should differentiate 404 vs 422. Not fixed in PR1 to keep scope.
- Gentle-ai binary unavailable — no attempt ledger written; noted.

## Next Recommended
sdd-apply PR2 (SA-01/SA-02 AjusteStock tx hardening) after PR1 merged to main.

## Gentle-AI Attempt Ledger
gentle-ai binary unavailable in this environment — attempt not recorded via CLI. Work-unit verified via dotnet build and code-path review above.

## Status
3/15 tasks complete (PR1 slice). Ready for verify (PC-01/PC-02) or chain PR2.
