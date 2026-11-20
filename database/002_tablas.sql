/*
    002_tablas.sql
    Tablas principales del sistema (todas menos Multas, que va en 003).

    Decisiones de diseño:
    - Las claves primarias son INT IDENTITY: números que SQL Server genera solo.
    - Los textos son NVARCHAR para aceptar tildes y la ñ.
    - Las reglas que la base puede garantizar sola van como restricciones (CHECK, UNIQUE,
      FOREIGN KEY). Así, aunque la aplicación tenga un error, la base nunca guarda datos
      inválidos. Las reglas que necesitan lógica (el dígito verificador del ISBN, el tope de
      préstamos por rol) se validan en Domain y en los stored procedures.
    - Las restricciones llevan nombre (CK_..., UQ_..., FK_...) para que los mensajes de
      error se entiendan y se puedan capturar desde C#.
*/
USE SGBU;
GO

/* ---------- Roles ----------
   Tabla aparte (y no un texto dentro de Usuarios) porque cada rol tiene sus propios
   límites. Si mañana cambia el límite de Estudiante, se cambia en una sola fila. */
IF OBJECT_ID(N'dbo.Roles', N'U') IS NULL
CREATE TABLE dbo.Roles
(
    Id                  INT           IDENTITY(1, 1) NOT NULL,
    Nombre              NVARCHAR(30)  NOT NULL,
    LimitePrestamos     INT           NOT NULL,   -- préstamos activos al mismo tiempo
    DiasPrestamo        INT           NOT NULL,   -- plazo de devolución

    CONSTRAINT PK_Roles PRIMARY KEY (Id),
    CONSTRAINT UQ_Roles_Nombre UNIQUE (Nombre),
    CONSTRAINT CK_Roles_Nombre CHECK (Nombre IN (N'Administrador', N'Profesor', N'Estudiante')),
    CONSTRAINT CK_Roles_LimitePrestamos CHECK (LimitePrestamos > 0),
    CONSTRAINT CK_Roles_DiasPrestamo CHECK (DiasPrestamo > 0)
);
GO

/* ---------- Usuarios ----------
   La contraseña NUNCA se guarda: se guarda su hash BCrypt (60 caracteres).
   Email, DNI y teléfono se validan con Regex en la aplicación; aquí solo se exige
   que el carné, el DNI y el email no se repitan. */
IF OBJECT_ID(N'dbo.Usuarios', N'U') IS NULL
CREATE TABLE dbo.Usuarios
(
    Id                  INT            IDENTITY(1, 1) NOT NULL,
    Carne               NVARCHAR(20)   NOT NULL,
    Dni                 NVARCHAR(20)   NOT NULL,
    Nombre              NVARCHAR(60)   NOT NULL,
    Apellidos           NVARCHAR(80)   NOT NULL,
    Email               NVARCHAR(120)  NOT NULL,
    Telefono            NVARCHAR(20)   NULL,
    RolId               INT            NOT NULL,
    PasswordHash        CHAR(60)       NOT NULL,
    Activo              BIT            NOT NULL CONSTRAINT DF_Usuarios_Activo DEFAULT (1),
    FechaRegistro       DATETIME2(0)   NOT NULL CONSTRAINT DF_Usuarios_FechaRegistro DEFAULT (SYSDATETIME()),

    CONSTRAINT PK_Usuarios PRIMARY KEY (Id),
    CONSTRAINT UQ_Usuarios_Carne UNIQUE (Carne),
    CONSTRAINT UQ_Usuarios_Dni UNIQUE (Dni),
    CONSTRAINT UQ_Usuarios_Email UNIQUE (Email),
    CONSTRAINT FK_Usuarios_Roles FOREIGN KEY (RolId) REFERENCES dbo.Roles (Id)
);
GO

/* ---------- Materiales ----------
   Decisión del grupo: UNA sola tabla para Libro, Revista y Tesis, con la columna Tipo
   (patrón "tabla por jerarquía"). En C# sí hay herencia: Material es la clase base y
   Libro, Revista y Tesis heredan de ella. El repositorio lee Tipo y crea el objeto
   correcto.
   Ventaja: buscar, prestar y reportar se hace contra una sola tabla, sin JOIN.
   Costo: las columnas propias de cada tipo quedan en NULL para los otros tipos; los
   CHECK de abajo evitan que se mezclen (una Revista no puede tener Universidad).

   El préstamo es por ISBN, por eso el ISBN es obligatorio y único para todo material.
   La base solo revisa que sean 13 dígitos; el dígito verificador se valida en Domain. */
IF OBJECT_ID(N'dbo.Materiales', N'U') IS NULL
CREATE TABLE dbo.Materiales
(
    Id                  INT            IDENTITY(1, 1) NOT NULL,
    Tipo                NVARCHAR(10)   NOT NULL,
    Isbn                CHAR(13)       NOT NULL,
    Titulo              NVARCHAR(200)  NOT NULL,
    Autor               NVARCHAR(150)  NOT NULL,
    Editorial           NVARCHAR(100)  NULL,
    AnioPublicacion     SMALLINT       NULL,
    Categoria           NVARCHAR(60)   NULL,
    StockTotal          INT            NOT NULL,
    StockDisponible     INT            NOT NULL,
    Activo              BIT            NOT NULL CONSTRAINT DF_Materiales_Activo DEFAULT (1),
    FechaRegistro       DATETIME2(0)   NOT NULL CONSTRAINT DF_Materiales_FechaRegistro DEFAULT (SYSDATETIME()),

    -- Solo Libro
    Edicion             NVARCHAR(30)   NULL,
    -- Solo Revista
    NumeroVolumen       NVARCHAR(30)   NULL,
    Periodicidad        NVARCHAR(30)   NULL,
    -- Solo Tesis
    Universidad         NVARCHAR(120)  NULL,
    GradoAcademico      NVARCHAR(40)   NULL,
    Tutor               NVARCHAR(120)  NULL,

    CONSTRAINT PK_Materiales PRIMARY KEY (Id),
    CONSTRAINT UQ_Materiales_Isbn UNIQUE (Isbn),
    CONSTRAINT CK_Materiales_Tipo CHECK (Tipo IN (N'Libro', N'Revista', N'Tesis')),
    CONSTRAINT CK_Materiales_IsbnDigitos CHECK (Isbn NOT LIKE '%[^0-9]%'),
    CONSTRAINT CK_Materiales_Stock CHECK (StockTotal >= 0 AND StockDisponible >= 0 AND StockDisponible <= StockTotal),
    CONSTRAINT CK_Materiales_Anio CHECK (AnioPublicacion IS NULL OR AnioPublicacion BETWEEN 1450 AND 2100),
    CONSTRAINT CK_Materiales_CamposPorTipo CHECK (
        (Tipo = N'Libro'   AND NumeroVolumen IS NULL AND Periodicidad IS NULL AND Universidad IS NULL AND GradoAcademico IS NULL AND Tutor IS NULL)
     OR (Tipo = N'Revista' AND Edicion IS NULL AND Universidad IS NULL AND GradoAcademico IS NULL AND Tutor IS NULL)
     OR (Tipo = N'Tesis'   AND Edicion IS NULL AND NumeroVolumen IS NULL AND Periodicidad IS NULL AND Universidad IS NOT NULL)
    )
);
GO

/* ---------- Prestamos ----------
   Un préstamo es UN ejemplar de UN material para UN usuario.
   FechaDevolucion en NULL significa que todavía no se ha devuelto.
   Renovado es BIT porque la regla dice "una sola vez": basta saber si ya se renovó. */
IF OBJECT_ID(N'dbo.Prestamos', N'U') IS NULL
CREATE TABLE dbo.Prestamos
(
    Id                  INT            IDENTITY(1, 1) NOT NULL,
    UsuarioId           INT            NOT NULL,
    MaterialId          INT            NOT NULL,
    FechaPrestamo       DATETIME2(0)   NOT NULL CONSTRAINT DF_Prestamos_FechaPrestamo DEFAULT (SYSDATETIME()),
    FechaVencimiento    DATE           NOT NULL,
    FechaDevolucion     DATETIME2(0)   NULL,
    Renovado            BIT            NOT NULL CONSTRAINT DF_Prestamos_Renovado DEFAULT (0),
    RegistradoPorId     INT            NULL,      -- el bibliotecario/administrador que lo registró

    CONSTRAINT PK_Prestamos PRIMARY KEY (Id),
    CONSTRAINT FK_Prestamos_Usuarios FOREIGN KEY (UsuarioId) REFERENCES dbo.Usuarios (Id),
    CONSTRAINT FK_Prestamos_Materiales FOREIGN KEY (MaterialId) REFERENCES dbo.Materiales (Id),
    CONSTRAINT FK_Prestamos_RegistradoPor FOREIGN KEY (RegistradoPorId) REFERENCES dbo.Usuarios (Id),
    CONSTRAINT CK_Prestamos_Fechas CHECK (FechaVencimiento >= CAST(FechaPrestamo AS DATE)),
    CONSTRAINT CK_Prestamos_Devolucion CHECK (FechaDevolucion IS NULL OR FechaDevolucion >= FechaPrestamo)
);
GO

/* ---------- Reservas ----------
   Se necesita para la regla de renovación: un préstamo no se puede renovar si alguien
   más reservó ese material. */
IF OBJECT_ID(N'dbo.Reservas', N'U') IS NULL
CREATE TABLE dbo.Reservas
(
    Id                  INT            IDENTITY(1, 1) NOT NULL,
    UsuarioId           INT            NOT NULL,
    MaterialId          INT            NOT NULL,
    FechaReserva        DATETIME2(0)   NOT NULL CONSTRAINT DF_Reservas_FechaReserva DEFAULT (SYSDATETIME()),
    Estado              NVARCHAR(12)   NOT NULL CONSTRAINT DF_Reservas_Estado DEFAULT (N'Pendiente'),

    CONSTRAINT PK_Reservas PRIMARY KEY (Id),
    CONSTRAINT FK_Reservas_Usuarios FOREIGN KEY (UsuarioId) REFERENCES dbo.Usuarios (Id),
    CONSTRAINT FK_Reservas_Materiales FOREIGN KEY (MaterialId) REFERENCES dbo.Materiales (Id),
    CONSTRAINT CK_Reservas_Estado CHECK (Estado IN (N'Pendiente', N'Atendida', N'Cancelada'))
);
GO

/* ---------- Auditoria ----------
   "Quién, qué acción y cuándo". UsuarioId admite NULL para acciones del sistema o un
   intento de login con un usuario que no existe. No lleva FOREIGN KEY a propósito:
   la bitácora no se debe perder ni bloquear si después se desactiva un usuario. */
IF OBJECT_ID(N'dbo.Auditoria', N'U') IS NULL
CREATE TABLE dbo.Auditoria
(
    Id                  BIGINT         IDENTITY(1, 1) NOT NULL,
    UsuarioId           INT            NULL,
    Accion              NVARCHAR(50)   NOT NULL,   -- p. ej. LOGIN, PRESTAMO, DEVOLUCION, PAGO_MULTA
    Entidad             NVARCHAR(50)   NULL,       -- tabla afectada
    EntidadId           INT            NULL,
    Detalle             NVARCHAR(500)  NULL,
    Fecha               DATETIME2(0)   NOT NULL CONSTRAINT DF_Auditoria_Fecha DEFAULT (SYSDATETIME()),

    CONSTRAINT PK_Auditoria PRIMARY KEY (Id)
);
GO
