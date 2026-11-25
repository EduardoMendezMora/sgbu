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

if (-not (Get-Command sqlcmd -ErrorAction SilentlyContinue)) {
    throw "No se encontró sqlcmd. Instálelo con: winget install Microsoft.Sqlcmd"
}

if ($Servidor -like "(localdb)*") {
    sqllocaldb start MSSQLLocalDB | Out-Null
}

function Ejecutar([string]$archivo) {
    Write-Host "== $(Split-Path $archivo -Leaf)"
    # -b: detenerse si hay error · -I: QUOTED_IDENTIFIER (lo exigen los índices filtrados) · -f 65001: UTF-8 (tildes y ₡)
    sqlcmd -S $Servidor -E -C -b -I -f 65001 -i $archivo
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
Write-Host "  Server=$Servidor;Database=SGBU;Integrated Security=True;TrustServerCertificate=True"
