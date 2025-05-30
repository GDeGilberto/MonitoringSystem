namespace Domain.Entities;

public partial class CatProductosEntity
{
    public string ClaveProducto { get; set; } = null!;

    public int IdProductoDist { get; set; }

    public string? Tipo { get; set; }
}
