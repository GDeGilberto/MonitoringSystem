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

    public TanquesEntity(
        int idEstacion,
        string? noTanque,
        string? producto,
        decimal? fondeja,
        decimal? capacidad,
        decimal? capacidad95,
        List<InventarioEntity>? inventarios
    )
    {
        IdEstacion = idEstacion;
        NoTanque = noTanque;
        Producto = producto;
        Fondeja = fondeja;
        Capacidad = capacidad;
        Capacidad95 = capacidad95;
        Inventarios = inventarios;
    }
}
