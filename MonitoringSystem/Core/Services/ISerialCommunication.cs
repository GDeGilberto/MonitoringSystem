namespace MonitoringSystem.Core.Services
{
    internal interface ISerialCommunication
    {
        void Open();
        void Close();
        void Write(string message);
        string Read();
    }
}