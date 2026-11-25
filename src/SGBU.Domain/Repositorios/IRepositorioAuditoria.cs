using SGBU.Domain.Entidades;

namespace SGBU.Domain.Repositorios;

public interface IRepositorioAuditoria
{
    Task RegistrarAsync(RegistroAuditoria registro);

    // Busca por acción o usuario, entre dos fechas, con paginación (más recientes primero).
    Task<Pagina<AuditoriaDetalle>> BuscarAsync(string? texto, DateTime desde, DateTime hasta, int numeroPagina, int tamanoPagina);
}
