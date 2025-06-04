namespace Domain.Entities;

public class Tank
{
    public int NoTank { get; set; }
    public char CodeProduct { get; set; }
    public int StatusTank { get; set; }
    public int? NumberDataHex { get; set; }
    public TankData? TankData { get; set; }
}

public class DeliveryTank
{
    public int NoTank { get; set; }
    public int CountDeliveries { get; set; }
    public required List<FullDeliveries> Deliveries { get; set; }
}

public class FullDeliveries {
    public DeliveryTankData Start { get; set; } = new DeliveryTankData();
    public DeliveryTankData End { get; set; } = new DeliveryTankData();
}
