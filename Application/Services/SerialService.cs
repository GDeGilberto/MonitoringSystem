using Domain.Interfaces;

namespace Application.Services
{
    public class SerialService
    {
        private readonly ISerialCommunication _serialComm;

        public SerialService(ISerialCommunication serialComm)
        {
            _serialComm = serialComm;
        }

        public void Run()
        {
            _serialComm.Open();

            try
            {
                while (true)
                {
                    Console.Write(">> ");
                    string command = Console.ReadLine() ?? "";

                    if (command.ToLower() == "exit") break;

                    _serialComm.Write(command);
                    string response = _serialComm.Read();
                    Console.WriteLine(response);
                }
            }
            finally
            {
                _serialComm.Close();
            }
        }
    }
}
