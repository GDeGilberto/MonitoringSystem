using Application.Models;

namespace Application.UseCases
{
    public class ParceDeliveryReport
    {
        public DeliveryTankReport Execute(string input)
        {
            ValidateInput(input);
            input = SanitizeInput(input);

            var reportDate = ExtractReportDate(input);
            var tanks = ProcessTanks(input);

            return new DeliveryTankReport
            {
                Date = reportDate,
                Tanks = tanks
            };
        }

        private static void ValidateInput(string input)
        {
            if (string.IsNullOrEmpty(input))
                throw new ArgumentException("El string no puede ser nulo o vacío.");
        }

        // Remover caracteres de control (SOH y ETX)
        private static string SanitizeInput(string input)
        {
            return input.Trim('\u0001', '\u0003');
        }

        private static DateTime ExtractReportDate(string input)
        {
            const int dateStartIndex = 6;
            const int dateLength = 10; // YYMMDDHHmm
            string dateString = input.Substring(dateStartIndex, dateLength);
            return DateTime.ParseExact(dateString, "yyMMddHHmm", null);
        }

        private static List<DeliveryTank> ProcessTanks(string input)
        {
            List<DeliveryTank> tanks = [];
            int currentIndex = 16; // Posición inicial del primer tanque

            while (currentIndex < input.Length)
            {
                if (ShouldStopProcessing(input, currentIndex)) break;

                var tank = ProcessSingleTank(input, ref currentIndex);
                tanks.Add(tank);

                if (IsEndOfTanksMarker(input, currentIndex)) break;
            }

            return tanks;
        }

        // Evitar desbordamiento
        private static bool ShouldStopProcessing(string input, int currentIndex)
        {
            return currentIndex + 2 >= input.Length; 
        }

        private static bool IsEndOfTanksMarker(string input, int currentIndex)
        {
            return currentIndex < input.Length && input.Substring(currentIndex, 2) == "&&";
        }

        private static DeliveryTank ProcessSingleTank(string input, ref int currentIndex)
        {
            var tankNumber = ExtractTankNumber(input, ref currentIndex);
            var productCode = ExtractProductCode(input, ref currentIndex);
            var deliveryCount = ExtractDeliveryCount(input, ref currentIndex);

            var tank = new DeliveryTank
            {
                NoTank = tankNumber,
                CountDeliveries = deliveryCount,
                Deliveries = ProcessDeliveries(input, deliveryCount, ref currentIndex)
            };

            return tank;
        }

        private static int ExtractTankNumber(string input, ref int currentIndex)
        {
            const int tankNumberLength = 2; //TT
            string tankNumberStr = input.Substring(currentIndex, tankNumberLength);
            currentIndex += tankNumberLength;
            return int.Parse(tankNumberStr);
        }

        private static char ExtractProductCode(string input, ref int currentIndex)
        {
            char productCode = input[currentIndex];
            currentIndex += 1; //p
            return productCode;
        }

        private static int ExtractDeliveryCount(string input, ref int currentIndex)
        {
            const int deliveryCountLength = 2; //dd
            string deliveryCountStr = input.Substring(currentIndex, deliveryCountLength);
            currentIndex += deliveryCountLength;
            return int.Parse(deliveryCountStr);
        }

        private static List<FullDeliveries> ProcessDeliveries(string input, int deliveryCount, ref int currentIndex)
        {
            var deliveries = new List<FullDeliveries>();

            for (int i = 0; i < deliveryCount; i++)
            {
                var delivery = ProcessSingleDelivery(input, ref currentIndex);
                deliveries.Add(delivery);
            }

            return deliveries;
        }

        private static FullDeliveries ProcessSingleDelivery(string input, ref int currentIndex)
        {
            const int dateTimeLength = 10; // YYMMDDHHmm

            var startDate = ExtractDateTime(input, ref currentIndex, dateTimeLength);
            var endDate = ExtractDateTime(input, ref currentIndex, dateTimeLength);
            var fieldCount = ExtractFieldCount(input, ref currentIndex);

            var delivery = new FullDeliveries
            {
                Start = new DeliveryTankData { Date = startDate },
                End = new DeliveryTankData { Date = endDate }
            };

            ProcessDeliveryFields(input, ref currentIndex, fieldCount, delivery);

            return delivery;
        }

        private static DateTime ExtractDateTime(string input, ref int currentIndex, int length)
        {
            string dateString = input.Substring(currentIndex, length);
            currentIndex += length;
            return DateTime.ParseExact(dateString, "yyMMddHHmm", null);
        }

        private static int ExtractFieldCount(string input, ref int currentIndex)
        {
            const int fieldCountLength = 2; //NN
            string fieldCountStr = input.Substring(currentIndex, fieldCountLength);
            currentIndex += fieldCountLength;
            return int.Parse(fieldCountStr, System.Globalization.NumberStyles.HexNumber);
        }

        private static void ProcessDeliveryFields(string input, ref int currentIndex, int fieldCount, FullDeliveries delivery)
        {
            for (int fieldNumber = 1; fieldNumber <= fieldCount; fieldNumber++)
            {
                float value = ExtractFloatValue(input, currentIndex);
                currentIndex += 8; //FFFFFFFF

                AssignFieldValue(delivery, fieldNumber, value);
            }
        }

        private static void AssignFieldValue(FullDeliveries delivery, int fieldNumber, float value)
        {
            switch (fieldNumber)
            {
                case 1: delivery.Start.Volume = value; break;
                case 2: delivery.Start.TCVolume = value; break;
                case 3: delivery.Start.Water = value; break;
                case 4: delivery.Start.Temperature = value; break;
                case 5: delivery.End.Volume = value; break;
                case 6: delivery.End.TCVolume = value; break;
                case 7: delivery.End.Water = value; break;
                case 8: delivery.End.Temperature = value; break;
                case 9: delivery.Start.Height = value; break;
                case 10: delivery.End.Height = value; break;
            }
        }

        private static float ExtractFloatValue(string input, int startIndex)
        {
            const int hexValueLength = 8; //FFFFFFFF
            string hexValue = input.Substring(startIndex, hexValueLength);
            byte[] bytes = ConvertHexStringToByteArray(hexValue);
            return ConvertByteArrayToFloat(bytes);
        }

        private static byte[] ConvertHexStringToByteArray(string hexValue)
        {
            return Enumerable.Range(0, hexValue.Length / 2)
                            .Select(x => Convert.ToByte(hexValue.Substring(x * 2, 2), 16))
                            .ToArray();
        }

        private static float ConvertByteArrayToFloat(byte[] bytes)
        {
            return BitConverter.ToSingle(bytes.Reverse().ToArray(), 0);
        }
    }
}
