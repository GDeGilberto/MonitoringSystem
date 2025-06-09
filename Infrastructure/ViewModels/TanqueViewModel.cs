using Domain.Enums;

namespace Infrastructure.ViewModels
{
    public class TanqueViewModel
    {
        public int NoTanque { get; set; }
        public TipoProducto Producto { get; set; }
        public decimal? Volumen { get; set; }
        public decimal? Capacidad { get; set; }
        public decimal? Temperatura { get; set; }
    }
}
