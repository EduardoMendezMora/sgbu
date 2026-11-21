#!/usr/bin/env bash
# Crea o actualiza la base SGBU en el SQL Server de Docker y corre las pruebas.
# Uso:  bash database/ejecutar.sh            (aplica los scripts y prueba)
#       bash database/ejecutar.sh --reiniciar (borra la base y la crea de cero)
#
# Necesita la variable SGBU_SA_PASSWORD (la clave del usuario sa). En el servidor está en
# /root/.sgbu_sql.env, que NO se sube al repositorio.
# En Windows no hace falta este archivo: se abren los .sql en SSMS en orden numérico.
set -euo pipefail

CONTENEDOR="${SGBU_CONTENEDOR:-sgbu-sql}"
DIR="$(cd "$(dirname "$0")" && pwd)"

if [[ -z "${SGBU_SA_PASSWORD:-}" && -f /root/.sgbu_sql.env ]]; then
    # shellcheck disable=SC1091
    source /root/.sgbu_sql.env
fi
: "${SGBU_SA_PASSWORD:?Falta SGBU_SA_PASSWORD}"

sql() {
    docker exec -i -e SQLCMDPASSWORD="$SGBU_SA_PASSWORD" "$CONTENEDOR" \
        /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -C -b -I "$@"
}

if [[ "${1:-}" == "--reiniciar" ]]; then
    echo "== Borrando la base SGBU"
    echo "IF DB_ID(N'SGBU') IS NOT NULL BEGIN ALTER DATABASE SGBU SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE SGBU; END" | sql
fi

for archivo in "$DIR"/[0-9][0-9][0-9]_*.sql; do
    echo "== $(basename "$archivo")"
    sql < "$archivo"
done

echo "== Pruebas"
sql < "$DIR/pruebas/verificar_restricciones.sql"
