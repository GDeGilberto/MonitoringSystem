namespace Infrastructure.ViewModels
{
    public class InventarioViewModel
    {
        public string? NombreEstacion { get; set; }
        public int NoTank { get; set; }
        public string? NombreProducto { get; set; }
        public float VolumenDisponible { get; set; }
        public float Temperatura { get; set; }
        public DateTime Fecha { get; set; }
    }
}
