namespace Domain.Entities;

public partial class ProcAnalisisPedido
{
    public int IdEstacion { get; set; }

    public string ClaveProducto { get; set; } = null!;

    public DateTime Fecha { get; set; }

    public int? InventarioAnterior { get; set; }

    public int? CapacidadTanque { get; set; }

    public int? InventarioTiempoReal { get; set; }

    public int? VentaAlMomento { get; set; }

    public int? PromVenta { get; set; }

    public int? PorVender { get; set; }

    public int? InventarioCierre { get; set; }

    public int? InventarioCierreRealTime { get; set; }

    public int? PedidoNecesario { get; set; }

    public int? PedidoRecibidos { get; set; }

    public int? PedidoRestante { get; set; }

    public decimal? DiasInventario { get; set; }
}
