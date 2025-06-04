namespace Infrastructure.Dtos
{
    public class DescargaRequestDTO
    {
        public int? IdEstacion { get; set; }
        public int? NoTanque { get; set; }
        public double? VolumenInicial { get; set; }
        public double? TemperaturaInicial { get; set; }
        public DateTime? FechaInicial { get; set; }
        public double? VolumenDisponible { get; set; }
        public double? TemperaturaFinal { get; set; }
        public DateTime? FechaFinal { get; set; }
        public double? CantidadCargada { get; set; }
    }
}
