﻿namespace Infrastructure.Models;

public partial class CatValorInventarioModel
{
    public int Idestacion { get; set; }

    public int Claveproducto { get; set; }

    public string? Dias { get; set; }

    public decimal? Valordias { get; set; }

    public decimal? Porcentaje { get; set; }

    public int? CapPipa { get; set; }

    public string? Nombre { get; set; }
}
