namespace Domain.Entities;

public class TanquesEntity
{
    public int IdEstacion { get; }

    public string? NoTanque { get; }

    public string? Producto { get; }

    public decimal? Fondeja { get; }

    public decimal? Capacidad { get; }

    public decimal? Capacidad95 { get; }

    public List<InventarioEntity>? Inventarios { get; }

    public virtual EstacionesEntity? Estaciones { get; }
}
