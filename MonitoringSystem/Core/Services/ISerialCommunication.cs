namespace MonitoringSystem.Core.Services
{
    internal interface ISerialCommunication
    {
        void Open();
        void Close();
        void WriteLine(string message);
        string ReadLine();
    }
}