namespace SGBU.Services.Seguridad;

public interface IHasherContrasenas
{
    string Generar(string contrasena);

    bool Verificar(string contrasena, string hash);
}
