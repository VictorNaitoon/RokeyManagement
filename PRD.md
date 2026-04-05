# RoKey MANAGEMENT - Product Requirements Document (PRD)

## Versión: 1.0
## Fecha: Marzo 2026
## Estado: Draft

---

## 1. Resumen Ejecutivo

### 1.1 Visión del Producto

**RoKey MANAGEMENT** es una plataforma SaaS multi-tenant de gestión empresarial (ERP/POS) diseñada específicamente para pequeños y medianos negocios del sector **ferretero** y **cerrajero** en Argentina y Latinoamérica.

El sistema permite a los negocios gestionar sus operaciones internas (ventas, compras, inventario, presupuestos, facturas, caja) y ofrece una interfaz pública de catálogo para clientes (con carrito y pedidos via WhatsApp).

### 1.2 Propuesta de Valor

| Para el Negocio | Diferenciador |
|-----------------|---------------|
| Todo-en-uno | Sistema integral que elimina la necesidad de múltiples herramientas |
| Multi-device | Acceso desde cualquier dispositivo con navegador |
| Especializado | Diseñado específicamente para ferreterías y cerrajerías |
| Sin安装 | Funciona en la nube, sin infrastructure headaches |
| Escalable | Planes que crecen con el negocio |

### 1.3 Modelo de Negocio SaaS

- **Ingresos recurrentes**: Suscripciones mensuales/anuales
- **Planes escalonados**: Básico, Profesional, Enterprise
- **Freemium**: Período de prueba gratuito (14 días)
- **Multi-moneda**: Pesos Argentinos (ARS) inicialmente, expandable a USD

---

## 2. Alcance del Producto

### 2.1 Funcionalidades Core (MVP)

#### 2.1.1 Gestión de Negocio (Multi-tenant)
- Registro y onboarding de nuevos negocios
- Perfil del negocio (datos fiscales, logo, configuración)
- Tipo de negocio: Ferretería, Cerrajería, Ambos
- Estado del negocio: Activo, Inactivo, Suspendido

#### 2.1.2 Gestión de Usuarios
- Roles: Administrador (Owner), Manager, Vendedor
- Autenticación JWT
- Gestión de contraseñas
- Permisos granulares por módulo

#### 2.1.3 Gestión de Productos
- CRUD de productos con imágenes
- Categorías (activación/desactivación)
- Control de stock (mínimo, actual)
- Alertas de stock bajo
- Productos vs Servicios (sin stock)
- Duplicación de productos (variantes)
- Precio de compra (solo Admin)

#### 2.1.4 Gestión de Ventas
- Punto de venta (POS)
- Registro de ventas con múltiples métodos de pago
- Cancelación de ventas (con reversal de stock)
- Facturación (proforma y oficial con CAE)
- Integración con ARCA (Argentina)

#### 2.1.5 Gestión de Compras
- Registro de compras a proveedores
- Control de stock automático
- Cancelación de compras

#### 2.1.6 Gestión de Presupuestos
- Creación y seguimiento de presupuestos
- Estados: Pendiente, Aceptado, Vencido
- Conversión automática a venta

#### 2.1.7 Gestión de Caja
- Apertura y cierre de caja
- Control de ingresos/egresos

#### 2.1.8 Catálogo Público (B2C)
- Visor de productos sin login
- Carrito de compras (session-based)
- Pedido vía WhatsApp API
- Configurable por tipo de negocio

#### 2.1.9 Reportes y Dashboard
- Métricas financieras por rol
- Informes de ventas, compras, stock
- Auditoría de movimientos

### 2.2 Funcionalidades SaaS (REQUIRED)

#### 2.2.1 Sistema de Suscripción
- **Planes de Subscription**:
  - **Básico**: 1 usuario, 500 productos, básicas features
  - **Profesional**: Hasta 5 usuarios, productos ilimitados, features completas
  - **Enterprise**: Usuarios ilimitados, multi-sucursal, soporte prioritario

- **Gestión de Plan**:
  - Upgrade/downgrade de plan
  - Cambio de facturación (mensual/anual)
  - Período de facturación

#### 2.2.2 Sistema de Facturación SaaS
- Generación de facturas por uso del servicio
- Métodos de pago: Tarjeta de crédito/débito, Mercado Pago, transferencia
- Historial de pagos
- Notas crédito por disputes

#### 2.2.3 Portal de Auto-gestión (Super Admin)
- Panel de administración de la plataforma
- Listado de todos los tenants (negocios)
- Monitor de uso y métricas
- Gestión de planes y precios
- Soporte técnico inline

#### 2.2.4 Portal del Cliente (Tenant Admin)
- Dashboard de uso y facturación
- Gestión de usuarios del negocio
- Cambio de plan
- Configuración del negocio
- Soporte técnico

#### 2.2.5 Onboarding de Nuevos Tenants
- flow de registro público
- Verificación de email
- Selección de plan
- Datos fiscales (CUIT, razón social)
- Pago inicial
- Setup automático del tenant

#### 2.2.6 Usage Tracking & Metering
- Cantidad de usuarios activos
- Cantidad de productos
- Cantidad de transacciones
- Almacenamiento de datos
- API calls (si aplica)

#### 2.2.7 Limits & Quotas
- Enforce de límites por plan
- Notificaciones de proximidad a límites
- Bloqueo temporal al alcanzar límite
- Opciones de upgrade inline

---

## 3. Requisitos No Funcionales

### 3.1 Arquitectura Multi-tenant

```
┌─────────────────────────────────────────────────────────────┐
│                    SUPER ADMIN PANEL                        │
│                  (Gestión de la Plataforma)                 │
└─────────────────────────────────────────────────────────────┘
                              │
        ┌─────────────────────┼─────────────────────┐
        ▼                     ▼                     ▼
┌───────────────┐     ┌───────────────┐     ┌───────────────┐
│   Tenant 1   │     │   Tenant 2   │     │   Tenant N   │
│  (Negocio A) │     │  (Negocio B) │     │  (Negocio N) │
│  - Usuarios  │     │  - Usuarios  │     │  - Usuarios  │
│  - Productos │     │  - Productos │     │  - Productos │
│  - Ventas    │     │  - Ventas    │     │  - Ventas    │
│  - etc.      │     │  - etc.      │     │  - etc.      │
└───────────────┘     └───────────────┘     └───────────────┘
        │                     │                     │
        └─────────────────────┼─────────────────────┘
                              ▼
                    ┌─────────────────┐
                    │  SHARED CORE    │
                    │  (Código común) │
                    │  - API Core     │
                    │  - Auth         │
                    │  - Billing      │
                    │  - Tenant mgmt  │
                    └─────────────────┘
```

### 3.2 Estrategias de Tenant Isolation

| Approach | Descripción | Pros | Contras |
|----------|-------------|------|---------|
| Database separada | Cada tenant tiene su propia DB | Máximo aislamiento | Costo de infraestructura |
| Schema separado | Un DB, múltiplos schemas | Balance costo/aislamiento | Complejidad queries |
| **Shared Schema** | Una tabla con `Id_negocio` | **Costo mínimo** | **Requiere strict filtering** |
| Application Level | Lógica en código | Flexible | Complejidad |

**DECISIÓN**: Shared Schema con `Id_negocio` filtering (ya implementado en el core)

### 3.3 Requisitos Técnicos

| Categoría | Requisito |
|-----------|-----------|
| **Disponibilidad** | 99.5% uptime SLA |
| **Escalabilidad** | Soporte 1000+ tenants concurrentes |
| **Seguridad** | Encriptación at-rest y in-transit, SOC2 compliant |
| **Rendimiento** | < 200ms response time (P95) |
| **Cumplimiento** | ARCA (facturación), LGPD (datos) |
| **Backup** | Daily automated backups, 30-day retention |

### 3.4 Stack Tecnológico Confirmado

- **Backend**: ASP.NET Core (.NET 9.0)
- **Frontend**: React 19 + TypeScript
- **Database**: PostgreSQL
- **ORM**: Entity Framework Core
- **CQRS**: MediatR
- **Auth**: JWT Bearer
- **Cloud**: Ready for deployment (AWS/GCP/Azure)

---

## 4. Modelo de Datos SaaS Extensiones

### 4.1 Nuevas Entidades Required

```csharp
// Planes de Suscripción
public class Plan
{
    public int Id { get; set; }
    public string Nombre { get; set; } // "Básico", "Profesional", "Enterprise"
    public string Descripcion { get; set; }
    public decimal PrecioMensual { get; set; }
    public decimal PrecioAnual { get; set; }
    public int MaxUsuarios { get; set; }
    public int MaxProductos { get; set; }
    public int MaxTransaccionesMes { get; set; }
    public bool SoportePrioritario { get; set; }
    public bool MultiSucursal { get; set; }
    public bool APIAccess { get; set; }
    public bool Activo { get; set; }
}

// Suscripción de un Negocio
public class Suscripcion
{
    public int Id { get; set; }
    public int IdNegocio { get; set; }
    public int IdPlan { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    public DateTime? FechaProximoPago { get; set; }
    public EstadoSuscripcion Estado { get; set; } // Activa, PendientePago, Cancelada, Vencida
    public TipoFacturacion TipoFacturacion { get; set; } // Mensual, Anual
    public DateTime? FechaCancelacion { get; set; }
    public string? MotivoCancelacion { get; set; }
}

// Pagos de Suscripción
public class PagoSuscripcion
{
    public int Id { get; set; }
    public int IdSuscripcion { get; set; }
    public decimal Monto { get; set; }
    public DateTime FechaPago { get; set; }
    public MetodoPago Metodo { get; set; }
    public string? TransactionId { get; set; }
    public EstadoPago Estado { get; set; } // Pendiente, Exitoso, Fallido, Reembolsado
}

// Métricas de Uso (para billing)
public class MetricaUso
{
    public int Id { get; set; }
    public int IdNegocio { get; set; }
    public int Mes { get; set; }
    public int Anio { get; set; }
    public int TotalUsuarios { get; set; }
    public int TotalProductos { get; set; }
    public int TotalTransacciones { get; set; }
    public DateTime UltimaActualizacion { get; set; }
}

// Super Admin (gestor de la plataforma)
public class SuperAdmin
{
    public int Id { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public string Rol { get; set; } // SuperAdmin, Soporte
    public bool Activo { get; set; }
}
```

### 4.2 Enums Required

```csharp
public enum EstadoSuscripcion
{
    Activa = 1,
    PendientePago = 2,
    Cancelada = 3,
    Vencida = 4,
    Suspendida = 5
}

public enum TipoFacturacion
{
    Mensual = 1,
    Anual = 2
}

public enum MetodoPago
{
    TarjetaCredito = 1,
    TarjetaDebito = 2,
    MercadoPago = 3,
    Transferencia = 4
}

public enum EstadoPago
{
    Pendiente = 1,
    Exitoso = 2,
    Fallido = 3,
    Reembolsado = 4
}
```

---

## 5. User Flows Principales

### 5.1 Onboarding de Nuevo Tenant

```
┌──────────────┐    ┌──────────────┐    ┌──────────────┐    ┌──────────────┐
│  Landing     │───▶│  Registro    │───▶│  Selección   │───▶│  Datos       │
│  Page        │    │  (Email/Pass)│    │  de Plan     │    │  Fiscales    │
└──────────────┘    └──────────────┘    └──────────────┘    └──────────────┘
                                                                │
                           ┌────────────────────────────────────┘
                           ▼
                    ┌──────────────┐    ┌──────────────┐    ┌──────────────┐
                    │  Pago Inicial│───▶│  Creación    │───▶│  Dashboard   │
                    │  (MercadoPago)│    │  Tenant     │    │  Negocio     │
                    └──────────────┘    └──────────────┘    └──────────────┘
```

### 5.2 Renovación de Suscripción

```
┌──────────────┐    ┌──────────────┐    ┌──────────────┐
│  5 días antes│───▶│  Notificación│───▶│  Proceso     │
│  vencimiento │    │  email       │    │  pago auto   │
└──────────────┘    └──────────────┘    └──────────────┘
                                               │
                           ┌────────────────────┘
                           ▼
                    ┌──────────────┐    ┌──────────────┐
                    │  Éxito       │    │  Fallido     │
                    │  → Renovado  │    │  → Suspender │
                    └──────────────┘    └──────────────┘
```

### 5.3 Upgrade de Plan

```
┌──────────────┐    ┌──────────────┐    ┌──────────────┐    ┌──────────────┐
│  Usuario     │───▶│  Verificar   │───▶│  Diferencia  │───▶│  Activar     │
│  solicita    │    │  límites     │    │  prorrateo   │    │  nuevo plan  │
└──────────────┘    └──────────────┘    └──────────────┘    └──────────────┘
```

---

## 6. Casos de Uso SaaS

| Código | Caso de Uso | Actor | Descripción |
|--------|-------------|-------|-------------|
| SAAS-001 | Registro de Negocio | Usuario Nuevo | El usuario registra un nuevo negocio en la plataforma |
| SAAS-002 | Selección de Plan | Usuario Nuevo | El usuario elige un plan de suscripción |
| SAAS-003 | Pago de Suscripción | Usuario Nuevo | El usuario realiza el pago inicial |
| SAAS-004 | Inicio de Sesión (SaaS) | Admin Negocio | El admin del negocio inicia sesión |
| SAAS-005 | Renovación Automática | Sistema | El sistema renueva la suscripción automáticamente |
| SAAS-006 | Upgrade de Plan | Admin Negocio | El admin cambia a un plan superior |
| SAAS-007 | Downgrade de Plan | Admin Negocio | El admin cambia a un plan inferior |
| SAAS-008 | Cancelación de Suscripción | Admin Negocio | El admin cancela la suscripción |
| SAAS-009 | Visualizar Facturas | Admin Negocio | El admin ve el historial de facturas |
| SAAS-010 | Uso de Métricas | Sistema | El sistema registra uso mensual |
| SAAS-011 | Verificar Límites | Sistema | El sistema verifica si el tenant excedió límites |
| SAAS-012 | Notificar Límites | Sistema | El sistema notifica al admin sobre uso próximo a límites |
| SAAS-013 | Suspensión por Mora | Sistema | El sistema suspende el acceso por falta de pago |
| SAAS-014 | Gestión de Super Admin | Super Admin | El super admin gestiona la plataforma |
| SAAS-015 | Listar Tenants | Super Admin | El super admin ve todos los negocios |
| SAAS-016 | Configurar Planes | Super Admin | El super admin crea/gestiona planes |
| SAAS-017 | Ver Dashboard Plataforma | Super Admin | El super admin ve métricas globales |

---

## 7. Requisitos de Interface

### 7.1 Portal Público (Landing Page)

| Sección | Contenido |
|---------|-----------|
| Hero | Nombre, tagline, CTA (Registrarme) |
| Features | Lista de funcionalidades clave |
| Planes | Tabla comparativa de planes con precios |
| Testimonios | Casos de éxito |
| FAQ | Preguntas frecuentes |
| Contacto | Formulario de contacto |
| Footer | Links legales, redes sociales |

### 7.2 Portal de Registro/Onboarding

| Paso | Campos |
|------|--------|
| 1. Cuenta | Email, Contraseña, Confirmar contraseña |
| 2. Plan | Selección de plan (cards interactivas) |
| 3. Negocio | Nombre, CUIT, Dirección, Teléfono, Tipo de negocio |
| 4. Admin | Nombre, Apellido del usuario admin |
| 5. Pago | Datos de tarjeta, total a pagar |

### 7.3 Dashboard del Negocio

- Resumen de suscripción (plan, próximo pago, uso)
- Métricas rápidas del negocio
- Alertas (stock bajo, pagos pendientes)
- Acceso rápido a módulos

### 7.4 Super Admin Panel

- Lista de tenants con filtros
- Métricas de la plataforma
- Gestión de planes
- Facturación global
- Soporte técnico

---

## 8. Integraciones Required

### 8.1 Payment Gateway

| Proveedor | Propósito | Estado |
|-----------|-----------|--------|
| **Mercado Pago** | Procesamiento de pagos en Argentina | Required |
| Stripe | Backup/International | Opcional |
| PayU | LATAM backup | Opcional |

### 8.2 Notificaciones

| Canal | Propósito |
|-------|-----------|
| **Email (SMTP/SendGrid)** | Bienvenida, facturas, alertas, renovación |
| **WhatsApp Business** | Notificaciones críticas (opcional) |

### 8.3 Facturación ARCA

| Requisito | Descripción |
|-----------|-------------|
| CAE | Código de Autorización de Electrónico |
| WebService ARCA | Integración para facturación electrónica |
| Tipos de comprobantes | Factura A/B/C, Nota de Crédito, Nota de Débito |

### 8.4 Analytics

| Herramienta | Propósito |
|-------------|-----------|
| Google Analytics | Tracking usage del portal público |
| PostHog (self-hosted) | Product analytics (opcional) |

---

## 9. Roadmap de Desarrollo

### Fase 1: MVP SaaS (Mes 1-2)
- [ ] Sistema de planes y suscripciones
- [ ] Portal de registro público
- [ ] Integración con Mercado Pago
- [ ] Dashboard de suscripción para tenant
- [ ] Sistema de métricas de uso
- [ ] Super admin básico

### Fase 2: Enterprise Features (Mes 3-4)
- [ ] Upgrade/downgrade de planes
- [ ] Notificaciones automaticas (email)
- [ ] Facturación SaaS detallada
- [ ] API access para Enterprise

### Fase 3: Escalabilidad (Mes 5-6)
- [ ] Multi-sucursal (Enterprise)
- [ ] Sistema de soporte técnico
- [ ] Marketplace de integraciones
- [ ] App mobile (PWA)

---

## 10. KPIs y Métricas de Éxito

### 10.1 Métricas de Negocio

| Métrica | Target Mes 6 | Target Mes 12 |
|---------|--------------|---------------|
| Tenants Activos | 50 | 200 |
| MRR (Monthly Recurring Revenue) | $5,000 USD | $25,000 USD |
| Churn Rate | < 5% | < 3% |
| Trial to Paid | 30% | 40% |
| NPS | > 50 | > 60 |

### 10.2 Métricas Técnicas

| Métrica | Target |
|---------|--------|
| Uptime | 99.5% |
| Response Time (P95) | < 200ms |
| Error Rate | < 0.1% |
| Deployment Frequency | Weekly |

---

## 11. Riesgos y Mitigaciones

| Riesgo | Probabilidad | Impacto | Mitigación |
|--------|--------------|---------|------------|
| Low tenant adoption | Media | Alto | Marketing focus, SEO, partnerships |
| Payment failures | Baja | Alto | Retry logic, multiple gateways |
| Data isolation breach | Baja | Crítico | Security audit, pen testing |
| ARCA compliance changes | Media | Medio | Monitoring, flexible architecture |
| Competitor launch | Media | Medio | Fast iteration, unique features |

---

## 12. Apendices

### A. Glosario

| Término | Definición |
|---------|------------|
| **Tenant** | Un negocio/cliente individual de la plataforma |
| **Multi-tenant** | Arquitectura donde múltiples tenants comparten infraestructura |
| **MRR** | Monthly Recurring Revenue - Ingresos mensuales recurrentes |
| **Churn** | Tasa de cancelación de suscriptores |
| **CAE** | Código de Autorización Electrónica (ARCA) |
| **ARCA** | Agencia de Recaudación y Control Aduanero (Argentina) |

### B. Referencias

- Documento SRS IEEE 830 existente
- Casos de Uso (UC-001 a UC-019)
- Modelo ER del proyecto
- AGENTS.md - Reglas de desarrollo

---

## 13. Aprobaciones

| Rol | Nombre | Fecha | Firma |
|-----|--------|-------|-------|
| Product Owner | | | |
| Technical Lead | | | |
| UX Designer | | | |

---

*Documento generado para guiar el desarrollo de RoKey MANAGEMENT como producto SaaS comercializable.*
