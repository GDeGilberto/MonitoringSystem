﻿namespace Infrastructure.Models;

public partial class ProcInventarioModel
{
    public int IdReg { get; set; }

    public int? Idestacion { get; set; }

    public string? NoTanque { get; set; }

    public string? ClaveProducto { get; set; }

    public double? VolumenDisponible { get; set; }

    public double? Temperatura { get; set; }

    public DateTime? Fecha { get; set; }

    public virtual CatTanqueModel? CatTanque { get; set; }
}
