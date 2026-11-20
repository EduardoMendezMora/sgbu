/*
    005_datos_iniciales.sql
    Datos de prueba (seed) para desarrollar y para la demostración.

    Todos los usuarios de prueba tienen la contraseña: Sgbu2026!
    (guardada como hash BCrypt; BCrypt.Net la verifica igual que una generada en C#).

    Los inserts revisan si el dato ya existe, así el script se puede correr varias veces.
    Los préstamos usan fechas relativas a hoy (DATEADD) para que los reportes
    "vencidos hoy" y "atrasados" siempre tengan algo que mostrar en la demo.
*/
USE SGBU;
GO
SET NOCOUNT ON;

/* ---------- Roles ----------
   Límites propuestos (no vienen en el enunciado; confirmar con el grupo/profe). */
MERGE dbo.Roles AS destino
USING (VALUES
    (N'Administrador', 10, 30),
    (N'Profesor',       5, 14),
    (N'Estudiante',     3,  7)
) AS origen (Nombre, LimitePrestamos, DiasPrestamo)
ON destino.Nombre = origen.Nombre
WHEN NOT MATCHED THEN
    INSERT (Nombre, LimitePrestamos, DiasPrestamo)
    VALUES (origen.Nombre, origen.LimitePrestamos, origen.DiasPrestamo);

/* ---------- Usuarios ---------- */
DECLARE @hash CHAR(60) = '$2b$11$vjXiYk5DCKFhMyyikFj60.hwFToLc0LKwceoGPBPZZyC8F7QIm4Vy';
DECLARE @admin INT = (SELECT Id FROM dbo.Roles WHERE Nombre = N'Administrador');
DECLARE @prof  INT = (SELECT Id FROM dbo.Roles WHERE Nombre = N'Profesor');
DECLARE @est   INT = (SELECT Id FROM dbo.Roles WHERE Nombre = N'Estudiante');

MERGE dbo.Usuarios AS destino
USING (VALUES
    (N'ADM-001',  N'101110111', N'Ana',     N'Rojas Mora',      N'admin@sgbu.ac.cr',              N'8888-0001', @admin),
    (N'PRF-001',  N'102220222', N'Ricardo', N'Salas Vega',      N'rsalas@sgbu.ac.cr',           N'8888-0002', @prof),
    (N'PRF-002',  N'103330333', N'Laura',   N'Chaves Solís',    N'lchaves@sgbu.ac.cr',            N'8888-0003', @prof),
    (N'EST-1001', N'104440444', N'Carlos',  N'Jiménez Araya',   N'carlos.jimenez@estudiante.ac.cr',N'7000-1001', @est),
    (N'EST-1002', N'105550555', N'María',   N'Vargas Quesada',  N'maria.vargas@estudiante.ac.cr', N'7000-1002', @est),
    (N'EST-1003', N'106660666', N'José',    N'Castro Núñez',    N'jose.castro@estudiante.ac.cr',  N'7000-1003', @est),
    (N'EST-1004', N'107770777', N'Daniela', N'Méndez Ulate',    N'daniela.mendez@estudiante.ac.cr',N'7000-1004', @est),
    (N'EST-1005', N'108880888', N'Luis',    N'Herrera Campos',  N'luis.herrera@estudiante.ac.cr', N'7000-1005', @est)
) AS origen (Carne, Dni, Nombre, Apellidos, Email, Telefono, RolId)
ON destino.Carne = origen.Carne
WHEN NOT MATCHED THEN
    INSERT (Carne, Dni, Nombre, Apellidos, Email, Telefono, RolId, PasswordHash)
    VALUES (origen.Carne, origen.Dni, origen.Nombre, origen.Apellidos, origen.Email, origen.Telefono, origen.RolId, @hash);

/* ---------- Materiales ----------
   ISBN-13 con dígito verificador válido. Varios con stock ≤ 3 para el reporte de stock crítico. */
MERGE dbo.Materiales AS destino
USING (VALUES
    -- Tipo,      Isbn,            Titulo,                                         Autor,                     Editorial,          Año,  Categoria,       Total, Disp, Edicion, NumeroVolumen, Periodicidad, Universidad,                        Grado,          Tutor
    (N'Libro',   '9780132350884', N'Clean Code',                                  N'Robert C. Martin',        N'Prentice Hall',    2008, N'Programación',   5, 4, N'1.ª',  NULL,          NULL,         NULL,                               NULL,           NULL),
    (N'Libro',   '9780201633610', N'Design Patterns',                             N'Gamma, Helm, Johnson, Vlissides', N'Addison-Wesley', 1994, N'Programación', 3, 2, N'1.ª', NULL, NULL,       NULL,                               NULL,           NULL),
    (N'Libro',   '9780134685991', N'Effective Java',                              N'Joshua Bloch',            N'Addison-Wesley',   2018, N'Programación',   4, 4, N'3.ª',  NULL,          NULL,         NULL,                               NULL,           NULL),
    (N'Libro',   '9788498745672', N'Fundamentos de bases de datos',               N'Silberschatz, Korth, Sudarshan', N'McGraw-Hill', 2014, N'Bases de datos', 6, 5, N'6.ª', NULL,    NULL,         NULL,                               NULL,           NULL),
    (N'Libro',   '9786073212342', N'Ingeniería de software',                      N'Ian Sommerville',         N'Pearson',          2011, N'Ingeniería',     2, 1, N'9.ª',  NULL,          NULL,         NULL,                               NULL,           NULL),
    (N'Libro',   '9789580001119', N'Cálculo de una variable',                     N'James Stewart',           N'Cengage',          2012, N'Matemática',     8, 8, N'7.ª',  NULL,          NULL,         NULL,                               NULL,           NULL),
    (N'Libro',   '9789876543217', N'Estructuras de datos en C#',                  N'Luis Joyanes Aguilar',    N'McGraw-Hill',      2015, N'Programación',   1, 0, N'1.ª',  NULL,          NULL,         NULL,                               NULL,           NULL),
    (N'Libro',   '9780306406157', N'Redes de computadoras',                       N'Andrew S. Tanenbaum',     N'Pearson',          2012, N'Redes',          4, 3, N'5.ª',  NULL,          NULL,         NULL,                               NULL,           NULL),
    (N'Revista', '9781234567897', N'Communications of the ACM',                   N'ACM',                     N'ACM',              2025, N'Computación',    3, 3, NULL,    N'Vol. 68 n.º 4', N'Mensual', NULL,                               NULL,           NULL),
    (N'Revista', '9781111222239', N'IEEE Software',                               N'IEEE Computer Society',   N'IEEE',             2025, N'Ingeniería',     2, 2, NULL,    N'Vol. 42 n.º 2', N'Bimestral', NULL,                             NULL,           NULL),
    (N'Revista', '9782222333340', N'Revista de Ciencias Económicas',              N'Universidad de Costa Rica', N'UCR',            2024, N'Economía',       5, 5, NULL,    N'Vol. 42 n.º 1', N'Semestral', NULL,                             NULL,           NULL),
    (N'Tesis',   '9783333444451', N'Sistema de reservas en línea para clínicas',  N'Valeria Soto Brenes',     NULL,                2023, N'Ingeniería',     1, 1, NULL,    NULL,          NULL,         N'Universidad Hispanoamericana',    N'Licenciatura', N'Lic. Marco Ruiz'),
    (N'Tesis',   '9784444555562', N'Uso de aprendizaje automático en el agro',    N'Esteban Mora Picado',     NULL,                2024, N'Computación',    1, 1, NULL,    NULL,          NULL,         N'Universidad Hispanoamericana',    N'Maestría',     N'Dra. Sofía León'),
    (N'Tesis',   '9785555666673', N'Seguridad en aplicaciones web bancarias',     N'Andrés Quirós Lobo',      NULL,                2022, N'Seguridad',      2, 2, NULL,    NULL,          NULL,         N'Universidad de Costa Rica',       N'Licenciatura', N'Ing. Pablo Arce')
) AS origen (Tipo, Isbn, Titulo, Autor, Editorial, AnioPublicacion, Categoria, StockTotal, StockDisponible,
             Edicion, NumeroVolumen, Periodicidad, Universidad, GradoAcademico, Tutor)
ON destino.Isbn = origen.Isbn
WHEN NOT MATCHED THEN
    INSERT (Tipo, Isbn, Titulo, Autor, Editorial, AnioPublicacion, Categoria, StockTotal, StockDisponible,
            Edicion, NumeroVolumen, Periodicidad, Universidad, GradoAcademico, Tutor)
    VALUES (origen.Tipo, origen.Isbn, origen.Titulo, origen.Autor, origen.Editorial, origen.AnioPublicacion,
            origen.Categoria, origen.StockTotal, origen.StockDisponible, origen.Edicion, origen.NumeroVolumen,
            origen.Periodicidad, origen.Universidad, origen.GradoAcademico, origen.Tutor);

/* ---------- Préstamos ----------
   Coinciden con StockDisponible de arriba (Total - Disponible = préstamos activos).
   Solo se insertan si la tabla está vacía, para no duplicarlos. */
IF NOT EXISTS (SELECT 1 FROM dbo.Prestamos)
BEGIN
    DECLARE @hoy DATE = CAST(SYSDATETIME() AS DATE);
    DECLARE @registra INT = (SELECT Id FROM dbo.Usuarios WHERE Carne = N'ADM-001');

    INSERT dbo.Prestamos (UsuarioId, MaterialId, FechaPrestamo, FechaVencimiento, FechaDevolucion, Renovado, RegistradoPorId)
    SELECT u.Id, m.Id, p.FechaPrestamo, p.FechaVencimiento, p.FechaDevolucion, p.Renovado, @registra
    FROM (VALUES
        -- Activos al día
        (N'EST-1001', '9780132350884', DATEADD(DAY,  -2, @hoy), DATEADD(DAY,   5, @hoy), CAST(NULL AS DATETIME2(0)), 0),
        (N'PRF-001',  '9780201633610', DATEADD(DAY,  -3, @hoy), DATEADD(DAY,  11, @hoy), NULL, 0),
        -- Vencen hoy (reporte "vencidos hoy")
        (N'EST-1002', '9788498745672', DATEADD(DAY,  -7, @hoy), @hoy,                    NULL, 0),
        (N'EST-1003', '9786073212342', DATEADD(DAY,  -7, @hoy), @hoy,                    NULL, 0),
        -- Atrasados (generarán multa al devolverse)
        (N'EST-1004', '9789876543217', DATEADD(DAY, -20, @hoy), DATEADD(DAY, -13, @hoy), NULL, 0),
        (N'EST-1005', '9780306406157', DATEADD(DAY, -60, @hoy), DATEADD(DAY, -46, @hoy), NULL, 1),
        -- Historial devuelto (para el Top 10)
        (N'EST-1001', '9789580001119', DATEADD(DAY, -40, @hoy), DATEADD(DAY, -33, @hoy), DATEADD(DAY, -34, @hoy), 0),
        (N'EST-1002', '9789580001119', DATEADD(DAY, -30, @hoy), DATEADD(DAY, -23, @hoy), DATEADD(DAY, -25, @hoy), 0),
        (N'EST-1003', '9780132350884', DATEADD(DAY, -50, @hoy), DATEADD(DAY, -43, @hoy), DATEADD(DAY, -44, @hoy), 0),
        (N'EST-1002', '9780132350884', DATEADD(DAY, -25, @hoy), DATEADD(DAY, -18, @hoy), DATEADD(DAY, -10, @hoy), 0)
    ) AS p (Carne, Isbn, FechaPrestamo, FechaVencimiento, FechaDevolucion, Renovado)
    JOIN dbo.Usuarios   AS u ON u.Carne = p.Carne
    JOIN dbo.Materiales AS m ON m.Isbn  = p.Isbn;
END

/* ---------- Reservas ----------
   Una reserva pendiente sobre "Ingeniería de software": el préstamo de EST-1003 sobre ese
   material NO se podrá renovar (sirve para probar la regla). */
IF NOT EXISTS (SELECT 1 FROM dbo.Reservas)
    INSERT dbo.Reservas (UsuarioId, MaterialId)
    SELECT u.Id, m.Id
    FROM dbo.Usuarios u CROSS JOIN dbo.Materiales m
    WHERE u.Carne = N'EST-1001' AND m.Isbn = '9786073212342';

/* Las multas de prueba se agregan cuando exista la tabla Multas (003).
   El préstamo devuelto 8 días tarde (EST-1002, Clean Code) debería tener una multa
   de 8 × ₡50 = ₡400. */
GO
