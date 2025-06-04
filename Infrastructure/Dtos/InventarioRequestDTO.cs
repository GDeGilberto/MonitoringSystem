namespace Infrastructure.Dtos
{
    public class InventarioRequestDTO
    {
        public int? IdEstacion { get; set; }
        public int? NoTanque { get; set; }
        public string? ClaveProducto { get; set; }
        public double? VolumenDisponible { get; set; }
        public double? Temperatura { get; set; }
        public DateTime? Fecha { get; set; }
    }
}
