namespace Domain.Entities;

public class EstacionesEntity
{
    public int Id { get; }

    public string? Nombre { get; }

    public string? Direccion { get; }

    public string? Telefono { get; }

    public string? NomContacto { get; }

    public string? CorreoElectronico { get; }

    public string? Activa { get; }

    public DateTime? UltimoEnvio { get; }

    public DateTime? EnvioCorreo { get; }

    public string? DistUbicacion { get; }

    public string? TipoCliente { get; }

    public int? IdCliente { get; }

    public string? Atiende { get; }

    public int? IdZonaPrecio { get; }

    public DateTime? UltimaActualizacionAnalisis { get; }

    public int? IdEstacionAutoabasto { get; }

    public List<TanquesEntity>? Tanques { get; }

    public EstacionesEntity(
        int id,
        string? nombre,
        string? direccion,
        string? telefono,
        string? nomContacto,
        string? correoElectronico,
        string? activa,
        DateTime? ultimoEnvio,
        DateTime? envioCorreo,
        string? distUbicacion,
        string? tipoCliente,
        int? idCliente,
        string? atiende,
        int? idZonaPrecio,
        DateTime? ultimaActualizacionAnalisis,
        int? idEstacionAutoabasto,
        List<TanquesEntity>? tanques
    )
    {
        Id = id;
        Nombre = nombre;
        Direccion = direccion;
        Telefono = telefono;
        NomContacto = nomContacto;
        CorreoElectronico = correoElectronico;
        Activa = activa;
        UltimoEnvio = ultimoEnvio;
        EnvioCorreo = envioCorreo;
        DistUbicacion = distUbicacion;
        TipoCliente = tipoCliente;
        IdCliente = idCliente;
        Atiende = atiende;
        IdZonaPrecio = idZonaPrecio;
        UltimaActualizacionAnalisis = ultimaActualizacionAnalisis;
        IdEstacionAutoabasto = idEstacionAutoabasto;
        Tanques = tanques;
    }
}
