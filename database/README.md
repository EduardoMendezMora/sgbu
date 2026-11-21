# database

Scripts SQL del proyecto, numerados en el orden en que se ejecutan.

| Script | Contenido |
|---|---|
| `001_crear_base.sql` | Crea la base `SGBU` |
| `002_tablas.sql` | Roles, Usuarios, Materiales (Libro/Revista/Tesis en una tabla), Prestamos, Reservas, Auditoria |
| `003_tabla_multas.sql` | Tabla Multas: una por préstamo atrasado, monto entre 0 y 2000 |
| `004_indices.sql` | Índices para búsquedas y reportes |
| `005_datos_iniciales.sql` | Datos de prueba |
| `pruebas/verificar_restricciones.sql` | Intenta guardar datos inválidos y comprueba que la base los rechaza |

Todos los scripts se pueden ejecutar varias veces sin error ni datos duplicados.

## Usuarios de prueba

Contraseña de todos: `Sgbu2026!`

| Carné | Rol |
|---|---|
| ADM-001 | Administrador |
| PRF-001, PRF-002 | Profesor |
| EST-1001 a EST-1005 | Estudiante |

Límites por rol (propuestos, falta confirmar): Administrador 10 préstamos / 30 días,
Profesor 5 / 14 días, Estudiante 3 / 7 días.

## Cómo ejecutarlos

**Windows (PowerShell):** `.\database\ejecutar.ps1` (LocalDB) o
`.\database\ejecutar.ps1 -Servidor localhost` (SQL Server Developer). Agregar `-Reiniciar`
para borrar la base y crearla de cero.

**Windows (SSMS), a mano:** abrir cada `.sql` en orden numérico y ejecutarlo (F5). Después,
`pruebas/verificar_restricciones.sql`; al final debe decir `0 fallan`.

**Linux con Docker:**

```bash
bash database/ejecutar.sh             # aplica los scripts y corre las pruebas
bash database/ejecutar.sh --reiniciar # borra la base y la crea de cero
```

Necesita la variable `SGBU_SA_PASSWORD`. La cadena de conexión real nunca se sube al repositorio.
