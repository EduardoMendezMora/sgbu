/*
    001_crear_base.sql
    Crea la base de datos SGBU si todavía no existe.

    Por qué "IF DB_ID(...) IS NULL": así el script se puede correr varias veces sin error
    (es idempotente). Todos los scripts de esta carpeta siguen la misma idea.
*/
IF DB_ID(N'SGBU') IS NULL
BEGIN
    CREATE DATABASE SGBU COLLATE Modern_Spanish_CI_AS;
END
GO
