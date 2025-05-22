namespace Application.Models;

public class Tank
{
    public int NoTank { get; set; }
    public char CodeProduct { get; set; }
    public int StatusTank { get; set; }
    public int NumberDataHex { get; set; }
    public TankData? TankData { get; set; }
}
