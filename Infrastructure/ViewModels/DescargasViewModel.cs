namespace Infrastructure.ViewModels
{
    public class DescargasViewModel
    {
        public int NoTanque { get; set; }
        public string VolumenInicial { get; set; }
        public string? TemperaturaInicial { get; set; }
        public string FechaInicial { get; set; }
        public string VolumenDisponible { get; set; }
        public string? TemperaturaFinal { get; set; }
        public string FechaFinal { get; set; }
        public string CantidadCargada { get; set; }
    }
}
