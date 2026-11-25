<#
    Crea (o actualiza) la base SGBU en Windows y corre las pruebas de la base.

    Uso, desde la carpeta del repositorio, en PowerShell:
        .\database\ejecutar.ps1                       # LocalDB (viene con Visual Studio)
        .\database\ejecutar.ps1 -Servidor localhost   # SQL Server 2022 Developer instalado
        .\database\ejecutar.ps1 -Reiniciar            # borra la base y la crea de cero

    Usa la autenticación de Windows: no hace falta escribir ninguna contraseña.
#>
param(
    [string]$Servidor = "(localdb)\MSSQLLocalDB",
    [switch]$Reiniciar
)

$ErrorActionPreference = "Stop"
$carpeta = $PSScriptRoot
$servidorApp = $Servidor   # el que usa la aplicación (se muestra al final)

if (-not (Get-Command sqlcmd -ErrorAction SilentlyContinue)) {
    throw "No se encontró sqlcmd. Instálelo con: winget install Microsoft.Sqlcmd"
}

# Hay dos sqlcmd: el clásico (ODBC, viene con SQL Server) y el nuevo de winget (go-sqlcmd).
# El clásico necesita -f 65001 para leer UTF-8 (tildes y ₡); el nuevo ya lee UTF-8 y no acepta -f.
$sqlcmdNuevo = (sqlcmd -? | Out-String) -like "*Install/Create/Query*"

if ($Servidor -like "(localdb)*") {
    sqllocaldb start MSSQLLocalDB | Out-Null

    # El sqlcmd nuevo no entiende "(localdb)": hay que darle la "tubería" (named pipe) de la instancia.
    if ($sqlcmdNuevo) {
        $pipe = (sqllocaldb info MSSQLLocalDB | Select-String "np:\\\S+").Matches.Value
        if (-not $pipe) { throw "No se pudo leer la tubería de LocalDB (sqllocaldb info MSSQLLocalDB)" }
        $Servidor = $pipe
    }
}

function Ejecutar([string]$archivo) {
    Write-Host "== $(Split-Path $archivo -Leaf)"
    # -b: detenerse si hay error · -I: QUOTED_IDENTIFIER (lo exigen los índices filtrados)
    if ($sqlcmdNuevo) {
        sqlcmd -S $Servidor -E -C -b -I -i $archivo
    } else {
        sqlcmd -S $Servidor -E -C -b -I -f 65001 -i $archivo
    }
    if ($LASTEXITCODE -ne 0) { throw "Falló $archivo" }
}

if ($Reiniciar) {
    Write-Host "== Borrando la base SGBU"
    sqlcmd -S $Servidor -E -C -b -Q "IF DB_ID(N'SGBU') IS NOT NULL BEGIN ALTER DATABASE SGBU SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE SGBU; END"
    if ($LASTEXITCODE -ne 0) { throw "No se pudo borrar la base" }
}

Get-ChildItem (Join-Path $carpeta "0*.sql") | Sort-Object Name | ForEach-Object { Ejecutar $_.FullName }
Ejecutar (Join-Path $carpeta "pruebas\verificar_restricciones.sql")

Write-Host ""
Write-Host "Listo. Cadena de conexión para la aplicación:"
Write-Host "  Server=$servidorApp;Database=SGBU;Integrated Security=True;TrustServerCertificate=True"
