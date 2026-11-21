/*
    003_tabla_multas.sql

    Regla de negocio:
    - Multa de ₡50 por día de atraso, con tope de ₡2000.
    - Mientras un usuario tenga multas pendientes, no puede pedir préstamos.
    - Una multa nace de un préstamo devuelto tarde.

    Decisiones (lo que pregunta el profe):
    1. Apunta a Prestamos Y a Usuarios. Con el préstamo ya se sabe el usuario, pero
       "¿este usuario tiene multas pendientes?" se pregunta en cada préstamo nuevo;
       con UsuarioId aquí se responde sin JOIN. Es una duplicación controlada.
    2. UNIQUE (PrestamoId): un préstamo se devuelve una vez, así que genera máximo una multa.
    3. DECIMAL(10, 2) para el monto: FLOAT guarda aproximaciones (0.1 + 0.2 <> 0.3).
    4. CHECK del monto entre 0 y 2000, y DiasAtraso > 0 (sin atraso no hay multa).
    5. Estado Pendiente/Pagada + FechaPago. CK_Multas_Pago amarra las dos: si está
       pagada tiene fecha, si está pendiente no.
*/
USE SGBU;
GO

IF OBJECT_ID(N'dbo.Multas', N'U') IS NULL
CREATE TABLE dbo.Multas
(
    Id                  INT            IDENTITY(1, 1) NOT NULL,
    PrestamoId          INT            NOT NULL,
    UsuarioId           INT            NOT NULL,
    DiasAtraso          INT            NOT NULL,
    Monto               DECIMAL(10, 2) NOT NULL,
    Estado              NVARCHAR(10)   NOT NULL CONSTRAINT DF_Multas_Estado DEFAULT (N'Pendiente'),
    FechaGeneracion     DATETIME2(0)   NOT NULL CONSTRAINT DF_Multas_FechaGeneracion DEFAULT (SYSDATETIME()),
    FechaPago           DATETIME2(0)   NULL,

    CONSTRAINT PK_Multas PRIMARY KEY (Id),
    CONSTRAINT FK_Multas_Prestamos FOREIGN KEY (PrestamoId) REFERENCES dbo.Prestamos (Id),
    CONSTRAINT FK_Multas_Usuarios FOREIGN KEY (UsuarioId) REFERENCES dbo.Usuarios (Id),
    CONSTRAINT UQ_Multas_Prestamo UNIQUE (PrestamoId),
    CONSTRAINT CK_Multas_DiasAtraso CHECK (DiasAtraso > 0),
    CONSTRAINT CK_Multas_Monto CHECK (Monto >= 0 AND Monto <= 2000),
    CONSTRAINT CK_Multas_Estado CHECK (Estado IN (N'Pendiente', N'Pagada')),
    CONSTRAINT CK_Multas_Pago CHECK (
        (Estado = N'Pendiente' AND FechaPago IS NULL) OR
        (Estado = N'Pagada' AND FechaPago IS NOT NULL))
);
GO
