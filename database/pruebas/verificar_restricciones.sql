/*
    verificar_restricciones.sql
    Pruebas de la base de datos: intenta guardar datos inválidos y comprueba que SQL Server
    los rechaza. Todo corre dentro de una transacción que al final se deshace (ROLLBACK),
    así que no deja basura.

    Si una prueba falla, el script termina con error y dice cuál.
    Se ejecuta con: bash database/ejecutar.sh
*/
USE SGBU;
GO
SET NOCOUNT ON;
SET XACT_ABORT OFF;

DECLARE @fallos INT = 0, @ok INT = 0, @nombre NVARCHAR(200);
DECLARE @est INT = (SELECT Id FROM dbo.Roles WHERE Nombre = N'Estudiante');
DECLARE @usuario INT = (SELECT MIN(Id) FROM dbo.Usuarios);
DECLARE @material INT = (SELECT MIN(Id) FROM dbo.Materiales);
DECLARE @hash CHAR(60) = REPLICATE('x', 60);

BEGIN TRANSACTION;

/* Cada bloque: intenta algo inválido; si NO da error, es un fallo. */
SET @nombre = N'Rechaza stock disponible negativo';
BEGIN TRY
    INSERT dbo.Materiales (Tipo, Isbn, Titulo, Autor, StockTotal, StockDisponible) VALUES (N'Libro', '9786666777784', N'X', N'X', 1, -1);
    SET @fallos += 1; PRINT N'FALLA: ' + @nombre;
END TRY BEGIN CATCH SET @ok += 1; PRINT N'ok    ' + @nombre; END CATCH;

SET @nombre = N'Rechaza disponible mayor que el total';
BEGIN TRY
    INSERT dbo.Materiales (Tipo, Isbn, Titulo, Autor, StockTotal, StockDisponible) VALUES (N'Libro', '9786666777784', N'X', N'X', 1, 2);
    SET @fallos += 1; PRINT N'FALLA: ' + @nombre;
END TRY BEGIN CATCH SET @ok += 1; PRINT N'ok    ' + @nombre; END CATCH;

SET @nombre = N'Rechaza ISBN con letras';
BEGIN TRY
    INSERT dbo.Materiales (Tipo, Isbn, Titulo, Autor, StockTotal, StockDisponible) VALUES (N'Libro', '978ABC7777784', N'X', N'X', 1, 1);
    SET @fallos += 1; PRINT N'FALLA: ' + @nombre;
END TRY BEGIN CATCH SET @ok += 1; PRINT N'ok    ' + @nombre; END CATCH;

SET @nombre = N'Rechaza ISBN repetido';
BEGIN TRY
    INSERT dbo.Materiales (Tipo, Isbn, Titulo, Autor, StockTotal, StockDisponible) VALUES (N'Libro', '9780132350884', N'X', N'X', 1, 1);
    SET @fallos += 1; PRINT N'FALLA: ' + @nombre;
END TRY BEGIN CATCH SET @ok += 1; PRINT N'ok    ' + @nombre; END CATCH;

SET @nombre = N'Rechaza tipo de material desconocido';
BEGIN TRY
    INSERT dbo.Materiales (Tipo, Isbn, Titulo, Autor, StockTotal, StockDisponible) VALUES (N'DVD', '9786666777784', N'X', N'X', 1, 1);
    SET @fallos += 1; PRINT N'FALLA: ' + @nombre;
END TRY BEGIN CATCH SET @ok += 1; PRINT N'ok    ' + @nombre; END CATCH;

SET @nombre = N'Rechaza Revista con datos de Tesis';
BEGIN TRY
    INSERT dbo.Materiales (Tipo, Isbn, Titulo, Autor, StockTotal, StockDisponible, Universidad) VALUES (N'Revista', '9786666777784', N'X', N'X', 1, 1, N'UH');
    SET @fallos += 1; PRINT N'FALLA: ' + @nombre;
END TRY BEGIN CATCH SET @ok += 1; PRINT N'ok    ' + @nombre; END CATCH;

SET @nombre = N'Rechaza Tesis sin universidad';
BEGIN TRY
    INSERT dbo.Materiales (Tipo, Isbn, Titulo, Autor, StockTotal, StockDisponible) VALUES (N'Tesis', '9786666777784', N'X', N'X', 1, 1);
    SET @fallos += 1; PRINT N'FALLA: ' + @nombre;
END TRY BEGIN CATCH SET @ok += 1; PRINT N'ok    ' + @nombre; END CATCH;

SET @nombre = N'Acepta un Libro válido';
BEGIN TRY
    INSERT dbo.Materiales (Tipo, Isbn, Titulo, Autor, StockTotal, StockDisponible, Edicion) VALUES (N'Libro', '9786666777784', N'X', N'X', 2, 2, N'1.ª');
    SET @ok += 1; PRINT N'ok    ' + @nombre;
END TRY BEGIN CATCH SET @fallos += 1; PRINT N'FALLA: ' + @nombre + N' -> ' + ERROR_MESSAGE(); END CATCH;

SET @nombre = N'Rechaza email repetido';
BEGIN TRY
    INSERT dbo.Usuarios (Carne, Dni, Nombre, Apellidos, Email, RolId, PasswordHash) VALUES (N'T-1', N'T-1', N'X', N'X', N'admin@sgbu.ac.cr', @est, @hash);
    SET @fallos += 1; PRINT N'FALLA: ' + @nombre;
END TRY BEGIN CATCH SET @ok += 1; PRINT N'ok    ' + @nombre; END CATCH;

SET @nombre = N'Rechaza carné repetido';
BEGIN TRY
    INSERT dbo.Usuarios (Carne, Dni, Nombre, Apellidos, Email, RolId, PasswordHash) VALUES (N'EST-1001', N'T-2', N'X', N'X', N't2@x.cr', @est, @hash);
    SET @fallos += 1; PRINT N'FALLA: ' + @nombre;
END TRY BEGIN CATCH SET @ok += 1; PRINT N'ok    ' + @nombre; END CATCH;

SET @nombre = N'Rechaza usuario con rol inexistente';
BEGIN TRY
    INSERT dbo.Usuarios (Carne, Dni, Nombre, Apellidos, Email, RolId, PasswordHash) VALUES (N'T-3', N'T-3', N'X', N'X', N't3@x.cr', 999, @hash);
    SET @fallos += 1; PRINT N'FALLA: ' + @nombre;
END TRY BEGIN CATCH SET @ok += 1; PRINT N'ok    ' + @nombre; END CATCH;

SET @nombre = N'Rechaza préstamo que vence antes de prestarse';
BEGIN TRY
    INSERT dbo.Prestamos (UsuarioId, MaterialId, FechaPrestamo, FechaVencimiento) VALUES (@usuario, @material, SYSDATETIME(), DATEADD(DAY, -1, CAST(SYSDATETIME() AS DATE)));
    SET @fallos += 1; PRINT N'FALLA: ' + @nombre;
END TRY BEGIN CATCH SET @ok += 1; PRINT N'ok    ' + @nombre; END CATCH;

SET @nombre = N'Rechaza préstamo de material inexistente';
BEGIN TRY
    INSERT dbo.Prestamos (UsuarioId, MaterialId, FechaVencimiento) VALUES (@usuario, 999999, CAST(SYSDATETIME() AS DATE));
    SET @fallos += 1; PRINT N'FALLA: ' + @nombre;
END TRY BEGIN CATCH SET @ok += 1; PRINT N'ok    ' + @nombre; END CATCH;

SET @nombre = N'Rechaza estado de reserva desconocido';
BEGIN TRY
    INSERT dbo.Reservas (UsuarioId, MaterialId, Estado) VALUES (@usuario, @material, N'Perdida');
    SET @fallos += 1; PRINT N'FALLA: ' + @nombre;
END TRY BEGIN CATCH SET @ok += 1; PRINT N'ok    ' + @nombre; END CATCH;

/* Coherencia de los datos iniciales */
SET @nombre = N'Seed: préstamos activos = total - disponible en cada material';
IF EXISTS (
    SELECT 1 FROM dbo.Materiales m
    WHERE m.StockTotal - m.StockDisponible <> (SELECT COUNT(*) FROM dbo.Prestamos p WHERE p.MaterialId = m.Id AND p.FechaDevolucion IS NULL)
)
BEGIN SET @fallos += 1; PRINT N'FALLA: ' + @nombre; END
ELSE BEGIN SET @ok += 1; PRINT N'ok    ' + @nombre; END

SET @nombre = N'Seed: hay préstamos que vencen hoy y materiales con stock crítico';
IF EXISTS (SELECT 1 FROM dbo.Prestamos WHERE FechaDevolucion IS NULL AND FechaVencimiento = CAST(SYSDATETIME() AS DATE))
   AND EXISTS (SELECT 1 FROM dbo.Materiales WHERE StockDisponible <= 3)
BEGIN SET @ok += 1; PRINT N'ok    ' + @nombre; END
ELSE BEGIN SET @fallos += 1; PRINT N'FALLA: ' + @nombre; END

/* Multas: se activan solas cuando exista la tabla . */
IF OBJECT_ID(N'dbo.Multas', N'U') IS NULL
    PRINT N'PEND  Multas: la tabla todavía no existe (003_tabla_multas.sql)';
ELSE
BEGIN
    DECLARE @prestamo INT = (SELECT MIN(Id) FROM dbo.Prestamos);
    DECLARE @sql NVARCHAR(MAX);

    SET @nombre = N'Multas: rechaza monto mayor a 2000';
    SET @sql = N'INSERT dbo.Multas (PrestamoId, UsuarioId, DiasAtraso, Monto) VALUES (@p, @u, 50, 2500);';
    BEGIN TRY
        EXEC sp_executesql @sql, N'@p INT, @u INT', @prestamo, @usuario;
        SET @fallos += 1; PRINT N'FALLA: ' + @nombre;
    END TRY BEGIN CATCH SET @ok += 1; PRINT N'ok    ' + @nombre; END CATCH;

    SET @nombre = N'Multas: rechaza monto negativo';
    SET @sql = N'INSERT dbo.Multas (PrestamoId, UsuarioId, DiasAtraso, Monto) VALUES (@p, @u, 1, -50);';
    BEGIN TRY
        EXEC sp_executesql @sql, N'@p INT, @u INT', @prestamo, @usuario;
        SET @fallos += 1; PRINT N'FALLA: ' + @nombre;
    END TRY BEGIN CATCH SET @ok += 1; PRINT N'ok    ' + @nombre; END CATCH;

    SET @nombre = N'Multas: rechaza dos multas para el mismo préstamo';
    SET @sql = N'INSERT dbo.Multas (PrestamoId, UsuarioId, DiasAtraso, Monto) VALUES (@p, @u, 2, 100);
                 INSERT dbo.Multas (PrestamoId, UsuarioId, DiasAtraso, Monto) VALUES (@p, @u, 2, 100);';
    BEGIN TRY
        EXEC sp_executesql @sql, N'@p INT, @u INT', @prestamo, @usuario;
        SET @fallos += 1; PRINT N'FALLA: ' + @nombre;
    END TRY BEGIN CATCH SET @ok += 1; PRINT N'ok    ' + @nombre; END CATCH;

    SET @nombre = N'Multas: rechaza multa pagada sin fecha de pago';
    SET @sql = N'INSERT dbo.Multas (PrestamoId, UsuarioId, DiasAtraso, Monto, Estado) VALUES (@p, @u, 1, 50, N''Pagada'');';
    BEGIN TRY
        EXEC sp_executesql @sql, N'@p INT, @u INT', @prestamo, @usuario;
        SET @fallos += 1; PRINT N'FALLA: ' + @nombre;
    END TRY BEGIN CATCH SET @ok += 1; PRINT N'ok    ' + @nombre; END CATCH;

    SET @nombre = N'Seed: EST-1002 tiene una multa pendiente de 400 (8 días tarde)';
    IF EXISTS (SELECT 1 FROM dbo.Multas mu JOIN dbo.Usuarios u ON u.Id = mu.UsuarioId
               WHERE u.Carne = N'EST-1002' AND mu.Estado = N'Pendiente' AND mu.Monto = 400 AND mu.DiasAtraso = 8)
    BEGIN SET @ok += 1; PRINT N'ok    ' + @nombre; END
    ELSE BEGIN SET @fallos += 1; PRINT N'FALLA: ' + @nombre; END
END

IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;

PRINT N'';
PRINT CONCAT(N'Resultado: ', @ok, N' pasan, ', @fallos, N' fallan');
IF @fallos > 0 THROW 50000, N'Hay pruebas de base de datos que fallan', 1;
GO
