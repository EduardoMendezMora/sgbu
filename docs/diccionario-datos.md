# Diccionario de datos

Base de datos `SGBU` en SQL Server 2022. Intercalación `Modern_Spanish_CI_AS` (acepta tildes y ñ, y no distingue mayúsculas).

Scripts en `database/`: `002_tablas.sql`, `003_tabla_multas.sql`, `004_indices.sql`, `006_procedimientos.sql`.

## Relaciones

```
Roles 1 ──< Usuarios 1 ──< Prestamos >── 1 Materiales
                  │            │
                  │            └──1 Multas (una por préstamo atrasado)
                  └──< Reservas >── Materiales
Auditoria: guarda UsuarioId sin llave foránea
```

## Roles

Los tres roles del sistema y sus límites de préstamo.

| Columna | Tipo | ¿Nulo? | Descripción |
|---|---|---|---|
| Id | int | No | Identificador del rol. |
| Nombre | nvarchar(30) | No | Administrador, Profesor o Estudiante. |
| LimitePrestamos | int | No | Cuántos préstamos activos puede tener al mismo tiempo. |
| DiasPrestamo | int | No | Días que dura un préstamo (y una renovación). |

Restricciones: `CK_Roles_DiasPrestamo` (Regla (CHECK)), `CK_Roles_LimitePrestamos` (Regla (CHECK)), `CK_Roles_Nombre` (Regla (CHECK)), `PK_Roles` (Llave primaria), `UQ_Roles_Nombre` (Único)

## Usuarios

Personas que usan el sistema.

| Columna | Tipo | ¿Nulo? | Descripción |
|---|---|---|---|
| Id | int | No | Identificador del usuario. |
| Carne | nvarchar(20) | No | Carné universitario. Único. |
| Dni | nvarchar(20) | No | Cédula (9 dígitos) o DIMEX (11 o 12). Único. |
| Nombre | nvarchar(60) | No | Nombre. |
| Apellidos | nvarchar(80) | No | Apellidos. |
| Email | nvarchar(120) | No | Correo; también sirve para iniciar sesión. Único. |
| Telefono | nvarchar(20) | Sí | Teléfono de Costa Rica (opcional). |
| RolId | int | No | Rol del usuario (FK a Roles). |
| PasswordHash | char(60) | No | Hash BCrypt de la contraseña. La contraseña nunca se guarda. |
| Activo | bit | No | 1 = puede usar el sistema; 0 = desactivado. |
| FechaRegistro | datetime2 | No | Cuándo se creó el usuario. |

Restricciones: `FK_Usuarios_Roles` (Llave foránea), `PK_Usuarios` (Llave primaria), `UQ_Usuarios_Carne` (Único), `UQ_Usuarios_Dni` (Único), `UQ_Usuarios_Email` (Único)

## Materiales

Libros, revistas y tesis en una sola tabla (columna Tipo).

| Columna | Tipo | ¿Nulo? | Descripción |
|---|---|---|---|
| Id | int | No | Identificador del material. |
| Tipo | nvarchar(10) | No | Libro, Revista o Tesis. |
| Isbn | char(13) | No | ISBN-13 sin guiones. Único. El dígito verificador se valida en Domain. |
| Titulo | nvarchar(200) | No | Título. |
| Autor | nvarchar(150) | No | Autor o autores. |
| Editorial | nvarchar(100) | Sí | Editorial (opcional). |
| AnioPublicacion | smallint | Sí | Año de publicación, entre 1450 y 2100. |
| Categoria | nvarchar(60) | Sí | Área del conocimiento (opcional). |
| StockTotal | int | No | Ejemplares que tiene la biblioteca. |
| StockDisponible | int | No | Ejemplares que no están prestados. Entre 0 y StockTotal. |
| Activo | bit | No | 1 = aparece en el catálogo. |
| FechaRegistro | datetime2 | No | Cuándo se registró. |
| Edicion | nvarchar(30) | Sí | Solo libros. |
| NumeroVolumen | nvarchar(30) | Sí | Solo revistas. |
| Periodicidad | nvarchar(30) | Sí | Solo revistas (mensual, bimestral...). |
| Universidad | nvarchar(120) | Sí | Solo tesis; obligatoria para tesis. |
| GradoAcademico | nvarchar(40) | Sí | Solo tesis. |
| Tutor | nvarchar(120) | Sí | Solo tesis. |

Restricciones: `CK_Materiales_Anio` (Regla (CHECK)), `CK_Materiales_CamposPorTipo` (Regla (CHECK)), `CK_Materiales_IsbnDigitos` (Regla (CHECK)), `CK_Materiales_Stock` (Regla (CHECK)), `CK_Materiales_Tipo` (Regla (CHECK)), `PK_Materiales` (Llave primaria), `UQ_Materiales_Isbn` (Único)

Índices: `IX_Materiales_Autor`, `IX_Materiales_StockDisponible`, `IX_Materiales_Tipo_Categoria`, `IX_Materiales_Titulo`

## Prestamos

Un ejemplar prestado a un usuario.

| Columna | Tipo | ¿Nulo? | Descripción |
|---|---|---|---|
| Id | int | No | Identificador del préstamo. |
| UsuarioId | int | No | Quién se lo llevó (FK a Usuarios). |
| MaterialId | int | No | Qué material (FK a Materiales). |
| FechaPrestamo | datetime2 | No | Cuándo se prestó. |
| FechaVencimiento | date | No | Fecha límite de devolución. |
| FechaDevolucion | datetime2 | Sí | Cuándo se devolvió. NULL = todavía prestado. |
| Renovado | bit | No | 1 = ya se renovó (solo se permite una vez). |
| RegistradoPorId | int | Sí | Bibliotecario que lo registró (FK a Usuarios). |

Restricciones: `CK_Prestamos_Devolucion` (Regla (CHECK)), `CK_Prestamos_Fechas` (Regla (CHECK)), `FK_Prestamos_Materiales` (Llave foránea), `FK_Prestamos_RegistradoPor` (Llave foránea), `FK_Prestamos_Usuarios` (Llave foránea), `PK_Prestamos` (Llave primaria)

Índices: `IX_Prestamos_Material`, `IX_Prestamos_Usuario_Activos`, `IX_Prestamos_Vencimiento_Activos`

## Multas

Multa por devolver tarde: ₡50 por día, máximo ₡2000.

| Columna | Tipo | ¿Nulo? | Descripción |
|---|---|---|---|
| Id | int | No | Identificador de la multa. |
| PrestamoId | int | No | Préstamo que la generó (FK). Único: una multa por préstamo. |
| UsuarioId | int | No | Usuario que debe pagarla (FK). Está repetido a propósito para consultar rápido si un usuario debe. |
| DiasAtraso | int | No | Días de atraso. Mayor que 0. |
| Monto | decimal(10,2) | No | Monto en colones, entre 0 y 2000. |
| Estado | nvarchar(10) | No | Pendiente o Pagada. |
| FechaGeneracion | datetime2 | No | Cuándo se generó (al devolver). |
| FechaPago | datetime2 | Sí | Cuándo se pagó. NULL si está pendiente. |

Restricciones: `CK_Multas_DiasAtraso` (Regla (CHECK)), `CK_Multas_Estado` (Regla (CHECK)), `CK_Multas_Monto` (Regla (CHECK)), `CK_Multas_Pago` (Regla (CHECK)), `FK_Multas_Prestamos` (Llave foránea), `FK_Multas_Usuarios` (Llave foránea), `PK_Multas` (Llave primaria), `UQ_Multas_Prestamo` (Único)

Índices: `IX_Multas_Usuario_Pendientes`

## Reservas

Un usuario aparta un material. Si alguien lo reservó, el préstamo no se puede renovar.

| Columna | Tipo | ¿Nulo? | Descripción |
|---|---|---|---|
| Id | int | No | Identificador de la reserva. |
| UsuarioId | int | No | Quién reservó (FK). |
| MaterialId | int | No | Qué material (FK). |
| FechaReserva | datetime2 | No | Cuándo se reservó. |
| Estado | nvarchar(12) | No | Pendiente, Atendida o Cancelada. |

Restricciones: `CK_Reservas_Estado` (Regla (CHECK)), `FK_Reservas_Materiales` (Llave foránea), `FK_Reservas_Usuarios` (Llave foránea), `PK_Reservas` (Llave primaria)

Índices: `IX_Reservas_Material_Pendientes`

## Auditoria

Bitácora: quién hizo qué y cuándo. No tiene FK para no perder registros si se borra un usuario.

| Columna | Tipo | ¿Nulo? | Descripción |
|---|---|---|---|
| Id | bigint | No | Identificador del registro. |
| UsuarioId | int | Sí | Quién hizo la acción. NULL = sistema o login con un correo que no existe. |
| Accion | nvarchar(50) | No | LOGIN, LOGIN_FALLIDO, PRESTAMO, DEVOLUCION, RENOVACION, PAGO_MULTA, MATERIAL_CREADO, etc. |
| Entidad | nvarchar(50) | Sí | Tabla afectada. |
| EntidadId | int | Sí | Id del registro afectado. |
| Detalle | nvarchar(500) | Sí | Información extra (por ejemplo, el monto de la multa). |
| Fecha | datetime2 | No | Cuándo ocurrió. |

Restricciones: `PK_Auditoria` (Llave primaria)

Índices: `IX_Auditoria_Fecha`, `IX_Auditoria_Usuario`

## Stored procedures

| Procedimiento | Qué hace |
|---|---|
| `sp_RegistrarPrestamo` | Revisa usuario activo, multas y límite del rol; resta 1 al stock solo si hay; crea el préstamo y lo audita. |
| `sp_RegistrarDevolucion` | Marca la devolución, suma 1 al stock y, si hay atraso, crea la multa. Devuelve días y monto. |
| `sp_RenovarPrestamo` | Una sola vez, no vencido y sin reserva pendiente. Suma los días del rol al vencimiento. |
| `sp_PagarMulta` | Marca la multa como pagada con su fecha y lo audita. |
