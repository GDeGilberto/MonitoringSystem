namespace Domain.Entities;

public partial class CatTanquesEntity
{
    public int IdEstacion { get; set; }

    public string NoTanque { get; set; } = null!;

    public string? Producto { get; set; }

    public decimal? Fondeja { get; set; }

    public decimal? Capacidad { get; set; }

    public decimal? Capacidad95 { get; set; }

    public virtual CatEstacionesEntity IdEstacionNavigation { get; set; } = null!;

    public virtual ICollection<ProcInventarioEntity> ProcInventarios { get; set; } = new List<ProcInventarioEntity>();
}
