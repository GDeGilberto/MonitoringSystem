﻿namespace Domain.Entities;
public partial class ProcVentaProductoEntity
{
    public int Idestacion { get; set; }

    public string ClaveProducto { get; set; } = null!;

    public DateTime Fechaventa { get; set; }

    public double? VolumenVenta { get; set; }
}
