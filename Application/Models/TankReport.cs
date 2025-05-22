namespace Application.Models
{
    public class TankReport
    {
        public DateTime Date { get; set; }
        public List<Tank>? Tanks { get; set; }
        public string? Checksum { get; set; }
    }
}
