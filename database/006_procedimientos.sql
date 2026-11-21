/*
    006_procedimientos.sql
    Stored procedures de las operaciones críticas: prestar, devolver y renovar.

    Por qué en stored procedures: cada operación cambia varias tablas a la vez (préstamo,
    stock, multa, auditoría). Dentro del procedimiento todo va en una transacción: o se
    guarda todo, o no se guarda nada.

    Transacción: si C# ya abrió una (Unit of Work), el procedimiento trabaja dentro de esa.
    Si no hay ninguna (por ejemplo, al probarlo desde SSMS), abre la suya.

    Errores: se usa THROW con números 50001 en adelante y un mensaje para el usuario.
    C# los recibe como SqlException y muestra el mensaje.
*/
USE SGBU;
GO

/* ---------- Registrar un préstamo ---------- */
CREATE OR ALTER PROCEDURE dbo.sp_RegistrarPrestamo
    @UsuarioId       INT,
    @MaterialId      INT,
    @RegistradoPorId INT,
    @PrestamoId      INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @transaccionPropia BIT = 0;
    DECLARE @activo BIT, @limite INT, @dias INT, @prestamosActivos INT;

    SELECT @activo = u.Activo, @limite = r.LimitePrestamos, @dias = r.DiasPrestamo
    FROM dbo.Usuarios u
    INNER JOIN dbo.Roles r ON r.Id = u.RolId
    WHERE u.Id = @UsuarioId;

    IF @activo IS NULL
        THROW 50001, N'El usuario no existe.', 1;
    IF @activo = 0
        THROW 50002, N'El usuario está inactivo.', 1;
    IF EXISTS (SELECT 1 FROM dbo.Multas WHERE UsuarioId = @UsuarioId AND Estado = N'Pendiente')
        THROW 50003, N'El usuario tiene multas pendientes. Debe pagarlas antes de un nuevo préstamo.', 1;

    SELECT @prestamosActivos = COUNT(*) FROM dbo.Prestamos WHERE UsuarioId = @UsuarioId AND FechaDevolucion IS NULL;
    IF @prestamosActivos >= @limite
        THROW 50004, N'El usuario ya tiene el máximo de préstamos permitidos para su rol.', 1;

    IF @@TRANCOUNT = 0
    BEGIN
        BEGIN TRANSACTION;
        SET @transaccionPropia = 1;
    END

    BEGIN TRY
        -- Se resta 1 al stock SOLO si hay disponible. Si dos personas piden el último
        -- ejemplar al mismo tiempo, solo a una le funciona el UPDATE (la otra ve 0 filas).
        UPDATE dbo.Materiales
        SET StockDisponible = StockDisponible - 1
        WHERE Id = @MaterialId AND Activo = 1 AND StockDisponible > 0;

        IF @@ROWCOUNT = 0
            THROW 50005, N'No hay ejemplares disponibles de este material.', 1;

        INSERT INTO dbo.Prestamos (UsuarioId, MaterialId, FechaPrestamo, FechaVencimiento, RegistradoPorId)
        VALUES (@UsuarioId, @MaterialId, SYSDATETIME(), DATEADD(DAY, @dias, CAST(SYSDATETIME() AS DATE)), @RegistradoPorId);

        SET @PrestamoId = SCOPE_IDENTITY();

        INSERT INTO dbo.Auditoria (UsuarioId, Accion, Entidad, EntidadId)
        VALUES (@RegistradoPorId, N'PRESTAMO', N'Prestamos', @PrestamoId);

        IF @transaccionPropia = 1
            COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @transaccionPropia = 1 AND @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

/* ---------- Registrar una devolución ----------
   Devuelve los días de atraso y el monto de la multa (0 si llegó a tiempo).
   Multa: ₡50 por día, con tope de ₡2000. */
CREATE OR ALTER PROCEDURE dbo.sp_RegistrarDevolucion
    @PrestamoId      INT,
    @RegistradoPorId INT,
    @DiasAtraso      INT OUTPUT,
    @Monto           DECIMAL(10, 2) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @transaccionPropia BIT = 0;
    DECLARE @usuarioId INT, @materialId INT, @vencimiento DATE, @devuelto DATETIME2(0);

    SELECT @usuarioId = UsuarioId, @materialId = MaterialId, @vencimiento = FechaVencimiento, @devuelto = FechaDevolucion
    FROM dbo.Prestamos
    WHERE Id = @PrestamoId;

    IF @usuarioId IS NULL
        THROW 50011, N'El préstamo no existe.', 1;
    IF @devuelto IS NOT NULL
        THROW 50012, N'Este préstamo ya fue devuelto.', 1;

    SET @DiasAtraso = DATEDIFF(DAY, @vencimiento, CAST(SYSDATETIME() AS DATE));
    IF @DiasAtraso < 0
        SET @DiasAtraso = 0;

    SET @Monto = @DiasAtraso * 50;
    IF @Monto > 2000
        SET @Monto = 2000;

    IF @@TRANCOUNT = 0
    BEGIN
        BEGIN TRANSACTION;
        SET @transaccionPropia = 1;
    END

    BEGIN TRY
        UPDATE dbo.Prestamos SET FechaDevolucion = SYSDATETIME() WHERE Id = @PrestamoId;
        UPDATE dbo.Materiales SET StockDisponible = StockDisponible + 1 WHERE Id = @materialId;

        IF @DiasAtraso > 0
            INSERT INTO dbo.Multas (PrestamoId, UsuarioId, DiasAtraso, Monto)
            VALUES (@PrestamoId, @usuarioId, @DiasAtraso, @Monto);

        INSERT INTO dbo.Auditoria (UsuarioId, Accion, Entidad, EntidadId, Detalle)
        VALUES (@RegistradoPorId, N'DEVOLUCION', N'Prestamos', @PrestamoId,
                CASE WHEN @DiasAtraso > 0 THEN CONCAT(N'Atraso de ', @DiasAtraso, N' días, multa ₡', @Monto) END);

        IF @transaccionPropia = 1
            COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @transaccionPropia = 1 AND @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

/* ---------- Renovar un préstamo ----------
   Reglas: una sola vez, no si está vencido y no si otra persona reservó el material.
   El nuevo vencimiento es el anterior más los días que da el rol. */
CREATE OR ALTER PROCEDURE dbo.sp_RenovarPrestamo
    @PrestamoId      INT,
    @RegistradoPorId INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @materialId INT, @vencimiento DATE, @devuelto DATETIME2(0), @renovado BIT, @dias INT;

    SELECT @materialId = p.MaterialId, @vencimiento = p.FechaVencimiento, @devuelto = p.FechaDevolucion,
           @renovado = p.Renovado, @dias = r.DiasPrestamo
    FROM dbo.Prestamos p
    INNER JOIN dbo.Usuarios u ON u.Id = p.UsuarioId
    INNER JOIN dbo.Roles r ON r.Id = u.RolId
    WHERE p.Id = @PrestamoId;

    IF @materialId IS NULL
        THROW 50021, N'El préstamo no existe.', 1;
    IF @devuelto IS NOT NULL
        THROW 50022, N'No se puede renovar: el préstamo ya fue devuelto.', 1;
    IF @renovado = 1
        THROW 50023, N'No se puede renovar: el préstamo ya se renovó una vez.', 1;
    IF @vencimiento < CAST(SYSDATETIME() AS DATE)
        THROW 50024, N'No se puede renovar: el préstamo está vencido.', 1;
    IF EXISTS (SELECT 1 FROM dbo.Reservas WHERE MaterialId = @materialId AND Estado = N'Pendiente')
        THROW 50025, N'No se puede renovar: otro usuario reservó este material.', 1;

    -- Un solo UPDATE y un INSERT; si falla el INSERT, la transacción de C# lo deshace.
    UPDATE dbo.Prestamos
    SET FechaVencimiento = DATEADD(DAY, @dias, FechaVencimiento), Renovado = 1
    WHERE Id = @PrestamoId;

    INSERT INTO dbo.Auditoria (UsuarioId, Accion, Entidad, EntidadId)
    VALUES (@RegistradoPorId, N'RENOVACION', N'Prestamos', @PrestamoId);
END
GO

/* ---------- Pagar una multa ----------
   Al pagarla, el usuario puede volver a pedir préstamos (si no tiene otras pendientes). */
CREATE OR ALTER PROCEDURE dbo.sp_PagarMulta
    @MultaId         INT,
    @RegistradoPorId INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @estado NVARCHAR(10), @monto DECIMAL(10, 2);

    SELECT @estado = Estado, @monto = Monto FROM dbo.Multas WHERE Id = @MultaId;

    IF @estado IS NULL
        THROW 50031, N'La multa no existe.', 1;
    IF @estado = N'Pagada'
        THROW 50032, N'La multa ya está pagada.', 1;

    UPDATE dbo.Multas SET Estado = N'Pagada', FechaPago = SYSDATETIME() WHERE Id = @MultaId;

    INSERT INTO dbo.Auditoria (UsuarioId, Accion, Entidad, EntidadId, Detalle)
    VALUES (@RegistradoPorId, N'PAGO_MULTA', N'Multas', @MultaId, CONCAT(N'Pago de ₡', @monto));
END
GO
