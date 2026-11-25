using SGBU.Domain.Entidades;
using SGBU.Domain.Excepciones;
using SGBU.Services.Multas;
using SGBU.Services.Seguridad;
using Xunit;

namespace SGBU.Tests.Services;

public class ServicioMultasTests
{
    private static readonly Sesion Admin = new(1, "Ana Rojas", "Administrador", DateTime.Now);
    private static readonly Sesion Maria = new(5, "María Vargas", "Estudiante", DateTime.Now);

    [Fact]
    public async Task ListarPendientes_TraeTodasYSumaElTotal()
    {
        var (servicio, _) = Crear();

        var multas = await servicio.ListarPendientesAsync(Admin, null);

        Assert.Equal(2, multas.Count);
        Assert.Equal(550m, ServicioMultas.Total(multas));
    }

    [Fact]
    public async Task Pagar_LlamaAlProcedimientoYAhoraElUsuarioNoDebe()
    {
        var (servicio, fabrica) = Crear();

        await servicio.PagarAsync(Admin, 1);

        Assert.Equal(["pagar 1"], fabrica.LlamadasProcedimientos);
        Assert.Equal(1, fabrica.Confirmaciones);
        Assert.Single(await servicio.ListarPendientesAsync(Admin, null));
    }

    [Fact]
    public async Task Pagar_MultaYaPagada_NoLlamaAlProcedimiento()
    {
        var (servicio, fabrica) = Crear();
        fabrica.Multas.Add(new Multa(3, 9, 5, 1, 50m, EstadoMulta.Pagada, DateTime.Now, DateTime.Now));

        await Assert.ThrowsAsync<OperacionNoPermitidaException>(() => servicio.PagarAsync(Admin, 3));
        Assert.Empty(fabrica.LlamadasProcedimientos);
    }

    [Fact]
    public async Task Pagar_MultaQueNoExiste_DaDatoInvalido()
    {
        var (servicio, _) = Crear();

        await Assert.ThrowsAsync<DatoInvalidoException>(() => servicio.PagarAsync(Admin, 99));
    }

    [Fact]
    public async Task EstudianteNoPuedeVerTodasNiPagar_PeroSiVeLasSuyas()
    {
        var (servicio, _) = Crear();

        await Assert.ThrowsAsync<OperacionNoPermitidaException>(() => servicio.ListarPendientesAsync(Maria, null));
        await Assert.ThrowsAsync<OperacionNoPermitidaException>(() => servicio.PagarAsync(Maria, 1));

        var mias = await servicio.ListarMisMultasAsync(Maria);
        Assert.Equal(400m, Assert.Single(mias).Monto);
    }

    private static (ServicioMultas Servicio, FabricaFalsa Fabrica) Crear()
    {
        var fabrica = new FabricaFalsa();
        fabrica.Multas.Add(new Multa(1, 7, 5, 8, 400m, EstadoMulta.Pendiente, DateTime.Now, null));
        fabrica.Multas.Add(new Multa(2, 8, 6, 3, 150m, EstadoMulta.Pendiente, DateTime.Now, null));
        return (new ServicioMultas(fabrica), fabrica);
    }
}
