# Changelog

Formato basado en [Keep a Changelog](https://keepachangelog.com/es-ES/1.1.0/).

## [1.0.0] - 2026-11-21

### Agregado
- Base de datos: tablas Roles, Usuarios, Materiales (libro, revista y tesis en una tabla),
  Prestamos, Multas, Reservas y Auditoria, con restricciones, índices y datos de prueba.
- Domain: entidades con herencia, validación de ISBN-13, email, DNI y teléfono con Regex,
  excepciones propias e interfaces de repositorio.
- Data: repositorios con ADO.NET y Unit of Work con transacciones.
- Login con BCrypt y permisos por rol (Administrador, Profesor, Estudiante).
- Catálogo y Usuarios: búsqueda, paginación en el servidor y formularios con validación.
- Préstamos, devoluciones y renovaciones con stored procedures.
- Multas automáticas (₡50 por día, máximo ₡2000) y registro de pagos.
- Reportes: Top 10, usuarios con multas, stock crítico y vencidos hoy; exportación a PDF y
  Excel; gráfico de barras; respaldo en XML.
- Configuración de tema (claro/oscuro) e idioma (español/inglés) y pantalla de auditoría.
- Instalador MSI.
- Pruebas unitarias, de integración, en SQL y capturas E2E con FlaUI; CI en GitHub Actions.
