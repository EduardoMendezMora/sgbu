/*
    003_tabla_multas.sql
    >>> ESTA TABLA LA ESCRIBE EDUARDO (guía abajo). <<<

    Regla de negocio:
    - Multa de ₡50 por día de atraso, con tope de ₡2000.
    - Mientras un usuario tenga multas pendientes, no puede pedir préstamos.
    - Una multa nace de un préstamo devuelto tarde.

    Preguntas para pensar antes de escribir (las va a hacer el profe):
    1. ¿A qué tabla apunta la multa: a Prestamos, a Usuarios o a las dos? ¿Por qué?
       Pista: con el préstamo se sabe el usuario, pero el bloqueo de préstamos pregunta
       "¿este usuario tiene multas pendientes?" muchas veces al día.
    2. ¿Puede un mismo préstamo tener dos multas? ¿Qué restricción lo impide?
    3. ¿Qué tipo de dato usar para el monto? (Pista: nunca FLOAT para dinero.)
    4. ¿Qué CHECK garantiza el tope de ₡2000 y que el monto no sea negativo?
    5. ¿Cómo se sabe si está pendiente o pagada, y cuándo se pagó?

    Columnas sugeridas: Id, PrestamoId, UsuarioId, DiasAtraso, Monto, Estado,
    FechaGeneracion, FechaPago.

    Modelo a seguir: la tabla Reservas en 002_tablas.sql (tiene IDENTITY, DEFAULT,
    FOREIGN KEY y un CHECK de Estado).

    Cuando la tengas, también falta su índice en 004_indices.sql (está marcado ahí).
    Para probar: bash database/ejecutar.sh
*/
USE SGBU;
GO

-- Escribe aquí: IF OBJECT_ID(N'dbo.Multas', N'U') IS NULL CREATE TABLE dbo.Multas ( ... );

GO
