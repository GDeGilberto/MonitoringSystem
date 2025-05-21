namespace Domain.Interfaces
{
    public interface ISerialCommunication
    {
        void Open();
        void Close();
        string Read();
        void Write(string message);
        bool IsOpen { get; }
    }
}
