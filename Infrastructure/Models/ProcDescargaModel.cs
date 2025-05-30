namespace Infrastructure.Models;

public partial class ProcDescargaModel
{
    public int Id { get; set; }

    public int IdEstacion { get; set; }

    public int NoTanque { get; set; }

    public double VolumenInicio { get; set; }

    public double? TemperaturaInicio { get; set; }

    public DateTime FechaInicio { get; set; }

    public double VolumenDisponible { get; set; }

    public double? TemperaturaFinal { get; set; }

    public DateTime FechaFinal { get; set; }

    public double CantidadCargada { get; set; }
}
