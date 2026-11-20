# SGBU

Sistema de Gestión de Biblioteca Universitaria. Proyecto Programado de Programación IV, Grupo 3.

## Requisitos

- Windows 10 u 11
- Visual Studio 2026 con la carga "Desarrollo de escritorio de .NET" (incluye .NET 10)
- SQL Server 2022 Developer y SSMS

## Cómo ejecutarlo

```powershell
git clone https://github.com/EduardoMendezMora/sgbu.git
cd sgbu
dotnet build
dotnet test
dotnet run --project src/SGBU.UI
```

También se puede abrir `SGBU.slnx` en Visual Studio y pulsar F5 con `SGBU.UI` como proyecto de inicio.

## Estructura

| Carpeta | Contenido |
|---|---|
| `src/SGBU.Domain` | Entidades y reglas del negocio. No depende de nadie. |
| `src/SGBU.Data` | Repositorios ADO.NET contra SQL Server. |
| `src/SGBU.Services` | Casos de uso. Solo conoce Domain. |
| `src/SGBU.UI` | Aplicación WPF con MVVM. |
| `tests/SGBU.Tests` | Pruebas xUnit. |
| `database` | Scripts SQL. |
| `docs` | Manuales y diagramas. |

## Flujo de trabajo con Git

- `main`: versiones entregables.
- `develop`: integración del trabajo del grupo.
- `feature/<nombre>`: una rama por tarea, que entra a `develop` mediante Pull Request.

Cada Pull Request se compila y se prueba automáticamente en GitHub Actions.
