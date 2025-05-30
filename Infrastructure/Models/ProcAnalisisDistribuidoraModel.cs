namespace Infrastructure.Models;

public partial class ProcAnalisisDistribuidoraModel
{
    public int IdPlanta { get; set; }

    public string Claveproducto { get; set; } = null!;

    public DateTime Fecha { get; set; }

    public int? Capacidad { get; set; }

    public int? InventarioAnterior { get; set; }

    public int? InventarioRealTime { get; set; }

    public int? ReqEstaciones { get; set; }

    public decimal? Viajes { get; set; }

    public int? LitrosRecomendados { get; set; }
}
