-- Esquema inicial para MySQL 8.0.16 o superior (CHECK aplicados).
-- Ejecutar sobre una base nueva: no es una migración ni un script de reinicio.
-- No elimina tablas ni datos existentes. Las tablas existentes provocan error.
CREATE DATABASE IF NOT EXISTS ECommerceJuegos
    CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci;
USE ECommerceJuegos;

CREATE TABLE Usuarios (
    UsuarioId INT NOT NULL AUTO_INCREMENT,
    Email VARCHAR(254) NOT NULL,
    ContraseniaHash VARCHAR(255) NOT NULL,
    Rol VARCHAR(20) COLLATE utf8mb4_0900_as_cs NOT NULL,
    AvatarUrl VARCHAR(255) NULL,
    EstadoActivo BOOLEAN NOT NULL DEFAULT TRUE,
    PRIMARY KEY (UsuarioId),
    CONSTRAINT UQ_Usuarios_Email UNIQUE (Email),
    CONSTRAINT CK_Usuarios_Rol CHECK (Rol IN ('Administrador', 'Cliente')),
    CONSTRAINT CK_Usuarios_EstadoActivo CHECK (EstadoActivo IN (0, 1))
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE Categorias (
    CategoriaId INT NOT NULL AUTO_INCREMENT,
    Nombre VARCHAR(100) NOT NULL,
    PRIMARY KEY (CategoriaId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE Productos (
    ProductoId INT NOT NULL AUTO_INCREMENT,
    Nombre VARCHAR(150) NOT NULL,
    Descripcion VARCHAR(4000) NOT NULL,
    Precio DECIMAL(10,2) NOT NULL,
    CategoriaId INT NOT NULL,
    EstadoActivo BOOLEAN NOT NULL DEFAULT TRUE,
    PortadaRuta VARCHAR(255) NULL,
    PRIMARY KEY (ProductoId),
    CONSTRAINT CK_Productos_Precio CHECK (Precio >= 0),
    CONSTRAINT CK_Productos_EstadoActivo CHECK (EstadoActivo IN (0, 1)),
    INDEX IX_Productos_CategoriaId (CategoriaId),
    INDEX IX_Productos_Nombre (Nombre),
    CONSTRAINT FK_Productos_Categorias FOREIGN KEY (CategoriaId)
        REFERENCES Categorias (CategoriaId) ON DELETE RESTRICT ON UPDATE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE ClavesDigitales (
    ClaveDigitalId INT NOT NULL AUTO_INCREMENT,
    ProductoId INT NOT NULL,
    -- Comparación exacta de códigos; la normalización se definirá en el ABM.
    Codigo VARCHAR(255) COLLATE utf8mb4_0900_as_cs NOT NULL,
    Estado VARCHAR(20) COLLATE utf8mb4_0900_as_cs NOT NULL DEFAULT 'Disponible',
    PRIMARY KEY (ClaveDigitalId),
    CONSTRAINT UQ_ClavesDigitales_Codigo UNIQUE (Codigo),
    CONSTRAINT CK_ClavesDigitales_Estado CHECK (Estado IN ('Disponible', 'Vendida')),
    INDEX IX_ClavesDigitales_Producto_Estado (ProductoId, Estado, ClaveDigitalId),
    CONSTRAINT FK_ClavesDigitales_Productos FOREIGN KEY (ProductoId)
        REFERENCES Productos (ProductoId) ON DELETE RESTRICT ON UPDATE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE Ordenes (
    OrdenId INT NOT NULL AUTO_INCREMENT,
    UsuarioId INT NOT NULL,
    -- Fecha explícita: el backend establecerá la convención horaria.
    Fecha DATETIME NOT NULL,
    Estado VARCHAR(20) COLLATE utf8mb4_0900_as_cs NOT NULL DEFAULT 'Pendiente',
    MontoTotal DECIMAL(10,2) NOT NULL,
    PRIMARY KEY (OrdenId),
    CONSTRAINT CK_Ordenes_Estado CHECK (Estado IN ('Pendiente', 'Pagada', 'Cancelada')),
    CONSTRAINT CK_Ordenes_MontoTotal CHECK (MontoTotal >= 0),
    INDEX IX_Ordenes_Usuario_Fecha (UsuarioId, Fecha, OrdenId),
    INDEX IX_Ordenes_Fecha (Fecha, OrdenId),
    CONSTRAINT FK_Ordenes_Usuarios FOREIGN KEY (UsuarioId)
        REFERENCES Usuarios (UsuarioId) ON DELETE RESTRICT ON UPDATE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE OrdenDetalles (
    OrdenDetalleId INT NOT NULL AUTO_INCREMENT,
    OrdenId INT NOT NULL,
    ProductoId INT NOT NULL,
    -- El backend verificará que la clave pertenezca al producto del detalle.
    ClaveDigitalId INT NOT NULL,
    PrecioUnitario DECIMAL(10,2) NOT NULL,
    PRIMARY KEY (OrdenDetalleId),
    CONSTRAINT UQ_OrdenDetalles_ClaveDigitalId UNIQUE (ClaveDigitalId),
    CONSTRAINT CK_OrdenDetalles_PrecioUnitario CHECK (PrecioUnitario >= 0),
    INDEX IX_OrdenDetalles_OrdenId (OrdenId),
    INDEX IX_OrdenDetalles_ProductoId (ProductoId),
    CONSTRAINT FK_OrdenDetalles_Ordenes FOREIGN KEY (OrdenId)
        REFERENCES Ordenes (OrdenId) ON DELETE RESTRICT ON UPDATE RESTRICT,
    CONSTRAINT FK_OrdenDetalles_Productos FOREIGN KEY (ProductoId)
        REFERENCES Productos (ProductoId) ON DELETE RESTRICT ON UPDATE RESTRICT,
    CONSTRAINT FK_OrdenDetalles_ClavesDigitales FOREIGN KEY (ClaveDigitalId)
        REFERENCES ClavesDigitales (ClaveDigitalId) ON DELETE RESTRICT ON UPDATE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE Resenas (
    ResenaId INT NOT NULL AUTO_INCREMENT,
    UsuarioId INT NOT NULL,
    ProductoId INT NOT NULL,
    Puntuacion INT NOT NULL,
    Comentario VARCHAR(2000) NOT NULL,
    Fecha DATETIME NOT NULL,
    PRIMARY KEY (ResenaId),
    CONSTRAINT UQ_Resenas_Usuario_Producto UNIQUE (UsuarioId, ProductoId),
    CONSTRAINT CK_Resenas_Puntuacion CHECK (Puntuacion BETWEEN 1 AND 5),
    INDEX IX_Resenas_Producto_Fecha (ProductoId, Fecha, ResenaId),
    CONSTRAINT FK_Resenas_Usuarios FOREIGN KEY (UsuarioId)
        REFERENCES Usuarios (UsuarioId) ON DELETE RESTRICT ON UPDATE RESTRICT,
    CONSTRAINT FK_Resenas_Productos FOREIGN KEY (ProductoId)
        REFERENCES Productos (ProductoId) ON DELETE RESTRICT ON UPDATE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Datos exclusivamente académicos. Hashes generados con PasswordHasher<Usuario>.
-- Acceso de prueba: admin@example.test / AdminPrueba123!
-- Acceso de prueba: cliente@example.test / ClientePrueba123!
-- Las contraseñas se documentan aquí; los INSERT almacenan únicamente hashes.
-- No se crean órdenes ni reseñas. Los códigos siguientes son ficticios.
START TRANSACTION;

INSERT INTO Usuarios (UsuarioId, Email, ContraseniaHash, Rol, AvatarUrl, EstadoActivo) VALUES
    (1, 'admin@example.test', 'AQAAAAIAAYagAAAAENYYf0ARccEnTMVOmZdLu5o1HWYg00Y1CCX0YBm8fILXUSwHOYqKU+ToLmbJYojXUg==', 'Administrador', NULL, TRUE),
    (2, 'cliente@example.test', 'AQAAAAIAAYagAAAAEIxMumLmllAFzeYDZCoEF46AK0lycjuISa3ZFpQZpv6Nnk0tnjSI+TGOxr7mpqS9aA==', 'Cliente', NULL, TRUE);

INSERT INTO Categorias (CategoriaId, Nombre) VALUES
    (1, 'Videojuegos'),
    (2, 'Ofimática');

INSERT INTO Productos (ProductoId, Nombre, Descripcion, Precio, CategoriaId, EstadoActivo, PortadaRuta) VALUES
    (1, 'Juego de ejemplo', 'Videojuego ficticio para pruebas académicas.', 15000.00, 1, TRUE, NULL),
    (2, 'Suite de oficina de ejemplo', 'Software ficticio para pruebas académicas.', 25000.00, 2, TRUE, NULL);

INSERT INTO ClavesDigitales (ClaveDigitalId, ProductoId, Codigo, Estado) VALUES
    (1, 1, 'DEMO-JUEGO-001', 'Disponible'),
    (2, 1, 'DEMO-JUEGO-002', 'Disponible'),
    (3, 2, 'DEMO-OFICINA-001', 'Disponible');

COMMIT;