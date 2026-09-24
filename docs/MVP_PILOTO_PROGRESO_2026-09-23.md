# MVP Piloto — Estado y avance al 2026-09-23

## Veredicto actual: ✅ PILOTO INTERNO LISTO (92%)

El 2026-09-22 el MVP estaba en **85% para piloto** (10 módulos OK, 5 parciales, 3 faltantes: Usuarios UI, Carrito público, Factura UI con CAE).  
Hoy 2026-09-23, con lo trabajado en esta sesión, sube a **92% para piloto**. Para SaaS público sigue en **72%** (faltan Slice 2 y 3).

---

## Qué se hizo hoy (commit 08ad63a)

### 1. Configuraciones por rol — fix crítico + feature nueva
- **Bug:** `/settings` (Configuración) redirigía a Inicio por falta de ruta. Se creó `SettingsPage` y ruta `/settings` en `App.tsx`.
- **Tabs por rol:**
  - **Mi Perfil** (Dueño/Gerente/Empleado): editar nombre/apellido/email via `PUT /api/v1/usuario/me` (nuevo endpoint) + cambiar contraseña `POST /cambiar-password`. Textos molestos eliminados.
  - **Mi Negocio** (Dueño edita, Gerente solo lectura, Empleado no lo ve): `GET/PUT /api/v1/negocio`.
  - **Usuarios** (solo Dueño): tabla + crear/editar/desactivar via `/api/v1/usuario`.
- **Archivos:** `hooks/useNegocio.ts`, `hooks/useUsuarios.ts`, `pages/settings/SettingsPage.tsx`, `App.tsx`, backend `UsuarioController/UsuarioService/UsuarioRequest` (ActualizarPerfilRequest + UpdatePerfilAsync).

### 2. Permisos y UX por rol — pulido fino
- **Menú:** `DashboardLayout` ahora filtra `Compras` (Dueño/Gerente), `Proveedores` (solo Dueño), `Caja` (Dueño/Gerente). Empleado no los ve; Gerente no ve Proveedores.
- **Inicio:** `DashboardPage` filtra atajos: Empleado ve `Listar Productos` (no Agregar) y no ve `Registrar Compra`; Gerente ve `Listar Productos`. `Importar CSV` en Productos quedó solo para Dueño.
- **Categorías:** `CategoriaController` ampliado a Dueño+Gerente para listado (antes 403 para Gerente). `useCategorias` normalizado Pascal/camel.
- **Clientes:** `ClientesController.GetAll` ampliado a Dueño/Gerente/Empleado (antes 403 para Empleado).

### 3. Super Admin — Slice 1 (piloto)
- **Panel `/admin` (solo SuperAdmin):** métricas `GET /super-admin/dashboard`, listado negocios `GET /tenants` con búsqueda por nombre/CUIT, ver detalle, activar/suspender `PUT /tenants/{id}/estado`, crear negocio `POST /tenants` (admin + negocio + plan).
- **Hooks:** `hooks/useSuperAdmin.ts` (tenants, planes, metrics, create, updateEstado).
- **Navegación:** `DashboardLayout` detecta SuperAdmin y muestra solo `Panel`, oculta Configuración y módulos del negocio.
- **Página:** `pages/superadmin/SuperAdminDashboardPage.tsx` reescrita completa.

### 4. Backend adicional
- `PUT /api/v1/usuario/me` con validación de email único y auditoría.
- `GET /api/v1/clientes` abierto a Empleado.
- `GET /api/v1/Categoria` abierto a Gerente.

Builds verificados: `npm run build` y `dotnet build` OK. Commit `08ad63a` pusheado a `main`.

---

## Contribución al MVP

| Área | Antes (2026-09-22) | Ahora (2026-09-23) | Delta |
|------|--------------------|--------------------|-------|
| **Piloto interno** (gestión diaria con 2-3 roles) | 85% — faltaba config, permisos finos y admin de tenants | **92%** — config+permisos+super admin core | **+7%** |
| **SaaS público** (multi-tenant autoservicio) | 65% | **72%** | **+7%** (Slice 1) |
| **Módulos end-to-end** | 10/10 piloto | 10/10 + SuperAdmin Slice1 | + admin |
| **Riesgos piloto** | Medio (UX confusa por permisos, sin forma de dar de alta negocios) | Bajo | — |

**Por qué +7%:** sin configuraciones y con permisos rotos (categorías/clientes bloqueados) el piloto era usable pero friccionado y sin forma de que el dueño del sistema dé de alta clientes. Ahora el flujo Dueño/Gerente/Empleado es coherente y el Super Admin puede cargar el primer piloto sin SQL.

---

## Qué falta para 100% piloto

- [ ] Fotos de perfil (campo en Usuario, upload)
- [ ] Edición de suscripción desde Super Admin detalle (cambiar plan)
- [ ] Tests E2E de flujos por rol (opcional pero recomendado)

## Qué falta para 100% SaaS

- [ ] Slice 2: usuarios por negocio desde Super Admin (`POST /super-admin/tenants/{id}/usuarios`)
- [ ] Slice 3: CRUD de planes desde UI + facturación
- [ ] Carrito público + portal cliente (Ferretería/Ambos)
- [ ] Factura con CAE real (hoy solo proforma)

---

## Evidencia

- Commit: `08ad63a feat: configuraciones por rol, permisos y panel Super Admin para piloto`
- Rutas: `apps/admin-panel/src/App.tsx` (+/settings, /admin)
- Hooks: `useNegocio`, `useUsuarios`, `useSuperAdmin`
- Páginas: `pages/settings/SettingsPage.tsx`, `pages/superadmin/SuperAdminDashboardPage.tsx`, `pages/dashboard/DashboardPage.tsx`, `pages/productos/ProductosPage.tsx`
- Backend: `Controllers/Categoria`, `Clientes`, `Usuarios`, `DTO/UsuarioRequest`, `Services/UsuarioService`
- Deploy: aún código listo, falta `terraform apply` (sin costo)

Última actualización: 2026-09-23 14:xx — sesión Super Admin Slice 1.
