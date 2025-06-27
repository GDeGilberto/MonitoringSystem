using System.IO.Ports;

namespace Infrastructure.Models
{
    public class SerialPortSettings
    {
        public string PortName { get; set; } = "COM1";
        public int BaudRate { get; set; } = 9600;
        public int DataBits { get; set; } = 8;
        public string Parity { get; set; } = "None";
        public string StopBits { get; set; } = "One";
        public string Handshake { get; set; } = "None";
        public int ReadTimeout { get; set; } = 500;
        public int WriteTimeout { get; set; } = 500;

        public Parity GetParity()
        {
            return Parity.ToLower() switch
            {
                "none" => System.IO.Ports.Parity.None,
                "odd" => System.IO.Ports.Parity.Odd,
                "even" => System.IO.Ports.Parity.Even,
                "mark" => System.IO.Ports.Parity.Mark,
                "space" => System.IO.Ports.Parity.Space,
                _ => System.IO.Ports.Parity.None
            };
        }

        public StopBits GetStopBits()
        {
            return StopBits.ToLower() switch
            {
                "none" => System.IO.Ports.StopBits.None,
                "one" => System.IO.Ports.StopBits.One,
                "onepointfive" => System.IO.Ports.StopBits.OnePointFive,
                "two" => System.IO.Ports.StopBits.Two,
                _ => System.IO.Ports.StopBits.One
            };
        }

        public Handshake GetHandshake()
        {
            return Handshake.ToLower() switch
            {
                "none" => System.IO.Ports.Handshake.None,
                "xonxoff" => System.IO.Ports.Handshake.XOnXOff,
                "requesttosend" => System.IO.Ports.Handshake.RequestToSend,
                "requesttosendxonxoff" => System.IO.Ports.Handshake.RequestToSendXOnXOff,
                _ => System.IO.Ports.Handshake.None
            };
        }
    }
}