namespace Infrastructure.Models;

public partial class CatProductoModel
{
    public string ClaveProducto { get; set; } = null!;

    public int IdProductoDist { get; set; }

    public string? Tipo { get; set; }
}
