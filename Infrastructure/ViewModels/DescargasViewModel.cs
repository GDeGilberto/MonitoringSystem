using System.ComponentModel;

namespace Infrastructure.ViewModels
{
    public class DescargasViewModel
    {
        [DisplayName("No. Tanque")]
        public int NoTanque { get; set; }
        
        [DisplayName("Vol. Inicial (m³)")]
        public string VolumenInicial { get; set; }
        
        [DisplayName("Temp. Ini. (°C)")]
        public string? TemperaturaInicial { get; set; }
        
        [DisplayName("Fecha Inicial")]
        public string FechaInicial { get; set; }
        
        [DisplayName("Vol. Disponible (m³)")]
        public string VolumenDisponible { get; set; }
        
        [DisplayName("Temp. Final (°C)")]
        public string? TemperaturaFinal { get; set; }
        
        [DisplayName("Fecha Final")]
        public string FechaFinal { get; set; }
        
        [DisplayName("Cant. Cargada (m³)")]
        public string CantidadCargada { get; set; }
    }
}
