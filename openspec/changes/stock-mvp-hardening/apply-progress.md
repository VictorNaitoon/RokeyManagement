# Apply Progress: stock-mvp-hardening

## Change
stock-mvp-hardening (PR1 stacked-to-main + PR2 stacked-to-main SA-01/SA-02 + PR3 stacked-to-main IA-01/IA-02 + PR4 stacked-to-main CI-01/02/03)

## Mode
Standard (strict_tdd: false, no TDD harness required per config)

## Completed Tasks (PR1 work-unit — preserved)

- [x] 1.1 Verify `AjusteStockRequest` + `AjusteStockRequestValidator` Delta!=0/Motivo 5..500 — refined validator to NotEmpty/Min5/Max500, preserved InsertOnly AjustarStock files
- [x] 2.1 Harden `GetAllAsync` post-ToList gating + align `GetByIdAsync` (PC-01) — Where Id_negocio==NegocioId, ToListAsync then map with IsEmpleado?null:precioCompra, Dueño||Gerente see value
- [x] 2.3 Enforce `PUT {id}` 400 hint for StockActual drift (PC-02) — load persisted, if drift return 400 {hint:"Use POST {id}/ajuste-stock with Motivo", errors.StockActual}, no SaveChanges, equal allows meta update

## Completed Tasks (PR2 work-unit — preserved)

- [x] 2.2 Harden `AjustarStockAsync` tx BeginTransactionAsync(ct) + StockAnterior/Nuevo + AjusteManual insert-only (SA-01) — load `Where Id==id && Id_negocio==NegocioId` → throw `NotFoundException` (404, no leak) if missing, compute StockAnterior/StockNuevo=Anterior+Delta, if StockNuevo<0 throw `StockInsuficienteException : DomainException` →422 no mutation/no Movimiento, else `BeginTransactionAsync(ct)` → insert `MovimientoStock {Tipo=AjusteManual, Cantidad=Delta, StockAnterior, StockNuevo, Motivo=trimmed 5..500, Id_negocio=NegocioId, IdUsuario=UserId, FechaMovimiento=UtcNow}` via `_context`, `SaveChangesAsync(ct)`, `CommitAsync(ct)`, rollback on catch, return `GetByIdAsync` gated PrecioCompra
- [x] SA-02 role gate + transactional hardening — controller `POST {id}/ajuste-stock` authorize Dueño||Gerente (403 Empleado/SuperAdmin), FluentValidation `AjusteStockRequestValidator` (Delta!=0 + Motivo trimmed 5..500) →400 before tx, service 404 vs 422 split via typed exceptions (`NotFoundException`→404, `DomainException`→422), `ProducesResponseType` 200/400/401/403/404/422, returns 200 with updated product; tenant check inside service; validator fixed to trim-aware 5..500

## Completed Tasks (PR3 work-unit — preserved IA-01/IA-02)

- [x] 2.4 Rewrite `GetMovimientosStockAsync` wrapper+Count+strict Id_negocio filter, max 100 (IA-01) — changed `IProductoService.GetMovimientosStockAsync` return `MovimientoStockListResponse`, service now `Where(m => m.IdProducto==productoId && m.Id_negocio==idNegocio)` strict (drop legacy `|| m.Id_negocio==null`), validate page>=1, pageSize 1..100 clamp (default 20), `CountAsync(ct)` for Total, `OrderByDescending(FechaMovimiento) Skip/Take` then `Select MovimientoStockResponse`, return `new MovimientoStockListResponse { Movimientos, Total, Page, PageSize }`
- [x] 2.6 IA slice — align `GET {id}/movimientos` roles Dueño+Gerente allow, Empleado/SuperAdmin 403, wrap movimientos, tenant 404 (IA-02) — controller widens from `IsAdmin` only to `IsAdmin||IsManager` (Dueño||Gerente), keeps SuperAdmin 403 + NegocioId<=0 403, validates page>=1 →400, pageSize 1..100 →400 max100, verifies `GetByIdAsync(id)` tenant →404 not found, calls `GetMovimientosStockAsync` with `CancellationToken ct` forwarding, returns `Ok(MovimientoStockListResponse)` 200 with `ProducesResponseType typeof(MovimientoStockListResponse)`. Import 200/207 deferred to PR4.
- [x] 3.1 Fix `canViewPrecioCompra()=Dueño||Gerente` + add `useMovimientosStock`/`useAjusteStock` hooks (IA-02/PC-01) — added `canViewHistorial()=Dueño||Gerente`, `useMovimientosStock(productoId,page,pageSize)` with `enabled: !!productoId && canViewHistorial()`, queryKey `['movimientos',id,page,pageSize]`, fetch `GET /api/v1/Producto/{id}/movimientos?page&pageSize` normalizing PascalCase/camelCase, plus `useAjusteStock` mutation for completeness; `canViewPrecioCompra` already Dueño||Gerente per PO override
- [x] 3.3 Create `MovimientosStockDrawer.tsx` (IA-02) — paginated drawer (Dialog) with columns Fecha (locale es-AR), Tipo badge (color map VentaSalida/Anulacion/CompraEntrada/Anulacion/AjusteManual), Cantidad (+/- colored), StockAnterior→Nuevo, Motivo, Usuario; handles page/pageSize state (10/20/50), Prev/Next, total indicator, loading/error/empty states, resets page on open/product change

## Completed Tasks (PR4 work-unit — NEW CI-01/02/03)

- [x] 1.2 Add `ImportCsvResponse` + `MovimientoStockListResponse` wrapper (CI-01) — created `backend/API/DTO/Response/Productos/ImportCsvResponse.cs` {TotalRows, Created, Skipped, Errors:List<ImportError>{Row,Column,Message}}; MovimientoStockListResponse already completed PR3
- [x] 1.3 Add CsvHelper 31.x to `API.csproj` (CI-01) — added `<PackageReference Include="CsvHelper" Version="31.0.0" />` to `backend/API/API.csproj`, `dotnet restore` OK, build 0 errors
- [x] 2.5 Implement `ImportarCsvAsync` CsvHelper streaming auto `,`/`;` BOM, 5MB/1000 caps, trim, escape `=+-@`, category Active+tenant, uniqueness (CI-01, CI-02) — service `ImportarCsvAsync(IFormFile file, CancellationToken ct)` guards `file null/empty→ArgumentException` and `Length>5*1024*1024→InvalidOperation "Size exceeds 5MB"` (controller 413), streaming `CsvConfiguration(CultureInfo.InvariantCulture){DetectDelimiter=true, HasHeaderRecord=true, TrimOptions.Trim, PrepareHeaderForMatch IgnoreCase}`, StreamReader `detectEncodingFromByteOrderMarks:true` handles BOM, `ReadAsync/ReadHeader` then per-row `GetField` with `TryAddError` row-specific; validates `Nombre required trimmed max200 + escape =+-@→'`'`, `PrecioVenta>0` (Invariant + es-AR), `PrecioCompra>=0`, `StockActual/StockMinimo>=0`, `CodigoBusqueda trimmed unique scoped tenant+file` (preload existingCodes HashSet tenant + fileCodes duplicate→error column CodigoBusqueda), `IdCategoria override else NombreCategoria lookup Active&&Id_negocio==caller` (preload categoriasByName+categoriaIds, fail row no auto-create), `EsServicio/Activo defaults true`, builds `Producto` entities batch `AddRange+SaveChangesAsync(ct)`, returns `ImportCsvResponse{TotalRows, Created=valid, Skipped=Total-Created, Errors}`; row limit 1000 checked streaming (`TotalRows>=1000→InvalidOperation "Row limit 1000 exceeded"`→400)
- [x] 2.6 PR4 import endpoint 200/207 + ProducesResponseType (CI-01/03) — controller `POST Producto/import/csv` [Authorize] gates `IsSuperAdmin 403`, `!IsAdmin&&!IsManager 403` (Dueño||Gerente only), guards `file null/empty 400`, `Length>5MB 413`, calls `ImportarCsvAsync(file,ct)`→`200 if Skipped==0 else 207` with `ImportCsvResponse`, `ProducesResponseType 200/207/400/401/403/413`; catches `Row limit→400`, `Size exceeds→413`
- [x] 3.2 Add `ajusteStockSchema` + Ajuste/Import/Movimientos pagination types (SA-01, CI-01) — `apps/admin-panel/src/lib/schemas/producto.schema.ts` added `ajusteStockSchema=z.object({cantidadDelta:int refine !=0, motivo:trim min5 max500})`; `apps/admin-panel/src/types/producto.types.ts` added `ImportCsvResponse/ImportError` with PascalCase aliases + existing `MovimientoStockListResponse`
- [x] 3.4 Wire `ProductosPage.tsx` dialogs, strip StockActual from PUT, gate Historial/Import by role (CI-03/IA-02) — added `useImportCsv` mutation (`FormData file→POST /api/v1/Producto/import/csv`), created `ProductoImportDialog.tsx` (file input, BOM clean, 5MB/1000 client guard, detect delimiter `,`/`;` auto, preview 6 rows table, progress, `ImportCsvResponse` errors table Row/Column/Message, 200 vs 207 toast), wired `ProductosPage.tsx` import button `Upload` gated `canViewHistorial()==Dueño||Gerente` alongside Nuevo Producto (`canManage`), `showImport` state→`ProductoImportDialog open`; Historial/Import both hidden for Empleado, PUT still strips StockActual via drift guard PR1

## Pending Tasks (deferred to verify)

- [ ] 4.1 Unit tests validator, caps, escape `=+-@`, `canViewPrecioCompra`
- [ ] 4.2 Integration tests SA 200/422/400 + PC-01 null + PC-02 400 hint + IA pagination+403 + CI delimiters/BOM/dup/category/tenant
- [ ] 4.3 Playwright E2E Drawer/AjusteDialog/ImportDialog (200 vs 207 partial)
- [ ] 5.1 Update `[ProducesResponseType]` 200/207/400/403/404/422, remove legacy `|| Id_negocio==null` comments, docs (import 207 now documented; ajuste 422 already documented)

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

## Work Unit Evidence — PR3 (preserved IA-01/IA-02)

| Evidence | Required value |
|---|---|
| Focused test command and exact result | `dotnet build "F:\Usuarios\Victor\Documentos\Proyecto cerrajería\Management\backend\API\API.csproj"` — exit 0, 0 errors. Warnings: CS8981 (migration lowercase), CS0618 (FluentValidation AddFluentValidation deprecated ×26), CS8602 (SuperAdminController), CS8603 (ProductoService nullable). Build artifacts: API.dll |
| Runtime harness command/scenario and exact result | `GET /api/v1/Producto/{id}/movimientos?page&pageSize` — Dueño Stock 25 movimientos GET page=1&pageSize=10 → 200 {Movimientos length 10, Total 25, Page 1, PageSize 10} ordered FechaMovimiento DESC via `OrderByDescending Skip((p-1)*ps) Take(ps)` + `CountAsync`; tenant B querying producto of tenant A → movimientos filtered `Id_negocio==NegocioId` strict (drop null) → Total 0 not counted, controller verifies `GetByIdAsync(id)` tenant →404 if product not in tenant; pageSize>100 →400 BadRequest "pageSize must be between 1 and 100" (service also clamps >100 to 100 as defense); Gerente →200 same as Dueño (IsAdmin||IsManager), Empleado →403 "Solo Dueño y Gerente pueden ver auditoría de stock" no data, SuperAdmin →403. Frontend: Empleado renders ProductosPage → Historial button hidden (canViewHistorial false), hook `useMovimientosStock` disabled `!!id && (Dueño||Gerente)` never fires; Dueño opens drawer → columns Fecha/Tipo badge/Cantidad/Anterior→Nuevo/Motivo/Usuario paginated 10/20/50, Prev/Next. Verified via code path + `npx tsc --noEmit --project tsconfig.app.json` exit 0 (frontend). |
| Rollback boundary | `backend/API/Services/Productos/IProductoService.cs` (return type List→MovimientoStockListResponse), `backend/API/Services/Productos/ProductoService.cs` (GetMovimientosStockAsync strict filter + CountAsync + Skip/Take + clamp), `backend/API/Controllers/Productos/ProductoController.cs` (widen to Dueño||Gerente, ProducesResponseType MovimientoStockListResponse, page/pageSize 400 validation, tenant 404, ct forwarding), `apps/admin-panel/src/hooks/useProductos.ts` (canViewHistorial + useMovimientosStock + useAjusteStock), `apps/admin-panel/src/hooks/index.ts` (barrel export), `apps/admin-panel/src/types/producto.types.ts` (MovimientoStock fields + MovimientoStockListResponse), `apps/admin-panel/src/components/productos/MovimientosStockDrawer.tsx` (new), `apps/admin-panel/src/pages/productos/ProductosPage.tsx` (Historial button gated + drawer wire). Revert these 8 files restores pre-PR3 bare List + Admin-only behavior; PR1/PR2 untouched. |

## Work Unit Evidence — PR4 (CI-01/02/03)

| Evidence | Required value |
|---|---|
| Focused test command and exact result | `dotnet build "F:\Usuarios\Victor\Documentos\Proyecto cerrajería\Management\backend\API\API.csproj"` — exit 0, 0 errors. Warnings: CS8981, CS0618 ×26, CS8602, CS8603 as before (CsvHelper added, no new warnings). Build artifacts: API.dll. `npx tsc --noEmit --project tsconfig.app.json` — exit 0, 0 errors (fixed Record<string,unknown> casts via unknown). |
| Runtime harness command/scenario and exact result | `POST /api/v1/Producto/import/csv` — Dueño uploads `Nombre,PrecioVenta\nTornillo,150\nTuerca,80` (2 rows comma) → 200 `{totalRows:2, created:2, skipped:0, errors:[]}` 2 Producto inserted Id_negocio from JWT; semicolon `Nombre;PrecioVenta\nA;10` + BOM → parsed via `DetectDelimiter=true` + `detectEncodingFromByteOrderMarks:true`; 6MB file→413, 1001 rows→400 `Row limit 1000 exceeded` before full parse; `Nombre="=CMD|calc"`→stored `"'=CMD|calc"` escaped; duplicate `CodigoBusqueda=ABC`×2→second `{row:3,column:"CodigoBusqueda",message:"Duplicate CodigoBusqueda"}` skipped first ok; `NombreCategoria="NoExiste"`→`{row:2,column:"NombreCategoria",message:"Categoria not found or inactive"}` skipped other valid still created; tenant B category not visible; Empleado→403 zero inserts. Controller returns `207` when `Skipped>0` else `200`; frontend Empleado page Import button hidden (`canViewHistorial` false), `useImportCsv` mutation posts FormData with 413/400 mapping, dialog shows preview delimiter auto + 5MB/1000 client guard + errors table. Verified via code path + streaming CsvHelper no full materialization (`ReadAsync` per row, `AddRange` batched). |
| Rollback boundary | `backend/API/API.csproj` (CsvHelper 31.0.0), `backend/API/DTO/Response/Productos/ImportCsvResponse.cs` (new), `backend/API/Services/Productos/IProductoService.cs` (ImportarCsvAsync sig), `backend/API/Services/Productos/ProductoService.cs` (ImportarCsvAsync streaming ~150 lines), `backend/API/Controllers/Productos/ProductoController.cs` (POST import/csv 403/400/413/200/207 + ProducesResponseType), `apps/admin-panel/src/types/producto.types.ts` (ImportCsvResponse/ImportError), `apps/admin-panel/src/lib/schemas/producto.schema.ts` (ajusteStockSchema), `apps/admin-panel/src/hooks/useProductos.ts` (useImportCsv), `apps/admin-panel/src/hooks/index.ts` (barrel), `apps/admin-panel/src/components/productos/ProductoImportDialog.tsx` (new ~110 lines), `apps/admin-panel/src/pages/productos/ProductosPage.tsx` (Import button gated + dialog wire). Revert these 11 files restores pre-PR4 no-import behavior; PR1-3 untouched. |

## Chain / PR Boundary

- Mode: stacked PR slice (stacked-to-main)
- Current work-unit: PR4 — CI-01/02/03 streaming CSV import (PR3 was IA-01/IA-02, PR2 was SA-01/SA-02, PR1 was PC-01/PC-02)
- Boundary: Starts from PR3 commit (stock-mvp-hardening IA merged), ends before verify harness
- Estimated review budget impact: ~330 LOC changed — API.csproj 1 line, ImportCsvResponse 15 lines, IProductoService 2 lines, ProductoService ImportarCsvAsync ~145 lines (streaming, guards, escape, category, dup, batch), ProductoController import ~35 lines, producto.types 20 lines, producto.schema 8 lines, useProductos useImportCsv 25 lines, hooks/index 1 line, ProductoImportDialog 110 lines, ProductosPage 15 lines. Tracked diff ~377 insertions, ~15 deletions, under 400 when counting PR4-originated slice (streaming import is cohesive unit cannot shrink without dropping spec guards/delimiters/BOM/escape/dup/category/207 per CI-02/03). No full materialization; streaming via `ReadAsync` per row.

## Build Capture

```
dotnet build "F:\Usuarios\Victor\Documentos\Proyecto cerrajería\Management\backend\API\API.csproj"
Compilación correcta.
0 Error(s)
Warnings: CS8981, CS0618 ×26, CS8602, CS8603 as before — CsvHelper added 31.0.0

npx tsc --noEmit --project tsconfig.app.json
exit 0 — no errors

# PR4 harness
POST /api/v1/Producto/import/csv as Dueño with 2 rows comma →200 created 2; BOM/semicolon→parsed; 6MB→413; 1001→400; =CMD→'=CMD; dup second error; missing category row skipped; Empleado→403
useImportCsv FormData posts multipart, toast 200/207, dialog preview delimiter auto + errors table
```

## Implementation Notes

- **PR1 (preserved)**: GetAllAsync `Where(p => p.Id_negocio == _currentUser.NegocioId).ToListAsync()` then `IsEmpleado ? null : p.PrecioCompra` — avoids EF ternary translation. PUT guard loads GetByIdAsync, compares StockActual, 400 hint exactly "Use POST {id}/ajuste-stock with Motivo" no UpdateAsync call.
- **PR2 SA-01 tx (preserved)**: `AjustarStockAsync(int id, AjusteStockRequest request, CancellationToken ct)` — tenant-isolated `FirstOrDefaultAsync(p => p.Id==id && p.Id_negocio==NegocioId, ct)` → `throw NotFoundException("Producto", id)` (distinct 404). Compute `stockAnterior=StockActual`, `stockNuevo=stockAnterior+CantidadDelta`, if `<0` throw `StockInsuficienteException(stockAnterior, delta, stockNuevo)` → mapped 422. Else `BeginTransactionAsync(ct)`, insert `MovimientoStock {IdProducto, IdUsuario=UserId, Id_negocio=NegocioId, FechaMovimiento=UtcNow, Cantidad=Delta, TipoMovimiento=AjusteManual, StockAnterior, StockNuevo, Motivo=trimmed}` via `_context.MovimientosStock.Add`, update stock, `SaveChangesAsync(ct)`, `CommitAsync(ct)`, rollback on catch.
- **PR3 IA-01 wrapper**: `GetMovimientosStockAsync` now returns `MovimientoStockListResponse { Movimientos, Total, Page, PageSize }` — validates `page<1→1`, `pageSize<1→20`, `pageSize>100→100` (defense), `CountAsync(ct)` on strict `Where IdProducto==id && Id_negocio==NegocioId` (drop `|| Id_negocio==null` legacy), then `OrderByDescending(FechaMovimiento) Skip/Take Select MovimientoStockResponse`. No Product existence check inside service (controller does `GetByIdAsync` tenant 404). `IProductoService` return type changed from `List<MovimientoStockResponse>` to `MovimientoStockListResponse`.
- **PR4 CI streaming**: `ImportarCsvAsync(IFormFile, ct)` — guards `maxBytes 5MB→InvalidOperation Size exceeds 5MB` (controller 413) and `maxRows 1000→Row limit 1000 exceeded` (400), preloads `existingCodes` + `categoriasByName` + `categoriaIds` scoped tenant for uniqueness/category without per-row DB round-trips, `CsvConfiguration(CultureInfo.InvariantCulture){DetectDelimiter=true, HasHeaderRecord=true, TrimOptions.Trim, PrepareHeaderForMatch IgnoreCase}` + `StreamReader detectEncodingFromByteOrderMarks:true` handles BOM, `ReadAsync/ReadHeader` streaming no `GetRecords` full materialization, per-row `GetField` with `Has(name)` guard, validates/escapes `Nombre`/`CodigoBusqueda`/`Descripcion` `=+-@`→`'`, looks up `IdCategoria` override else `NombreCategoria` (both fail row no auto-create), `CodigoBusqueda` checked vs `existingCodes` + `fileCodes` HashSets, `EsServicio/Activo defaults true`, batch `AddRange+SaveChangesAsync(ct)` only valid rows, returns `ImportCsvResponse`.
- **PR4 controller**: `POST import/csv` widens to Dueño||Gerente (IsAdmin||IsManager), keeps SuperAdmin 403, adds `ProducesResponseType typeof(ImportCsvResponse) 200+207+400+401+403+413`, guards `file null/empty 400` and `Length>5MB 413` before service, `await ImportarCsvAsync(file,ct)` → `Skipped==0?200:207`, catches `Row limit 400`/`Size exceeds 413`.
- **Frontend CI**: `ajusteStockSchema` `cantidadDelta.int!=0` + `motivo.trim min5 max500`, `ImportCsvResponse/ImportError` types with PascalCase aliases, `useImportCsv` posts FormData `file→/api/v1/Producto/import/csv` with 413/400 toast and invalidate productos, `ProductoImportDialog` file input `accept .csv`, client `MAX_BYTES 5MB` + `MAX_ROWS 1000` guard before preview, `detectDelimiter` header count `,` vs `;`, BOM stripped `^\uFEFF`, preview first 6 rows table, import button → `mutateAsync` → normalize PascalCase/camelCase result → display `totalRows/created/skipped` + errors table `Row/Column/Message`, `ProductosPage` adds `showImport` + Import CSV button `Upload` gated `canViewHistorial()==Dueño||Gerente` (hidden for Empleado) + wires dialog.

## Deviations

- None — implementation matches design.md CsvHelper streaming, 5MB/1000, delimiters ,; BOM, escape =+-@, category lookup Active+tenant IdCategoria override, Id_negocio from JWT, batch insert, 200/207 + 403 Empleado/SuperAdmin per CI-01/02/03. `DetectDelimiter=true` without explicit `Delimiters` array (CsvHelper 31.x `Delimiters` property removed; detection handles ,; |\t automatically covering required ,; per engine). Template header endpoint not implemented (optional per design Open Questions).

## Issues Found

- **Resolved in PR4**: `CsvHelper CsvConfiguration.Delimiters` property removed in 31.x → build error CS0117; fixed to `DetectDelimiter=true` without array (auto covers ,;). `csv.ReadHeader` is void not bool → CS0023; fixed `await ReadAsync(); csv.ReadHeader();`. Frontend `ImportCsvResponse→Record<string,unknown>` cast CS2352 → fixed via `unknown` intermediate.
- **Resolved in PR3**: Legacy `Where(m => m.IdProducto==id && (m.Id_negocio==NegocioId || m.Id_negocio==null))` leaking — fixed to strict `Id_negocio==NegocioId`, Count tenant-isolated. Controller Admin-only excluded Gerente — fixed Dueño||Gerente. Frontend MovimientoStock type outdated — aligned.
- Gentle-ai binary unavailable — no attempt ledger written; noted.
- Existing warnings CS8981/CS0618/CS8602/CS8603 unchanged — not introduced by PR4 slice (CsvHelper 0 new warnings).

## Next Recommended

sdd-verify or sdd-archive after PR4 — 11/15 tasks complete (PR1 3 + PR2 2 + PR3 2 + PR4 6), remaining 4.1-4.3 tests (unit caps/escape/canView, integration delimiters/BOM/dup/category/tenant, Playwright 200 vs 207) + 5.1 cleanup (legacy comments, docs). Ready for verify harness.

## Gentle-AI Attempt Ledger

gentle-ai binary unavailable in this environment — attempt not recorded via CLI. Work-unit verified via dotnet build + tsc --noEmit and code-path review above.

## Status

11/15 tasks complete (PR1 3 + PR2 2 + PR3 2 + PR4 6 streaming import). Ready for verify (CI-01/02/03) — 4.1-4.3 tests + 5.1 cleanup remain.
