namespace Infrastructure.Models;

public partial class ProcPedidosEstacioneModel
{
    public int IdPedido { get; set; }

    public int? Estacion { get; set; }

    public int? Idproducto { get; set; }

    public string? NomProducto { get; set; }

    public int? Cantidad { get; set; }

    public DateTime? FechaSolicitada { get; set; }

    public int? IdUsuarioCapturo { get; set; }

    public DateTime? FechaCapturada { get; set; }

    public int? EstatusPedido { get; set; }

    public int? IdUsuarioProgramo { get; set; }

    public DateTime? FechaProgramacion { get; set; }

    public int? IdUsuarioCancela { get; set; }

    public DateTime? FechaCancelacion { get; set; }

    public DateTime? FechaEntregado { get; set; }

    public int? IdEntrego { get; set; }

    public string? ComentariosEntrega { get; set; }

    public DateTime? FechaCierre { get; set; }

    public int? IdCierre { get; set; }
}
