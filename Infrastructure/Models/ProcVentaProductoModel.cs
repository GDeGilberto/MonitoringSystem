namespace Infrastructure.Models;

public partial class ProcVentaProductoModel
{
    public int Idestacion { get; set; }

    public string ClaveProducto { get; set; } = null!;

    public DateTime Fechaventa { get; set; }

    public double? VolumenVenta { get; set; }
}
