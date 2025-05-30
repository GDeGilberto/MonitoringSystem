namespace Infrastructure.Models;

public partial class VistaPedidosDiarioModel
{
    public int Idestacion { get; set; }

    public string? NomEstacion { get; set; }

    public double? Litros { get; set; }

    public string? Distribuidora { get; set; }

    public DateTime? Fecha { get; set; }

    public string? NomProducto { get; set; }
}
