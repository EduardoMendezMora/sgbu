using SGBU.Domain.Entidades;
using SGBU.Domain.Excepciones;
using SGBU.Domain.Repositorios;
using SGBU.Services.Seguridad;

namespace SGBU.Services.Catalogo;

// Casos de uso del catálogo: buscar materiales y agregar o editar uno.
public class ServicioCatalogo
{
    public const int TamanoPagina = 10;

    private readonly IFabricaUnidadDeTrabajo fabrica;

    public ServicioCatalogo(IFabricaUnidadDeTrabajo fabrica)
    {
        this.fabrica = fabrica;
    }

    public async Task<Pagina<Material>> BuscarAsync(string? texto, TipoMaterial? tipo, int numeroPagina)
    {
        await using IUnidadDeTrabajo uow = await fabrica.CrearAsync();
        return await uow.Materiales.BuscarAsync(texto, tipo, numeroPagina, TamanoPagina);
    }

    public async Task GuardarAsync(Sesion sesion, FormularioMaterial datos)
    {
        if (!sesion.Puede(Permiso.GestionarCatalogo))
        {
            throw new OperacionNoPermitidaException("No tiene permiso para modificar el catálogo.");
        }

        await using IUnidadDeTrabajo uow = await fabrica.CrearAsync();

        if (datos.Id == 0)
        {
            // Material nuevo: todos los ejemplares están disponibles.
            Material nuevo = CrearMaterial(datos, datos.StockTotal);
            int id = await uow.Materiales.AgregarAsync(nuevo);
            await uow.Auditoria.RegistrarAsync(new RegistroAuditoria(sesion.UsuarioId, "MATERIAL_CREADO", "Materiales", id, nuevo.Titulo, DateTime.Now));
        }
        else
        {
            Material? actual = await uow.Materiales.ObtenerPorIdAsync(datos.Id);
            if (actual == null)
            {
                throw new DatoInvalidoException("El material ya no existe.");
            }

            // Si cambia el total, el disponible cambia lo mismo. Los prestados se mantienen.
            int prestados = actual.StockTotal - actual.StockDisponible;
            int disponible = datos.StockTotal - prestados;
            if (disponible < 0)
            {
                throw new DatoInvalidoException("El total no puede ser menor que los ejemplares prestados (" + prestados + ").");
            }

            Material editado = CrearMaterial(datos, disponible);
            await uow.Materiales.ActualizarAsync(editado);
            await uow.Auditoria.RegistrarAsync(new RegistroAuditoria(sesion.UsuarioId, "MATERIAL_EDITADO", "Materiales", datos.Id, editado.Titulo, DateTime.Now));
        }

        await uow.ConfirmarAsync();
    }

    // Crea un Libro, una Revista o una Tesis según el tipo elegido.
    // Los constructores de Domain validan los datos (ISBN, stock, campos obligatorios).
    private static Material CrearMaterial(FormularioMaterial d, int disponible)
    {
        Material material;
        if (d.Tipo == TipoMaterial.Libro)
        {
            material = new Libro(d.Id, d.Isbn, d.Titulo, d.Autor, d.StockTotal, disponible, d.Edicion)
            {
                Editorial = d.Editorial, AnioPublicacion = d.AnioPublicacion, Categoria = d.Categoria, Activo = d.Activo,
            };
        }
        else if (d.Tipo == TipoMaterial.Revista)
        {
            material = new Revista(d.Id, d.Isbn, d.Titulo, d.Autor, d.StockTotal, disponible, d.NumeroVolumen, d.Periodicidad)
            {
                Editorial = d.Editorial, AnioPublicacion = d.AnioPublicacion, Categoria = d.Categoria, Activo = d.Activo,
            };
        }
        else
        {
            material = new Tesis(d.Id, d.Isbn, d.Titulo, d.Autor, d.StockTotal, disponible, d.Universidad ?? string.Empty, d.GradoAcademico, d.Tutor)
            {
                Editorial = d.Editorial, AnioPublicacion = d.AnioPublicacion, Categoria = d.Categoria, Activo = d.Activo,
            };
        }

        return material;
    }
}
