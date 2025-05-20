using MonitoringSystem.Infrastructure.SerialComm;

var serialComm = new SerialCommunication("COM3", 2400);

serialComm.Open();
while (true)
{
    Console.Write(">> ");
    string command = Console.ReadLine() ?? "";
    
    if (command.ToLower() == "exit") break;

    serialComm.Write(command);
    string response = serialComm.Read();
    Console.WriteLine(response);
}

serialComm.Close();