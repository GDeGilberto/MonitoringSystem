namespace Infrastructure.Models;

public partial class VistaPedidoModel
{
    public int Idestacion { get; set; }

    public int? Idcliente { get; set; }

    public string ClaveProducto { get; set; } = null!;

    public DateTime Fechaventa { get; set; }

    public double? VolumenVenta { get; set; }

    public double Promedio { get; set; }

    public double InventarioActual { get; set; }

    public decimal CapTanque { get; set; }

    public decimal Pedidos { get; set; }
}
