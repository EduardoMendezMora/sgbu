# SGBU: guía para Claude Code

Sistema de Gestión de Biblioteca Universitaria. Proyecto Programado (40% de la nota) del curso
Programación IV, Universidad Hispanoamericana, docente Lic. Jonathan Salgado Vega. Grupo 3.
Entrega final: semana 13 del curso (el trabajo empezó en la semana 10).

## Cómo trabajar con Eduardo

- Responder siempre en español.
- El objetivo es que **aprenda**, no solo que el proyecto quede hecho: el docente elige qué
  integrante expone. Explicar el porqué de cada decisión antes o junto con el código.
- Modalidad mixta: la primera pieza de cada tipo (primer repositorio, primer ViewModel, primer
  stored procedure) la escribe Eduardo con guía; las repetitivas las escribe Claude y explica
  qué cambia.
- Eduardo suele mandar capturas de pantalla en lugar de describir los errores.

## Tecnologías obligatorias (del enunciado)

C# / .NET 10, WPF con MVVM estricto, SQL Server 2022, ADO.NET directo con
Microsoft.Data.SqlClient (**sin ORM, nada de Entity Framework ni Dapper**), XML, xUnit,
BCrypt.Net, QuestPDF, EPPlus, LiveCharts2, CommunityToolkit.Mvvm, StyleCop.

## Arquitectura

Clean Architecture con dependencias que solo apuntan hacia Domain:

| Proyecto | Responsabilidad | Puede referenciar |
|---|---|---|
| `src/SGBU.Domain` | Entidades, reglas, excepciones e interfaces de repositorio | nadie |
| `src/SGBU.Data` | Repositorios ADO.NET, Unit of Work, transacciones | Domain |
| `src/SGBU.Services` | Casos de uso | Domain |
| `src/SGBU.UI` | WPF: Views y ViewModels; arma la inyección de dependencias | todos |
| `tests/SGBU.Tests` | Pruebas xUnit | Domain, Services |

`tests/SGBU.Tests/ArquitecturaTests.cs` falla si se rompen estas reglas. Sin lógica de negocio
en el code-behind de las vistas.

## Reglas de negocio clave

- Roles: Administrador, Profesor y Estudiante. Límite de préstamos concurrentes según rol.
- Materiales: Libro, Revista y Tesis por herencia. ISBN-13 válido, stock ≥ 0.
- Préstamo: por ISBN y carné; exige stock disponible y cero multas pendientes.
- Renovación: una sola vez, si el material no está reservado.
- Multa: ₡50 por día de atraso, con tope de ₡2000. Multas pendientes bloquean préstamos.
- Validar email, DNI y teléfono con Regex.
- Reportes: Top 10 más prestados, usuarios con multas, stock crítico (≤ 3), vencidos hoy.
- Auditoría: quién, qué acción y cuándo.

## Persistencia

Consultas siempre parametrizadas (`SqlParameter`), `SqlDataReader` para lecturas,
`SqlTransaction` y stored procedures para operaciones críticas, paginación del lado del
servidor, índices en tablas principales. Scripts numerados en `database/`.
Nunca subir cadenas de conexión reales al repositorio.

## Git

- Ramas: `main` (entregable), `develop` (integración), `feature/<nombre>` por tarea.
- Todo entra a `develop` por Pull Request; GitHub Actions compila y prueba en Windows.
- Commits convencionales, en español: `feat:`, `fix:`, `docs:`, `test:`, `refactor:`, `chore:`.
- No hacer merge de un Pull Request sin que Eduardo lo revise.

## Entorno de trabajo

Eduardo usa Claude Code en un **servidor Ubuntu**, no en Windows. Consecuencias:

- Domain, Data, Services y las pruebas se compilan y se ejecutan en el servidor.
- WPF **no se puede ejecutar** en Linux. La UI se verifica de dos formas: GitHub Actions la
  compila en Windows en cada Pull Request, y un integrante la ejecuta en una PC con Windows.
- Para compilar la solución completa en Linux hay que probar `-p:EnableWindowsTargeting=true`;
  si no funciona, compilar y probar solo `tests/SGBU.Tests`.
- SQL Server 2022 corre en Ubuntu (Docker, imagen `mcr.microsoft.com/mssql/server:2022-latest`,
  necesita al menos 2 GB de RAM). Sirve para probar scripts y para las pruebas de integración.
- El instalador MSI o ClickOnce y la demostración final requieren Windows.

## Comandos

```bash
dotnet test tests/SGBU.Tests          # funciona en Ubuntu
dotnet build SGBU.slnx                # solución completa (Windows o CI)
dotnet run --project src/SGBU.UI      # solo en Windows
```

## Plan y avance

- [x] 1. Entorno, repositorio, solución con las cuatro capas y CI
- [ ] 2. Base de datos: tablas, índices y seed. Decisión tomada: una sola tabla `Materiales`
      con columna `Tipo`. Eduardo escribe la tabla `Multas` con guía.
- [ ] 3. Domain: entidades con herencia, excepciones propias, interfaces de repositorio
- [ ] 4. Data: repositorios ADO.NET, Unit of Work, transacciones
- [ ] 5. Login de punta a punta: BCrypt, roles, primera pantalla MVVM
- [ ] 6. Catálogo y Usuarios: CRUD, búsquedas, validaciones, paginación
- [ ] 7. Préstamos y devoluciones con stored procedures
- [ ] 8. Multas y pagos
- [ ] 9. Reportes (PDF, Excel, gráficos) y respaldo XML
- [ ] 10. Configuración (tema, idioma ES/EN) y auditoría
- [ ] 11. Instalador (MSI o ClickOnce), manuales, diccionario de datos, CHANGELOG
- [ ] 12. Exposición de 20 minutos

Las pruebas xUnit se escriben junto con cada paso desde el 3 (meta: más de 80% de cobertura).
Actualizar esta lista al terminar cada paso.

## Pendientes con el docente

- El enunciado pide pruebas E2E con Playwright, que es para web; para WPF lo habitual es FlaUI.
  Falta preguntarle cuál acepta.
- En clase mencionó "modelo vista controlador", pero el enunciado y la rúbrica dicen MVVM.
  Se sigue con MVVM hasta que confirme lo contrario por escrito.
