namespace Domain.Entities;

public partial class ProcInventarioEntity
{
    public int IdReg { get; set; }

    public int? IdEstacion { get; set; }

    public int? NoTanque { get; set; }

    public string? ClaveProducto { get; set; }

    public double? VolumenDisponible { get; set; }

    public double? Temperatura { get; set; }

    public DateTime? Fecha { get; set; }
}
