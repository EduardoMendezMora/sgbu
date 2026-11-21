/*
    004_indices.sql
    Índices para las búsquedas que más se repiten.

    Qué es un índice: como el índice de un libro. Sin él, SQL Server lee la tabla entera
    para encontrar una fila. Con él, va directo.
    Costo: cada INSERT/UPDATE tiene que actualizar también el índice. Por eso solo se crean
    para consultas que de verdad se usan, no en todas las columnas.

    Ya existen índices "gratis": cada PRIMARY KEY y cada UNIQUE crea uno. Por eso no hace
    falta crear índices para Usuarios.Carne, Usuarios.Email ni Materiales.Isbn.
*/
USE SGBU;
GO

-- Catálogo: búsqueda por título y filtro por tipo/categoría (con paginación).
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Materiales_Titulo')
    CREATE INDEX IX_Materiales_Titulo ON dbo.Materiales (Titulo);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Materiales_Autor')
    CREATE INDEX IX_Materiales_Autor ON dbo.Materiales (Autor);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Materiales_Tipo_Categoria')
    CREATE INDEX IX_Materiales_Tipo_Categoria ON dbo.Materiales (Tipo, Categoria);

-- Reporte de stock crítico (≤ 3).
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Materiales_StockDisponible')
    CREATE INDEX IX_Materiales_StockDisponible ON dbo.Materiales (StockDisponible) INCLUDE (Titulo, Isbn);

-- Préstamos activos de un usuario (límite por rol). Índice FILTRADO: solo guarda los
-- préstamos sin devolver, que son pocos comparados con el historial completo.
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Prestamos_Usuario_Activos')
    CREATE INDEX IX_Prestamos_Usuario_Activos ON dbo.Prestamos (UsuarioId) WHERE FechaDevolucion IS NULL;

-- Reporte "vencidos hoy" y cálculo de atrasos.
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Prestamos_Vencimiento_Activos')
    CREATE INDEX IX_Prestamos_Vencimiento_Activos ON dbo.Prestamos (FechaVencimiento) INCLUDE (UsuarioId, MaterialId) WHERE FechaDevolucion IS NULL;

-- Top 10 más prestados.
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Prestamos_Material')
    CREATE INDEX IX_Prestamos_Material ON dbo.Prestamos (MaterialId);

-- Renovación: ¿hay reservas pendientes de este material?
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Reservas_Material_Pendientes')
    CREATE INDEX IX_Reservas_Material_Pendientes ON dbo.Reservas (MaterialId) WHERE Estado = N'Pendiente';

-- Bitácora: consultar por fecha y por usuario.
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Auditoria_Fecha')
    CREATE INDEX IX_Auditoria_Fecha ON dbo.Auditoria (Fecha DESC);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Auditoria_Usuario')
    CREATE INDEX IX_Auditoria_Usuario ON dbo.Auditoria (UsuarioId, Fecha DESC);
GO

-- "¿Este usuario tiene multas pendientes?" se pregunta en cada préstamo. Índice filtrado:
-- solo guarda las pendientes, que son pocas; las pagadas no ocupan espacio en él.
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Multas_Usuario_Pendientes')
    CREATE INDEX IX_Multas_Usuario_Pendientes ON dbo.Multas (UsuarioId)
        INCLUDE (Monto) WHERE Estado = N'Pendiente';
GO
