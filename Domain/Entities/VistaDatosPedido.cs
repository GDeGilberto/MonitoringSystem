namespace Domain.Entities;
public partial class VistaDatosPedido
{
    public int? Idestacion { get; set; }

    public string? ClaveProducto { get; set; }

    public string? Descripcion { get; set; }

    public double Ayer { get; set; }

    public int? Hoy { get; set; }

    public double _7dias { get; set; }

    public double Promedio { get; set; }
}
