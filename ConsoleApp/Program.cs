using System;
using System.IO;
using System.Net.Sockets;

const string serverAddress = "127.0.0.1";
const int serverPort = 7070;

TcpClient client = new TcpClient();

try
{
    await client.ConnectAsync(serverAddress, serverPort);
    Console.WriteLine("Connected to the server!");

    using var stream = client.GetStream();
    using var reader = new StreamReader(stream);
    using var writer = new StreamWriter(stream);

    _ = Task.Run(async () =>
    {
        while (true)
        {
            string input = Console.ReadLine();
            if (input != null)
            {
                await writer.WriteLineAsync(input);
                await writer.FlushAsync();
            }
        }
    });

    while (true)
    {
        string serverMessage = await reader.ReadLineAsync();
        if (serverMessage == null)
            break;

        Console.WriteLine("Server: " + serverMessage);
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}
