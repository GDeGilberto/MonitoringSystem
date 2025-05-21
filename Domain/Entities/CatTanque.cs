namespace Domain.Entities;

public partial class CatTanque
{
    public int IdEstacion { get; set; }

    public string NoTanque { get; set; } = null!;

    public string? Producto { get; set; }

    public decimal? Fondeja { get; set; }

    public decimal? Capacidad { get; set; }

    public decimal? Capacidad95 { get; set; }

    public virtual CatEstacione IdEstacionNavigation { get; set; } = null!;

    public virtual ICollection<ProcInventario> ProcInventarios { get; set; } = new List<ProcInventario>();
}
