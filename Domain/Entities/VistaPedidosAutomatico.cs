namespace Domain.Entities;

public partial class VistaPedidosAutomatico
{
    public string? Estacion { get; set; }

    public string? Clave { get; set; }

    public string? NomProducto { get; set; }

    public int? Ayer { get; set; }

    public int? Hoy { get; set; }

    public int? Promedio { get; set; }

    public int? Dias { get; set; }
}
