# Tasks: stock-mvp-hardening

## Review Workload Forecast

| Field | Value |
|-------|-------|
| Estimated changed lines | 650-850 (backend ~300 + frontend ~280 + tests ~150) |
| 400-line budget risk | High |
| Chained PRs recommended | Yes |
| Suggested split | PR1 leak+PUT-block → PR2 ajuste-stock tx → PR3 movimientos pagination/drawer → PR4 CSV import |
| Delivery strategy | stacked-to-main (PR1 autonomous) |
| Chain strategy | stacked-to-main |

Decision needed before apply: No — PR1 (PC-01/PC-02) approved as stacked-to-main work-unit
Chained PRs recommended: Yes — split as 4 PRs per forecast
Chain strategy: stacked-to-main
400-line budget risk: High — PR1 kept <100 LOC, remaining PRs sliced per unit

### Suggested Work Units

| Unit | Goal | Likely PR | Focused test command | Runtime harness | Rollback boundary |
|------|------|-----------|----------------------|-----------------|-------------------|
| 1 | PC-01/PC-02 price gating + PUT block | PR1 | `dotnet test --filter PC01` | `GET /api/v1/Producto` as Empleado vs Dueño; `PUT {id}` with changed StockActual →400 hint | `backend/API/Services/Productos/ProductoService.cs`, `backend/API/Controllers/Productos/ProductoController.cs`, `apps/admin-panel/src/hooks/useProductos.ts` |
| 2 | SA-01/SA-02 audited AjusteManual tx | PR2 | `dotnet test --filter AjusteStock` | `POST /api/v1/Producto/{id}/ajuste-stock` ± delta, cross-tenant 404, Empleado 403 | `backend/API/DTO/Request/Productos/AjusteStock*`, service AjustarStockAsync, controller ajuste-stock |
| 3 | IA-01/IA-02 paginated movimientos + drawer | PR3 | `dotnet test --filter Movimientos` | `GET /api/v1/Producto/{id}/movimientos?page&pageSize` + drawer render | `MovimientoStock*Response`, service GetMovimientos, hook useMovimientosStock, `MovimientosStockDrawer.tsx` |
| 4 | CI-01/02/03 streaming CSV import | PR4 | `dotnet test --filter ImportCsv` | `POST /api/v1/Producto/import/csv` comma/semicolon/BOM, 5MB/1000, 200 vs 207 | `ImportCsvResponse`, CsvHelper in `ProductoService`, import endpoint, `ProductoImportDialog.tsx`, `API.csproj` |

## Phase 1: Foundation

- [x] 1.1 Verify `AjusteStockRequest` + `AjusteStockRequestValidator` Delta!=0/Motivo 5..500 | Spec: SA-01 | Files: `backend/API/DTO/Request/Productos/AjusteStockRequest.cs`, `AjusteStockRequestValidator.cs` | AC: Given zero delta or Motivo<5 When POST ajuste-stock Then 400 validation no tx | Deps: none | Est: 0.5d
- [ ] 1.2 Add `ImportCsvResponse` + `MovimientoStockListResponse` wrapper | Spec: CI-01, IA-01 | Files: `backend/API/DTO/Response/Productos/ImportCsvResponse.cs` (create), `MovimientoStockResponse.cs` (wrap) | AC: Given 25 movimientos When GET page1/10 Then 200 shape {items,total,page,pageSize} | Deps: none | Est: 0.5d
- [ ] 1.3 Add CsvHelper 31.x to `API.csproj` | Spec: CI-01 | Files: `backend/API/API.csproj` | AC: Given `dotnet restore` When build Then CsvHelper present | Deps: none | Est: 0.25d

## Phase 2: Core Implementation

- [x] 2.1 Harden `GetAllAsync` post-ToList gating + align `GetByIdAsync` | Spec: PC-01 | Files: `backend/API/Services/Productos/ProductoService.cs` | AC: Given 3 productos PrecioCompra=100 When Empleado GET /Producto Then each PrecioCompra==null; Dueño/Gerente==100; cross-tenant absent per design decision C | Deps: 1.2 | Est: 0.5d
- [x] 2.2 Harden `AjustarStockAsync` tx BeginTransaction + StockAnterior/Nuevo + AjusteManual insert-only | Spec: SA-01 | Files: `backend/API/Services/Productos/ProductoService.cs`, `backend/API/Exceptions/DomainExceptions.cs`, `backend/API/Middleware/GlobalExceptionHandler.cs` | AC: Given StockActual=10 When POST Delta 5 Then StockNuevo 15+Movimiento persisted; Delta -5 with 3→422 no write | Deps: 1.1 | Est: 1d
- [x] 2.3 Enforce `PUT {id}` 400 hint for StockActual drift | Spec: PC-02 | Files: `backend/API/Controllers/Productos/ProductoController.cs` | AC: Given StockActual=10 When PUT StockActual 12 Then 400 `hint:Use POST {id}/ajuste-stock with Motivo` no update; equal→200 | Deps: 2.2 | Est: 0.5d
- [x] 2.4 Rewrite `GetMovimientosStockAsync` wrapper+Count+strict Id_negocio filter, max 100 | Spec: IA-01 | Files: `backend/API/Services/Productos/IProductoService.cs`, `ProductoService.cs` | AC: Given legacy null rows When tenant B queries Then not returned/counted; pageSize>100 capped | Deps: 1.2 | Est: 0.5d
- [ ] 2.5 Implement `ImportarCsvAsync` CsvHelper streaming auto `,`/`;` BOM, 5MB/1000 caps, trim, escape `=+-@`, category Active+tenant, uniqueness | Spec: CI-01, CI-02 | Files: `backend/API/Services/Productos/IProductoService.cs`, `ProductoService.cs` | AC: Given 2 rows comma When import Then 200 created 2; BOM/semicolon→parsed; 6MB/1001→400/413; `=CMD`→`'=CMD`; dup→first ok second error | Deps: 1.2,1.3 | Est: 1.5d
- [ ] 2.6 Align controller roles Dueño+Gerente allow, Empleado/SuperAdmin 403, wrap movimientos, import 200/207 | Spec: SA-02, CI-03, IA-02, PC-01 | Files: `backend/API/Controllers/Productos/ProductoController.cs` | AC: Given Empleado When POST ajuste/import or GET movimientos Then 403; cross-tenant 404; `GET movimientos` returns wrapper not bare List | Deps: 2.2,2.4,2.5 | Est: 1d — PR3 IA slice DONE (GET movimientos Dueño||Gerente 403 Empleado/SuperAdmin, wrapper MovimientoStockListResponse, tenant 404, page/pageSize validate max100, ProducesResponseType 200/400/403/404); import 200/207 deferred to PR4

## Phase 3: Integration / Wiring

- [x] 3.1 Fix `canViewPrecioCompra()=Dueño||Gerente` + add `useMovimientosStock`/`useAjusteStock`/`useImportCsv` hooks | Spec: PC-01, IA-02, SA-01, CI-01 | Files: `apps/admin-panel/src/hooks/useProductos.ts` | AC: Given Empleado renders When hook enabled Then movimientos hook disabled; Gerente enabled per PO override | Deps: 2.6 | Est: 0.5d — PR3: canViewHistorial()=Dueño||Gerente, useMovimientosStock enabled !!id&&(Dueño||Gerente) + page/pageSize wrapper, useAjusteStock added; useImportCsv deferred to PR4
- [ ] 3.2 Add `ajusteStockSchema` + Ajuste/Import/Movimientos pagination types | Spec: SA-01, IA-01, CI-01 | Files: `apps/admin-panel/src/lib/schemas/producto.schema.ts`, `apps/admin-panel/src/types/producto.types.ts` | AC: Given invalid motivo<5 When validate Then zod error | Deps: 1.1 | Est: 0.25d
- [x] 3.3 Create `AjusteStockDialog.tsx`, `MovimientosStockDrawer.tsx`, `ProductoImportDialog.tsx` | Spec: SA-01, IA-02, CI-01 | Files: `apps/admin-panel/src/components/productos/*` (create) | AC: Given Dueño opens drawer Then columns Fecha/Tipo badge/Cantidad/Anterior→Nuevo/Motivo/Usuario rendered paginated | Deps: 3.1,3.2 | Est: 1d — PR3: MovimientosStockDrawer.tsx DONE (paginated Fecha/Tipo badge/Cantidad/Anterior→Nuevo/Motivo/Usuario, page/pageSize, Dueño||Gerente; AjusteStockDialog+ProductoImportDialog deferred to PR4)
- [ ] 3.4 Wire `ProductosPage.tsx` dialogs, strip StockActual from PUT, gate Historial/Import by role | Spec: PC-02, IA-02, CI-03 | Files: `apps/admin-panel/src/pages/productos/ProductosPage.tsx` | AC: Given Empleado views page Then Historial/Import hidden; PUT never sends StockActual | Deps: 3.3 | Est: 0.5d

## Phase 4: Testing / Verification

- [ ] 4.1 Unit tests validator, caps, escape `=+-@`, `canViewPrecioCompra` | Spec: SA-01, CI-02, PC-01 | Files: `backend/Tests/**` (xUnit), `apps/admin-panel/src/tests/**` (Vitest) | AC: Given `=CMD|calc` When import Then stored `'=CMD|calc` | Deps: 3.2 | Est: 0.5d
- [ ] 4.2 Integration tests SA 200/422/400 + PC-01 null + PC-02 400 hint + IA pagination+403 + CI delimiters/BOM/dup/category/tenant | Spec: all | Files: `backend/Tests/**` (WebApplicationFactory+PG) | AC: Covers spec scenarios listed; 403/404 side-effect free | Deps: 2.6 | Est: 1d
- [ ] 4.3 Playwright E2E Drawer/AjusteDialog/ImportDialog (200 vs 207 partial) | Spec: IA-01, SA-01, CI-01 | Files: `apps/admin-panel/src/tests/e2e/**` | AC: Given import partial rows When UI shows errors[{row,column}] Then created rows still listed | Deps: 3.4 | Est: 0.5d

## Phase 5: Cleanup

- [ ] 5.1 Update `[ProducesResponseType]` 200/207/400/403/404/422, remove legacy `|| Id_negocio==null` comments, docs | Spec: IA-01, SA-01, CI-01 | Files: `backend/API/Controllers/Productos/ProductoController.cs`, docs | AC: Given Swagger When open Then ajuste 422, import 207 documented | Deps: 4.2 | Est: 0.25d
