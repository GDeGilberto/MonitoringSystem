namespace Application.Models
{
    public class TankReport
    {
        public DateTime Date { get; set; }
        public List<Tank>? Tanks { get; set; }
        public string? Checksum { get; set; }
    }

    public class  DeliveryTankReport
    {
        public DateTime Date { get; set; }
        public List<DeliveryTank>? Tanks { get; set; }
    }
}
