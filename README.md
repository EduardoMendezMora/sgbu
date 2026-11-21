# SGBU

Sistema de Gestión de Biblioteca Universitaria. Proyecto Programado de Programación IV, Grupo 3.

## Requisitos

- Windows 10 u 11 (la interfaz es WPF: no corre en Linux ni en Mac)
- Visual Studio 2026 con la carga "Desarrollo de escritorio de .NET" (incluye .NET 10) y, en
  "Componentes individuales", **SQL Server Express LocalDB**
- Opcional: SQL Server 2022 Developer y SSMS. Si no, alcanza con LocalDB.

## Cómo probarlo en Windows

En PowerShell:

```powershell
git clone https://github.com/EduardoMendezMora/sgbu.git
cd sgbu
git checkout develop

# 1. Crear la base con los datos de prueba (una sola vez; se puede repetir sin problema)
.\database\ejecutar.ps1                       # con LocalDB
# .\database\ejecutar.ps1 -Servidor localhost # si instaló SQL Server 2022 Developer

# 2. Abrir la aplicación
dotnet run --project src/SGBU.UI
```

O abrir `SGBU.slnx` en Visual Studio, elegir `SGBU.UI` como proyecto de inicio y pulsar F5.

Para entrar: usuario `admin@sgbu.ac.cr` (o el carné `ADM-001`) y contraseña `Sgbu2026!`.
Con `EST-1001` se entra como estudiante y el menú muestra menos opciones. Los demás
usuarios de prueba están en `database/README.md`.

**Si usa SQL Server Developer en vez de LocalDB**, cree el archivo
`src/SGBU.UI/appsettings.Local.json` (no se sube a GitHub) con:

```json
{ "ConnectionStrings": { "Sgbu": "Server=localhost;Database=SGBU;Integrated Security=True;TrustServerCertificate=True" } }
```

### Pruebas

```powershell
dotnet test tests/SGBU.Tests          # unitarias, no necesitan base
$env:SGBU_CONN = "Server=(localdb)\MSSQLLocalDB;Database=SGBU;Integrated Security=True;TrustServerCertificate=True"
dotnet test tests/SGBU.Data.Tests     # integración contra la base (sin SGBU_CONN se omiten)
```

## Estructura

| Carpeta | Contenido |
|---|---|
| `src/SGBU.Domain` | Entidades y reglas del negocio. No depende de nadie. |
| `src/SGBU.Data` | Repositorios ADO.NET contra SQL Server. |
| `src/SGBU.Services` | Casos de uso. Solo conoce Domain. |
| `src/SGBU.UI` | Aplicación WPF con MVVM. |
| `tests/SGBU.Tests` | Pruebas unitarias (Domain y Services). |
| `tests/SGBU.Data.Tests` | Pruebas de integración contra SQL Server. |
| `database` | Scripts SQL. |
| `docs` | Manuales y diagramas. |

## Flujo de trabajo con Git

- `main`: versiones entregables.
- `develop`: integración del trabajo del grupo.
- `feature/<nombre>`: una rama por tarea, que entra a `develop` mediante Pull Request.

Cada Pull Request se compila y se prueba automáticamente en GitHub Actions.
