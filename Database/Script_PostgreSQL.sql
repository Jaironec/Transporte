-- =============================================
-- Script de Base de Datos: TransporteDB
-- Sistema de Gestión Logística y Contable
-- Motor: PostgreSQL 12+
-- =============================================

-- Crear base de datos (ejecutar como superusuario)
-- CREATE DATABASE "TransporteDB" WITH ENCODING 'UTF8' LC_COLLATE='es_ES.UTF-8' LC_CTYPE='es_ES.UTF-8';
-- \c "TransporteDB";

-- =============================================
-- Tabla: Vehiculos
-- =============================================
CREATE TABLE IF NOT EXISTS "Vehiculos" (
    "Id" SERIAL PRIMARY KEY,
    "Placa" VARCHAR(10) NOT NULL UNIQUE,
    "Marca" VARCHAR(50) NOT NULL,
    "Modelo" VARCHAR(50),
    "CapacidadLitros" DECIMAL(10, 2) NOT NULL,
    "AnioFabricacion" INTEGER,
    "FechaUltimoMantenimiento" DATE,
    "FechaVencimientoSeguro" DATE,
    "FechaVencimientoSoat" DATE,
    "Estado" VARCHAR(20) NOT NULL DEFAULT 'Activo' CHECK ("Estado" IN ('Activo', 'Mantenimiento', 'Inactivo')),
    "Observaciones" TEXT,
    "FechaCreacion" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "FechaActualizacion" TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX IF NOT EXISTS "idx_vehiculos_placa" ON "Vehiculos"("Placa");
CREATE INDEX IF NOT EXISTS "idx_vehiculos_estado" ON "Vehiculos"("Estado");
CREATE INDEX IF NOT EXISTS "idx_vehiculos_vencimiento_seguro" ON "Vehiculos"("FechaVencimientoSeguro");

-- =============================================
-- Tabla: Conductores
-- =============================================
CREATE TABLE IF NOT EXISTS "Conductores" (
    "Id" SERIAL PRIMARY KEY,
    "NumeroDocumento" VARCHAR(20) NOT NULL UNIQUE,
    "Nombres" VARCHAR(100) NOT NULL,
    "Apellidos" VARCHAR(100) NOT NULL,
    "Telefono" VARCHAR(20),
    "Email" VARCHAR(100),
    "NumeroLicencia" VARCHAR(50) NOT NULL,
    "FechaVencimientoLicencia" DATE NOT NULL,
    "FotoPerfil" BYTEA,
    "CuentaCorriente" DECIMAL(12, 2) DEFAULT 0.00,
    "Estado" VARCHAR(20) NOT NULL DEFAULT 'Activo' CHECK ("Estado" IN ('Activo', 'Inactivo', 'Suspendido')),
    "FechaCreacion" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "FechaActualizacion" TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX IF NOT EXISTS "idx_conductores_documento" ON "Conductores"("NumeroDocumento");
CREATE INDEX IF NOT EXISTS "idx_conductores_licencia" ON "Conductores"("NumeroLicencia");
CREATE INDEX IF NOT EXISTS "idx_conductores_estado" ON "Conductores"("Estado");
CREATE INDEX IF NOT EXISTS "idx_conductores_vencimiento_licencia" ON "Conductores"("FechaVencimientoLicencia");

-- =============================================
-- Tabla: Clientes
-- =============================================
CREATE TABLE IF NOT EXISTS "Clientes" (
    "Id" SERIAL PRIMARY KEY,
    "RazonSocial" VARCHAR(200) NOT NULL,
    "RUC" VARCHAR(20) UNIQUE,
    "Direccion" TEXT,
    "Telefono" VARCHAR(20),
    "Email" VARCHAR(100),
    "Contacto" VARCHAR(100),
    "Estado" VARCHAR(20) NOT NULL DEFAULT 'Activo' CHECK ("Estado" IN ('Activo', 'Inactivo')),
    "FechaCreacion" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "FechaActualizacion" TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX IF NOT EXISTS "idx_clientes_ruc" ON "Clientes"("RUC");
CREATE INDEX IF NOT EXISTS "idx_clientes_estado" ON "Clientes"("Estado");

-- =============================================
-- Tabla: Viajes
-- =============================================
CREATE TABLE IF NOT EXISTS "Viajes" (
    "Id" SERIAL PRIMARY KEY,
    "NumeroViaje" VARCHAR(20) NOT NULL UNIQUE,
    "VehiculoId" INTEGER NOT NULL,
    "ConductorId" INTEGER NOT NULL,
    "ClienteId" INTEGER NOT NULL,
    "FechaSalida" TIMESTAMP NOT NULL,
    "FechaLlegada" TIMESTAMP,
    "Origen" VARCHAR(200) NOT NULL,
    "Destino" VARCHAR(200) NOT NULL,
    "CantidadLitros" DECIMAL(10, 2) NOT NULL,
    "Flete" DECIMAL(12, 2) NOT NULL,
    "PagoConductor" DECIMAL(12, 2) NOT NULL,
    "Estado" VARCHAR(20) NOT NULL DEFAULT 'Programado' CHECK ("Estado" IN ('Programado', 'EnCurso', 'Completado', 'Cancelado')),
    "Observaciones" TEXT,
    "FechaCreacion" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "FechaActualizacion" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT "fk_viajes_vehiculo" FOREIGN KEY ("VehiculoId") REFERENCES "Vehiculos"("Id") ON DELETE RESTRICT,
    CONSTRAINT "fk_viajes_conductor" FOREIGN KEY ("ConductorId") REFERENCES "Conductores"("Id") ON DELETE RESTRICT,
    CONSTRAINT "fk_viajes_cliente" FOREIGN KEY ("ClienteId") REFERENCES "Clientes"("Id") ON DELETE RESTRICT
);

CREATE INDEX IF NOT EXISTS "idx_viajes_numero_viaje" ON "Viajes"("NumeroViaje");
CREATE INDEX IF NOT EXISTS "idx_viajes_fecha_salida" ON "Viajes"("FechaSalida");
CREATE INDEX IF NOT EXISTS "idx_viajes_estado" ON "Viajes"("Estado");
CREATE INDEX IF NOT EXISTS "idx_viajes_vehiculo" ON "Viajes"("VehiculoId");
CREATE INDEX IF NOT EXISTS "idx_viajes_conductor" ON "Viajes"("ConductorId");
CREATE INDEX IF NOT EXISTS "idx_viajes_cliente" ON "Viajes"("ClienteId");

-- =============================================
-- Tabla: Gastos
-- =============================================
CREATE TABLE IF NOT EXISTS "Gastos" (
    "Id" SERIAL PRIMARY KEY,
    "ViajeId" INTEGER NOT NULL,
    "Tipo" VARCHAR(20) NOT NULL CHECK ("Tipo" IN ('Combustible', 'Peaje', 'Viatico', 'Otros')),
    "Descripcion" VARCHAR(200) NOT NULL,
    "Monto" DECIMAL(10, 2) NOT NULL,
    "Fecha" TIMESTAMP NOT NULL,
    "Comprobante" BYTEA,
    "FechaCreacion" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT "fk_gastos_viaje" FOREIGN KEY ("ViajeId") REFERENCES "Viajes"("Id") ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS "idx_gastos_viaje" ON "Gastos"("ViajeId");
CREATE INDEX IF NOT EXISTS "idx_gastos_tipo" ON "Gastos"("Tipo");
CREATE INDEX IF NOT EXISTS "idx_gastos_fecha" ON "Gastos"("Fecha");

-- =============================================
-- Tabla: MovimientosCuentaCorriente
-- =============================================
CREATE TABLE IF NOT EXISTS "MovimientosCuentaCorriente" (
    "Id" SERIAL PRIMARY KEY,
    "ConductorId" INTEGER NOT NULL,
    "ViajeId" INTEGER,
    "Tipo" VARCHAR(20) NOT NULL CHECK ("Tipo" IN ('Abono', 'Cargo', 'Ajuste')),
    "Monto" DECIMAL(12, 2) NOT NULL,
    "Descripcion" VARCHAR(200) NOT NULL,
    "FechaMovimiento" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT "fk_movimientos_conductor" FOREIGN KEY ("ConductorId") REFERENCES "Conductores"("Id") ON DELETE CASCADE,
    CONSTRAINT "fk_movimientos_viaje" FOREIGN KEY ("ViajeId") REFERENCES "Viajes"("Id") ON DELETE SET NULL
);

CREATE INDEX IF NOT EXISTS "idx_movimientos_conductor" ON "MovimientosCuentaCorriente"("ConductorId");
CREATE INDEX IF NOT EXISTS "idx_movimientos_fecha" ON "MovimientosCuentaCorriente"("FechaMovimiento");
CREATE INDEX IF NOT EXISTS "idx_movimientos_viaje" ON "MovimientosCuentaCorriente"("ViajeId");

-- =============================================
-- Función para actualizar FechaActualizacion automáticamente
-- =============================================
CREATE OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW."FechaActualizacion" = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Triggers para actualizar FechaActualizacion
CREATE TRIGGER update_vehiculos_updated_at BEFORE UPDATE ON "Vehiculos"
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_conductores_updated_at BEFORE UPDATE ON "Conductores"
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_clientes_updated_at BEFORE UPDATE ON "Clientes"
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_viajes_updated_at BEFORE UPDATE ON "Viajes"
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

-- =============================================
-- Vistas para Reportes
-- =============================================

-- Vista: Resumen de Viajes por Mes
CREATE OR REPLACE VIEW "vw_ResumenViajesMes" AS
SELECT 
    TO_CHAR(v."FechaSalida", 'YYYY-MM') AS "Mes",
    COUNT(*) AS "TotalViajes",
    SUM(v."CantidadLitros") AS "TotalLitros",
    SUM(v."Flete") AS "TotalFlete",
    SUM(v."PagoConductor") AS "TotalPagoConductor",
    SUM(v."Flete" - v."PagoConductor") AS "UtilidadBruta",
    COALESCE(SUM(g."Monto"), 0) AS "TotalGastos"
FROM "Viajes" v
LEFT JOIN "Gastos" g ON v."Id" = g."ViajeId"
WHERE v."Estado" = 'Completado'
GROUP BY TO_CHAR(v."FechaSalida", 'YYYY-MM');

-- Vista: Viajes en Curso
CREATE OR REPLACE VIEW "vw_ViajesEnCurso" AS
SELECT 
    v."Id",
    v."NumeroViaje",
    v."Origen",
    v."Destino",
    v."FechaSalida",
    v."CantidadLitros",
    ve."Placa" AS "PlacaVehiculo",
    CONCAT(c."Nombres", ' ', c."Apellidos") AS "NombreConductor",
    cl."RazonSocial" AS "NombreCliente"
FROM "Viajes" v
INNER JOIN "Vehiculos" ve ON v."VehiculoId" = ve."Id"
INNER JOIN "Conductores" c ON v."ConductorId" = c."Id"
INNER JOIN "Clientes" cl ON v."ClienteId" = cl."Id"
WHERE v."Estado" = 'EnCurso';

-- =============================================
-- Funciones para Cálculos
-- =============================================

-- Función: Calcular Utilidad de Viaje
CREATE OR REPLACE FUNCTION "sp_CalcularUtilidadViaje"(p_ViajeId INTEGER)
RETURNS TABLE (
    "Id" INTEGER,
    "NumeroViaje" VARCHAR,
    "Ingreso" DECIMAL,
    "PagoConductor" DECIMAL,
    "TotalGastos" DECIMAL,
    "UtilidadNeta" DECIMAL
) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        v."Id",
        v."NumeroViaje",
        v."Flete" AS "Ingreso",
        v."PagoConductor" AS "PagoConductor",
        COALESCE(SUM(g."Monto"), 0) AS "TotalGastos",
        (v."Flete" - v."PagoConductor" - COALESCE(SUM(g."Monto"), 0)) AS "UtilidadNeta"
    FROM "Viajes" v
    LEFT JOIN "Gastos" g ON v."Id" = g."ViajeId"
    WHERE v."Id" = p_ViajeId
    GROUP BY v."Id", v."NumeroViaje", v."Flete", v."PagoConductor";
END;
$$ LANGUAGE plpgsql;

-- Función: Actualizar Cuenta Corriente del Conductor
CREATE OR REPLACE FUNCTION "sp_ActualizarCuentaCorriente"(p_ConductorId INTEGER)
RETURNS VOID AS $$
BEGIN
    UPDATE "Conductores"
    SET "CuentaCorriente" = (
        SELECT COALESCE(SUM(
            CASE 
                WHEN "Tipo" = 'Abono' THEN "Monto"
                WHEN "Tipo" = 'Cargo' THEN -"Monto"
                WHEN "Tipo" = 'Ajuste' THEN "Monto"
            END
        ), 0)
        FROM "MovimientosCuentaCorriente"
        WHERE "ConductorId" = p_ConductorId
    )
    WHERE "Id" = p_ConductorId;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- Datos de Prueba (Opcional)
-- =============================================

-- Insertar Vehículo de Prueba
INSERT INTO "Vehiculos" ("Placa", "Marca", "Modelo", "CapacidadLitros", "AnioFabricacion", "FechaVencimientoSeguro", "Estado")
VALUES ('ABC-1234', 'Volvo', 'FH16', 25000.00, 2022, CURRENT_DATE + INTERVAL '30 days', 'Activo')
ON CONFLICT ("Placa") DO NOTHING;

-- Insertar Conductor de Prueba
INSERT INTO "Conductores" ("NumeroDocumento", "Nombres", "Apellidos", "Telefono", "NumeroLicencia", "FechaVencimientoLicencia", "Estado")
VALUES ('12345678', 'Juan', 'Pérez', '987654321', 'LIC-001', CURRENT_DATE + INTERVAL '180 days', 'Activo')
ON CONFLICT ("NumeroDocumento") DO NOTHING;

-- Insertar Cliente de Prueba
INSERT INTO "Clientes" ("RazonSocial", "RUC", "Telefono", "Estado")
VALUES ('Petróleos del Norte S.A.', '20123456789', '555-1234', 'Activo')
ON CONFLICT ("RUC") DO NOTHING;
