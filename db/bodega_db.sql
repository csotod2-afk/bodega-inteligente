-- =====================================================================
--  Bodega Inteligente - Script de base de datos (MySQL)
--  Base: bodega_db   Tablas: espacios, paquetes, historial
--
--  Regla de capacidad (ver docs/DER.md): cada espacio tiene 6 unidades.
--    GRANDE = 6 unidades  -> cabe 1 por espacio
--    MEDIANO = 3 unidades -> caben 2 por espacio
--    PEQUENO = 2 unidades -> caben 3 por espacio
--
--  Uso:  mysql -u root -p < db/bodega_db.sql
-- =====================================================================

DROP DATABASE IF EXISTS bodega_db;
CREATE DATABASE bodega_db CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE bodega_db;

-- ---------------------------------------------------------------------
-- Espacios físicos de la bodega (los cuadros del mapa visual)
-- ---------------------------------------------------------------------
CREATE TABLE espacios (
    id_espacio      VARCHAR(10) NOT NULL,
    zona            VARCHAR(20) NOT NULL,
    soporta_fragil  TINYINT(1)  NOT NULL DEFAULT 0,
    capacidad_total INT         NOT NULL DEFAULT 6,
    PRIMARY KEY (id_espacio),
    CONSTRAINT chk_espacio_capacidad CHECK (capacidad_total > 0)
) ENGINE = InnoDB;

-- ---------------------------------------------------------------------
-- Paquetes registrados en el sistema
-- ---------------------------------------------------------------------
CREATE TABLE paquetes (
    id_paquete        VARCHAR(10) NOT NULL,
    tipo              ENUM('NORMAL', 'FRAGIL')                       NOT NULL,
    tamano            ENUM('PEQUENO', 'MEDIANO', 'GRANDE')           NOT NULL,
    estado            ENUM('REGISTRADO', 'ALMACENADO',
                           'AUTORIZADO_RETIRO', 'RETIRADO')          NOT NULL DEFAULT 'REGISTRADO',
    id_espacio        VARCHAR(10) NULL,
    retiro_autorizado TINYINT(1)  NOT NULL DEFAULT 0,
    fecha_registro    DATETIME    NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (id_paquete),
    CONSTRAINT fk_paquete_espacio FOREIGN KEY (id_espacio)
        REFERENCES espacios (id_espacio)
        ON UPDATE CASCADE ON DELETE SET NULL
) ENGINE = InnoDB;

-- ---------------------------------------------------------------------
-- Bitácora de cada comando ejecutado con éxito (y de las alarmas)
-- ---------------------------------------------------------------------
CREATE TABLE historial (
    id_historial INT         NOT NULL AUTO_INCREMENT,
    id_paquete   VARCHAR(10) NULL,
    accion       VARCHAR(30) NOT NULL,   -- REGISTRAR, ALMACENAR, RETIRAR, ALARMA...
    detalle      VARCHAR(200) NULL,
    fecha        DATETIME    NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (id_historial)
) ENGINE = InnoDB;

-- ---------------------------------------------------------------------
-- Vista para el mapa: estado de cada espacio (verde / amarillo / rojo)
--   LIBRE = 0 unidades ocupadas, PARCIAL = algunas, LLENO = todas
-- ---------------------------------------------------------------------
CREATE VIEW vista_estado_espacios AS
SELECT  e.id_espacio,
        e.zona,
        e.soporta_fragil,
        e.capacidad_total,
        COALESCE(SUM(CASE p.tamano WHEN 'GRANDE' THEN 6
                                   WHEN 'MEDIANO' THEN 3
                                   WHEN 'PEQUENO' THEN 2 END), 0) AS unidades_ocupadas,
        CASE
            WHEN COALESCE(SUM(CASE p.tamano WHEN 'GRANDE' THEN 6 WHEN 'MEDIANO' THEN 3
                                            WHEN 'PEQUENO' THEN 2 END), 0) = 0
                THEN 'LIBRE'
            WHEN COALESCE(SUM(CASE p.tamano WHEN 'GRANDE' THEN 6 WHEN 'MEDIANO' THEN 3
                                            WHEN 'PEQUENO' THEN 2 END), 0) >= e.capacidad_total
                THEN 'LLENO'
            ELSE 'PARCIAL'
        END AS estado
FROM    espacios e
LEFT JOIN paquetes p
       ON p.id_espacio = e.id_espacio AND p.estado = 'ALMACENADO'
GROUP BY e.id_espacio, e.zona, e.soporta_fragil, e.capacidad_total;

-- ---------------------------------------------------------------------
-- Datos iniciales: 6 espacios (zona A apta para frágiles)
-- ---------------------------------------------------------------------
INSERT INTO espacios (id_espacio, zona, soporta_fragil, capacidad_total) VALUES
    ('E01', 'A', 1, 6),
    ('E02', 'A', 1, 6),
    ('E03', 'B', 0, 6),
    ('E04', 'B', 0, 6),
    ('E05', 'C', 0, 6),
    ('E06', 'C', 0, 6);
