namespace Domain.Interfaces
{
    public interface ISerialPortService : IDisposable
    {
        event EventHandler<string> DataReceived;
        event EventHandler<string> CompleteResponseReceived;
        void Initialize(string portName, int baudRate);
        void Write(string data);
        void Close();
    }
}