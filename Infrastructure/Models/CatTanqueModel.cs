namespace Infrastructure.Models;

public partial class CatTanqueModel
{
    public int IdEstacion { get; set; }

    public string NoTanque { get; set; } = null!;

    public string? Producto { get; set; }

    public decimal? Fondeja { get; set; }

    public decimal? Capacidad { get; set; }

    public decimal? Capacidad95 { get; set; }

    public virtual CatEstacionesModel IdEstacionNavigation { get; set; } = null!;

    public virtual ICollection<ProcInventarioModel> ProcInventarios { get; set; } = new List<ProcInventarioModel>();
}
