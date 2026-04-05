-- Script para insertar los planes de suscripción por defecto
-- Ejecutar en la base de datos RkDB

-- Plan Básico
INSERT INTO plan (Nombre, Descripcion, PrecioMensual, PrecioAnual, MaxUsuarios, MaxProductos, MaxTransaccionesMes, SoportePrioritario, MultiSucursal, APIAccess, Activo, Orden)
VALUES (
    'Básico',
    'Ideal para emprendimientos individuales o pequeños negocios. Incluye las funcionalidades esenciales para gestionar tu ferretería o cerrajería.',
    9.99,
    99.90,
    1,
    500,
    100,
    false,
    false,
    false,
    true,
    1
);

-- Plan Profesional
INSERT INTO plan (Nombre, Descripcion, PrecioMensual, PrecioAnual, MaxUsuarios, MaxProductos, MaxTransaccionesMes, SoportePrioritario, MultiSucursal, APIAccess, Activo, Orden)
VALUES (
    'Profesional',
    'Perfecto para negocios en crecimiento. Soporta hasta 5 usuarios, productos ilimitados y todas las funcionalidades avanzadas incluyendo catálogo público y facturación.',
    24.99,
    249.90,
    5,
    999999,
    1000,
    true,
    false,
    false,
    true,
    2
);

-- Plan Enterprise
INSERT INTO plan (Nombre, Descripcion, PrecioMensual, PrecioAnual, MaxUsuarios, MaxProductos, MaxTransaccionesMes, SoportePrioritario, MultiSucursal, APIAccess, Activo, Orden)
VALUES (
    'Enterprise',
    'Solución completa para grandes operaciones. Usuarios ilimitados, multi-sucursal, acceso a API y soporte prioritario 24/7.',
    49.99,
    499.90,
    999999,
    999999,
    999999,
    true,
    true,
    true,
    true,
    3
);
