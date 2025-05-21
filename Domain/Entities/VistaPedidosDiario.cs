namespace Domain.Entities;

public partial class VistaPedidosDiario
{
    public int Idestacion { get; set; }

    public string? NomEstacion { get; set; }

    public double? Litros { get; set; }

    public string? Distribuidora { get; set; }

    public DateTime? Fecha { get; set; }

    public string? NomProducto { get; set; }
}
