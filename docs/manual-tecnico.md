# Manual técnico

## Tecnologías

C# / .NET 10, WPF con MVVM (CommunityToolkit.Mvvm), SQL Server 2022, ADO.NET con
Microsoft.Data.SqlClient (sin ORM), BCrypt.Net, QuestPDF, EPPlus, LiveCharts2, xUnit, StyleCop.

## Arquitectura (Clean Architecture)

```
SGBU.UI  ──►  SGBU.Services  ──►  SGBU.Domain  ◄──  SGBU.Data
 (WPF)        (casos de uso)      (reglas)          (ADO.NET)
```

| Proyecto | Qué contiene |
|---|---|
| `SGBU.Domain` | Entidades (Material → Libro/Revista/Tesis, Usuario, Prestamo, Multa...), reglas (ISBN, Regex, multa), excepciones propias e interfaces de repositorio. No depende de nadie. |
| `SGBU.Data` | Un repositorio por tabla con SqlCommand y SqlDataReader. `UnidadDeTrabajoSql`: una conexión y una transacción para todos los repositorios. Llama a los stored procedures. |
| `SGBU.Services` | Casos de uso: autenticación, catálogo, usuarios, préstamos, multas, reportes, auditoría y preferencias. Revisan permisos por rol. |
| `SGBU.UI` | Vistas XAML y ViewModels. `App.xaml.cs` crea los servicios y se los pasa a las ventanas. |

`tests/SGBU.Tests/ArquitecturaTests.cs` falla si una capa usa a otra que no debe.

## Decisiones importantes

- **Una sola tabla `Materiales`** con columna `Tipo`; en C# hay herencia. El repositorio lee
  `Tipo` y crea un `Libro`, una `Revista` o una `Tesis`.
- **Consultas parametrizadas** siempre (`cmd.Parameters.Add`), nunca texto concatenado: evita la
  inyección SQL.
- **Stored procedures** para prestar, devolver, renovar y pagar multa, porque cambian varias
  tablas a la vez. `sp_RegistrarPrestamo` resta stock con
  `UPDATE ... WHERE StockDisponible > 0`: si dos personas piden el último ejemplar a la vez, solo
  una lo obtiene.
- **Unit of Work:** si no se llama a `ConfirmarAsync`, al cerrar se hace rollback.
- **Contraseñas** con BCrypt (factor 11). El login da el mismo mensaje si el usuario no existe o
  la clave es incorrecta.
- **Paginación en el servidor** con `OFFSET ... FETCH`.
- **Errores:** SQL Server avisa con `THROW 50001...`; el repositorio lo convierte en una
  excepción del dominio y la pantalla muestra el mensaje.
- **Tema e idioma:** `Temas/*.xaml` (colores) e `Idiomas/*.xaml` (textos). Las vistas usan
  `{StaticResource clave}`. La preferencia se guarda en `%AppData%\SGBU\configuracion.json`.

## Base de datos

Ver [diccionario de datos](diccionario-datos.md). Scripts numerados en `database/`; se pueden
correr varias veces sin error. En Windows: `.\database\ejecutar.ps1`.

## Configuración de la conexión

`src/SGBU.UI/appsettings.json` trae LocalDB. Para otro servidor, crear
`appsettings.Local.json` (no se sube a GitHub) con la cadena `ConnectionStrings:Sgbu`.

## Pruebas

| Proyecto | Qué prueba | Cuántas |
|---|---|---|
| `tests/SGBU.Tests` | Domain y Services, con repositorios falsos en memoria | 158 |
| `tests/SGBU.Data.Tests` | Repositorios y stored procedures contra SQL Server (cada prueba en una transacción que se revierte) | 37 |
| `database/pruebas/verificar_restricciones.sql` | Restricciones de las tablas y procedimientos, en SQL | 27 |
| `tools/SGBU.Capturas` | Abre la aplicación con FlaUI, inicia sesión y recorre las pantallas (prueba E2E) | 18 capturas |

## Integración continua (GitHub Actions)

- `ci.yml`: en cada Pull Request compila en Windows, crea la base en LocalDB y corre todas las pruebas.
- `capturas.yml`: publica la aplicación, toma las capturas de pantalla y arma el instalador MSI.
