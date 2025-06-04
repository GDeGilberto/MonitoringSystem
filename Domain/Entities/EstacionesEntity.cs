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
}
