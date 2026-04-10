# Testing Manual - Fase 4: Productos + Categorías

## 📋 Prerrequisitos

1. Backend corriendo en `http://localhost:5000`
2. Frontend corriendo en `http://localhost:5173`
3. Al menos un usuario de cada rol creado en el sistema
4. Al menos una categoría creada para poder asignar a productos

---

## 🔐 Roles para probar

| Rol | Permisos de productos | Permisos de categorías |
|-----|---------------------|----------------------|
| **Dueño** | Todo | Todo |
| **Gerente** | Crear/Editar/Eliminar | Todo |
| **Empleado** | Solo crear y listar | Ninguno |
| **SuperAdmin** | Sin acceso | Sin acceso |

---

## 🧪 Flujo 1: Gestión de Productos (Dueño/Gerente)

### 1.1 Listar productos

**Pasos:**
1. Iniciá sesión como **Dueño** o **Gerente**
2. Andá a **Productos** en el sidebar
3. Verificá que aparezca la tabla con columnas: Nombre, Código, Categoría, Precio Venta, Stock, Estado

**✅ Esperado:**
- La tabla carga con los productos del negocio
- El campo de búsqueda filtra por nombre o código
- Los productos inactivos aparecen con badge rojo

---

### 1.2 Crear producto

**Pasos:**
1. Estando en la lista de productos, click en **"+ Nuevo Producto"**
2. Completá los campos:
   - Nombre: "Cerradura Yale 660"
   - Código: "YALE660"
   - Descripción: "Cerradura de embutir Yale"
   - Precio Venta: 15000
   - Precio Compra: 8500 (visible solo para Admin/Gerente)
   - Stock Actual: 10
   - Stock Mínimo: 5
   - Activar "Es Servicio": NO
   - Seleccionar una categoría
3. Click en **"Guardar"**

**✅ Esperado:**
- Toast verde: "Producto creado correctamente"
- El producto aparece en la tabla
- El drawer se cierra

**❌ Si falla:**
- Toast rojo con el error del backend
- Los campos inválidos se resaltan en rojo

---

### 1.3 Subir imagen de producto

**Pasos:**
1. En el formulario de crear/editar producto
2. Buscá la sección de imagen
3. Click en **"Subir imagen"** o arrastrá una imagen al área designada
4. Seleccioná una imagen JPG/PNG (menor a 5MB)
5. Verificá que aparezca el preview de la imagen

**✅ Esperado:**
- El widget de Cloudinary se abre
- Se puede seleccionar una imagen
- La imagen se sube y aparece como preview
- La URL se guarda en el campo oculto

**❌ Si falla:**
- Mensaje de error si el archivo no es imagen válida
- Mensaje de error si la imagen es muy grande

---

### 1.4 Editar producto

**Pasos:**
1. En la lista de productos, click en el botón **✏️ Editar** de un producto
2. Modificá el Precio Venta a 16000
3. Modificá el Stock Actual a 8
4. Click en **"Actualizar"**

**✅ Esperado:**
- Toast verde: "Producto actualizado correctamente"
- Los cambios se reflejan inmediatamente en la tabla

---

### 1.5 Eliminar producto (soft delete)

**Pasos:**
1. En la lista de productos, click en el botón **🗑️ Eliminar** de un producto
2. Aparecerá un diálogo de confirmación: "¿Estás seguro de eliminar este producto?"
3. Click en **"Confirmar"**

**✅ Esperado:**
- Toast verde: "Producto eliminado"
- El producto desaparece de la tabla (o aparece como inactivo con badge rojo)

**❌ Si hay error:**
- Toast rojo con mensaje del backend

---

### 1.6 Reactivar producto

**Pasos:**
1. Buscá un producto que esté inactivo (con badge rojo)
2. Click en el botón **🔄 Activar**

**✅ Esperado:**
- Toast verde: "Producto activado"
- El producto pasa a estado activo (sin badge rojo)

---

## 🧪 Flujo 2: Gestión de Categorías (Solo Admin/Dueño/Gerente)

### 2.1 Ver categorías

**Pasos:**
1. Iniciá sesión como **Dueño** o **Gerente**
2. Andá a **Categorías** en el sidebar

**✅ Esperado:**
- Se muestra la lista de categorías
- Solo visibles para estos roles

**❌ Para otros roles:**
- Si entrás como **Empleado**: el item "Categorías" NO aparece en el sidebar
- Si intentás acceder via URL `/categorias`: error 403 o redirección

---

### 2.2 Crear categoría

**Pasos:**
1. En la página de categorías, click en **"+ Nueva Categoría"**
2. Completá:
   - Nombre: "Cerraduras"
   - Descripción: "Todo tipo de cerraduras"
3. Click en **"Guardar"**

**✅ Esperado:**
- Toast verde: "Categoría creada correctamente"
- La categoría aparece en la lista
- La categoría aparece disponible en el selector del formulario de productos

---

### 2.3 Editar categoría

**Pasos:**
1. En la lista de categorías, click en **✏️ Editar**
2. Cambiá el nombre a "Cerraduras y Candados"
3. Click en **"Actualizar"**

**✅ Esperado:**
- Toast verde: "Categoría actualizada"
- Los cambios se reflejan en la lista

---

### 2.4 Eliminar categoría vacía

**Pasos:**
1. Creá una categoría nueva de prueba
2. Verificá que NO tenga productos asignados
3. Click en **🗑️ Eliminar**
4. Confirmar

**✅ Esperado:**
- Toast verde: "Categoría eliminada"
- La categoría desaparece de la lista (o aparece como inactiva)

---

### 2.5 Intentar eliminar categoría con productos

**Pasos:**
1. Intentá eliminar una categoría que SÍ tiene productos asociados

**✅ Esperado:**
- Toast rojo: "No se puede eliminar una categoría que tiene productos asociados"
- La categoría NO se elimina

---

## 🧪 Flujo 3: Alertas de Stock Bajo

### 3.1 Ver alertas de stock

**Pasos:**
1. Andá a **Productos** en el sidebar
2. Click en **"Ver Alertas"** o andá a `/productos/alertas`

**✅ Esperado:**
- Se muestra una tabla con productos cuyo stock está bajo o igual al mínimo
- Columnas: Nombre, Stock Actual, Stock Mínimo, Diferencia, Categoría
- Ordenados por Diferencia (mayor diferencia primero = más críticos arriba)
- Badge de alerta visual (rojo/amarillo)

---

### 3.2 Sin alertas de stock

**Pasos:**
1. Asegurate de que todos los productos tengan stock > stock mínimo
2. Andá a la página de alertas

**✅ Esperado:**
- Mensaje: "¡Felicidades! No hay productos con stock bajo"
- Lista vacía

---

## 🧪 Flujo 4: Permisos por Rol

### 4.1 Empleado - Ver productos

**Pasos:**
1. Iniciá sesión como **Empleado**
2. Andá a **Productos**

**✅ Esperado:**
- Ve la lista de productos
- Ve los campos: Nombre, Código, Categoría, Precio Venta, Stock, Estado
- NO ve el campo **Precio Compra**

---

### 4.2 Empleado - Crear producto

**Pasos:**
1. Como Empleado, en la lista de productos
2. Click en **"+ Nuevo Producto"**
3. Completá los campos obligatorios
4. Click en **"Guardar"**

**✅ Esperado:**
- Toast verde: "Producto creado correctamente"
- El producto aparece en la lista

---

### 4.3 Empleado - NO puede editar/eliminar

**Pasos:**
1. Como Empleado, en la lista de productos

**✅ Esperado:**
- NO aparecen los botones ✏️ Editar ni 🗑️ Eliminar
- El producto no se puede modificar ni eliminar

---

### 4.4 SuperAdmin - Sin acceso a productos

**Pasos:**
1. Iniciá sesión como **SuperAdmin**

**✅ Esperado:**
- El item "Productos" NO aparece en el sidebar
- Si accedés via URL `/productos`: error 403
- El item "Categorías" tampoco aparece

---

## 🧪 Flujo 5: Búsqueda y Filtros

### 5.1 Buscar por nombre

**Pasos:**
1. En la lista de productos, escribí en el campo de búsqueda: "Yale"
2. Esperá 300ms (debounce)

**✅ Esperado:**
- La tabla se filtra mostrando solo productos con "Yale" en el nombre o código

---

### 5.2 Limpiar búsqueda

**Pasos:**
1. Con la búsqueda activa, borrá el texto del campo de búsqueda

**✅ Esperado:**
- Se muestran todos los productos nuevamente

---

### 5.3 Ordenar por columna

**Pasos:**
1. Click en el header de la columna "Nombre"

**✅ Esperado:**
- La tabla se ordena alfabéticamente por nombre
- Aparece un icono de flecha indicando ASC/DESC

---

### 5.4 Cambiar página

**Pasos:**
1. Verificá que haya más de 10 productos
2. Click en el botón de página siguiente "2"

**✅ Esperado:**
- Se muestran los siguientes 10 productos
- El indicador de página cambia a "2"

---

## 📝 Checklist de Prueba

| # | Escenario | Estado |
|---|-----------|--------|
| 1 | Listar productos como Dueño | ⬜ |
| 2 | Listar productos como Empleado | ⬜ |
| 3 | Crear producto exitoso | ⬜ |
| 4 | Crear producto con imagen | ⬜ |
| 5 | Validación: producto sin nombre | ⬜ |
| 6 | Editar producto | ⬜ |
| 7 | Eliminar producto | ⬜ |
| 8 | Reactivar producto | ⬜ |
| 9 | Ver categorías como Admin | ⬜ |
| 10 | Crear categoría | ⬜ |
| 11 | Editar categoría | ⬜ |
| 12 | Eliminar categoría vacía | ⬜ |
| 13 | Intentar eliminar categoría con productos | ⬜ |
| 14 | Ver alertas de stock | ⬜ |
| 15 | Mensaje sin alertas | ⬜ |
| 16 | Empleado no ve precio compra | ⬜ |
| 17 | Empleado no puede editar/eliminar | ⬜ |
| 18 | SuperAdmin sin acceso | ⬜ |
| 19 | Búsqueda por nombre | ⬜ |
| 20 | Ordenamiento de columnas | ⬜ |
| 21 | Paginación | ⬜ |

---

## 🐛 Reporte de Bugs

Si encontrás un bug, documentalo acá:

| Bug # | Descripción | Pasos para reproducir | Resultado esperado | Resultado actual | Severidad |
|-------|-------------|---------------------|-------------------|-----------------|-----------|
| 1 | | | | | |
| 2 | | | | | |

---

## ✅ Criterios de Aprobación

Para que la Fase 4 esté completa, TODOS los escenarios de arriba deben pasar.

- [ ] Todos los escenarios de la checklist marcados como ✅
- [ ] Sin bugs críticos o medios abiertos
- [ ] Código commiteado y pusheado
