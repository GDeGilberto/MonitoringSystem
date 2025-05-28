using Application.Models;

namespace Application.UseCases
{
    public class ParseTankInventoryReport
    {
        public TankReport Execute(string input)
        {
            if (string.IsNullOrEmpty(input)) throw new ArgumentException("El string no puede ser nulo o vacío.");

            // Remover caracteres de control (SOH y ETX)
            input = input.Trim('\u0001', '\u0003');

            // Extraer la fecha y el checksum
            string fechaHoraStr = input.Substring(6, 10); // YYMMDDHHmm
            DateTime fechaHora = DateTime.ParseExact(fechaHoraStr, "yyMMddHHmm", null);

            string checksum = input.Substring(input.LastIndexOf("&&") + 2); // Extraer el checksum después de &&
            input = input.Substring(0, input.LastIndexOf("&&")); // Remover el checksum y flag de terminación

            List<Tank> tanks = [];
            int index = 16; // Posición inicial donde empieza el primer tanque

            while (index < input.Length)
            {
                if (index + 2 >= input.Length) break; // Evitar desbordamiento

                // Extraer información del tanque
                string numeroTanqueStr = input.Substring(index, 2); // TT
                int numeroTanque = int.Parse(numeroTanqueStr);
                char codigoProducto = input[index + 2]; // p
                string estadoTanqueStr = input.Substring(index + 3, 4); // ssss
                int estadoTanque = int.Parse(estadoTanqueStr, System.Globalization.NumberStyles.HexNumber);
                string numeroCamposStr = input.Substring(index + 7, 2); // NN
                int numeroCampos = int.Parse(numeroCamposStr, System.Globalization.NumberStyles.HexNumber);

                // Extraer valores del tanque y asignarlos a sus propiedades
                index += 9; // Mover índice después de campos básicos
                Tank tank = new()
                {
                    NoTank = numeroTanque,
                    CodeProduct = codigoProducto,
                    StatusTank = estadoTanque,
                    NumberDataHex = numeroCampos,
                    TankData = new TankData() // Inicializar la estructura de datos del tanque
                };

                if (numeroCampos >= 1) tank.TankData.Volume = ExtractFloatValue(input, index);
                if (numeroCampos >= 2) tank.TankData.TCVolume = ExtractFloatValue(input, index + 8);
                if (numeroCampos >= 3) tank.TankData.Ullage = ExtractFloatValue(input, index + 16);
                if (numeroCampos >= 4) tank.TankData.Height = ExtractFloatValue(input, index + 24);
                if (numeroCampos >= 5) tank.TankData.Water = ExtractFloatValue(input, index + 32);
                if (numeroCampos >= 6) tank.TankData.Temperature = ExtractFloatValue(input, index + 40);
                if (numeroCampos >= 7) tank.TankData.WaterVolume = ExtractFloatValue(input, index + 48);

                tanks.Add(tank);
                index += numeroCampos * 8; // Avanzar el índice en función del número de campos

                // Si encontramos '&&', terminamos la lectura de tanques
                if (index < input.Length && input.Substring(index, 2) == "&&") break;
            }

            return new TankReport
            {
                Date = fechaHora,
                Tanks = tanks,
                Checksum = checksum
            };
        }

        // Función auxiliar para extraer valores flotantes en IEEE 754
        private static float ExtractFloatValue(string input, int startIndex)
        {
            string hexValue = input.Substring(startIndex, 8);
            byte[] bytes = Enumerable.Range(0, hexValue.Length / 2)
                                     .Select(x => Convert.ToByte(hexValue.Substring(x * 2, 2), 16))
                                     .ToArray();
            return BitConverter.ToSingle(bytes.Reverse().ToArray(), 0);
        }
    }
}
