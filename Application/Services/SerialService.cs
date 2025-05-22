using Application.UseCases;
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
            try
            {
                _serialComm.Open();
                while (true)
                {
                    Console.Write(">> ");
                    string command = Console.ReadLine()?.Trim() ?? "";

                    if (string.IsNullOrEmpty(command)) continue;

                    switch (command.ToLower())
                    {
                        case "exit":
                            return;
                        case "help":
                            ShowHelp();
                            break;
                        case "clean":
                            Console.Clear();
                            break;
                        default:
                            ProcessCommand(command);
                            break;
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            finally
            {
                _serialComm.Close();
            }
        }

        private static void ShowHelp()
        {
            Console.WriteLine("I201XX - Display InTank Inventory Report");
            Console.WriteLine("clean - Clear console");
            Console.WriteLine("exit - Exit the program");
        }

        private void ProcessCommand(string command)
        {
            if (command.Length < 6) return;

            _serialComm.Write(command);
            string response = _serialComm.Read();

            if (char.IsUpper(command[0]))
                Console.WriteLine(response);
            else 
            {
                var respuesta = ParseTankInventoryReport.Execute(response);
            }
                
        }
    }
}
