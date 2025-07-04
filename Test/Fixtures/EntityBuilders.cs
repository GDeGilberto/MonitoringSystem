using Domain.Entities;

namespace Test.Fixtures
{
    /// <summary>
    /// Builder simplificado para crear entidades de Inventario para testing
    /// </summary>
    public class InventarioEntityBuilder
    {
        private int? _idEstacion = 11162;
        private int? _noTanque = 1;
        private string? _claveProducto = "01"; // Magna
        private double? _volumenDisponible = 15000.0;
        private double? _temperatura = 25.5;
        private DateTime? _fecha = DateTime.Now;

        public InventarioEntityBuilder WithIdEstacion(int? idEstacion)
        {
            _idEstacion = idEstacion;
            return this;
        }

        public InventarioEntityBuilder WithNoTanque(string noTanque)
        {
            if (int.TryParse(noTanque, out int tanqueNum))
            {
                _noTanque = tanqueNum;
            }
            return this;
        }

        public InventarioEntityBuilder WithNoTanque(int? noTanque)
        {
            _noTanque = noTanque;
            return this;
        }

        public InventarioEntityBuilder WithNombreProducto(string nombreProducto)
        {
            // Map product names to codes
            _claveProducto = nombreProducto switch
            {
                "Magna" => "01",
                "Premium" => "02", 
                "Diesel" => "03",
                _ => nombreProducto
            };
            return this;
        }

        public InventarioEntityBuilder WithClaveProducto(string? claveProducto)
        {
            _claveProducto = claveProducto;
            return this;
        }

        public InventarioEntityBuilder WithVolumenDisponible(double? volumenDisponible)
        {
            _volumenDisponible = volumenDisponible;
            return this;
        }

        public InventarioEntityBuilder WithTemperatura(double? temperatura)
        {
            _temperatura = temperatura;
            return this;
        }

        public InventarioEntityBuilder WithFechaUltimaLectura(DateTime? fechaUltimaLectura)
        {
            _fecha = fechaUltimaLectura;
            return this;
        }

        public InventarioEntity Build()
        {
            return new InventarioEntity(
                _idEstacion,
                _noTanque,
                _claveProducto,
                _volumenDisponible,
                _temperatura,
                _fecha
            );
        }
    }

    /// <summary>
    /// Builder simplificado para crear entidades de Descarga para testing
    /// </summary>
    public class DescargaEntityBuilder
    {
        private int _id = 1;
        private int _idEstacion = 11162;
        private string _noTanque = "1";
        private double _volumenInicial = 5000.0;
        private double? _temperaturaInicial = 24.0;
        private DateTime _fechaInicial = DateTime.Now.AddHours(-2);
        private double _volumenDisponible = 20000.0;
        private double? _temperaturaFinal = 25.5;
        private DateTime _fechaFinal = DateTime.Now;
        private double _cantidadCargada = 15000.0;

        public DescargaEntityBuilder WithId(int id)
        {
            _id = id;
            return this;
        }

        public DescargaEntityBuilder WithIdEstacion(int idEstacion)
        {
            _idEstacion = idEstacion;
            return this;
        }

        public DescargaEntityBuilder WithNoTanque(string noTanque)
        {
            _noTanque = noTanque;
            return this;
        }

        public DescargaEntityBuilder WithVolumenInicial(double volumenInicial)
        {
            _volumenInicial = volumenInicial;
            return this;
        }

        public DescargaEntityBuilder WithCantidadCargada(double cantidadCargada)
        {
            _cantidadCargada = cantidadCargada;
            return this;
        }

        public DescargaEntityBuilder WithFechas(DateTime fechaInicial, DateTime fechaFinal)
        {
            _fechaInicial = fechaInicial;
            _fechaFinal = fechaFinal;
            return this;
        }

        public DescargasEntity Build()
        {
            return new DescargasEntity(
                _id,
                _idEstacion,
                int.Parse(_noTanque),
                _volumenInicial,
                _temperaturaInicial,
                _fechaInicial,
                _volumenDisponible,
                _temperaturaFinal,
                _fechaFinal,
                _cantidadCargada
            );
        }
    }
}