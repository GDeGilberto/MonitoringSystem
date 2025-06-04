namespace Domain.Entities;

public class InventarioEntity
{
    public int Id { get; }

    public int? IdEstacion { get; }

    public int? NoTanque { get; }

    public string? ClaveProducto { get; }

    public double? VolumenDisponible { get; }

    public double? Temperatura { get; }

    public DateTime? Fecha { get; }

    public InventarioEntity(int? idEstacion, int? noTanque, string? claveProducto, double? volumenDisponible, double? temperatura, DateTime? fecha)
    {
        IdEstacion = idEstacion;
        NoTanque = noTanque;
        ClaveProducto = claveProducto;
        VolumenDisponible = volumenDisponible;
        Temperatura = temperatura;
        Fecha = fecha;
    }
}
