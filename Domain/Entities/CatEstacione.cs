namespace Domain.Entities;

public partial class CatEstacione
{
    public int IdEstacion { get; set; }

    public string? Nombre { get; set; }

    public string? Direccion { get; set; }

    public string? Telefono { get; set; }

    public string? NomContacto { get; set; }

    public string? CorreoElectronico { get; set; }

    public string? Activa { get; set; }

    public DateTime? UltimoEnvio { get; set; }

    public DateTime? EnvioCorreo { get; set; }

    public string? DistUbicacion { get; set; }

    public string? TipoCliente { get; set; }

    public int? Idcliente { get; set; }

    public string? Atiende { get; set; }

    public int? IdZonaPrecio { get; set; }

    public DateTime? UltimaActualizacionAnalisis { get; set; }

    public int? IdEstacionAutoabasto { get; set; }

    public virtual ICollection<CatTanque> CatTanques { get; set; } = new List<CatTanque>();
}
