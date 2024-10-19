using System.Net;
using System.Net.Sockets;

const int serverPort = 7070;
IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
TcpListener tcpListener = new TcpListener(ipAddress, serverPort);

Dictionary<string, TcpClient> clients = new();
Dictionary<string, int> clientNumbers = new();

Random random = new Random();
tcpListener.Start();
Console.WriteLine($"Server started on port '{serverPort}'...");

async Task SendToClientAsync(TcpClient client, string message)
{
    var stream = client.GetStream();
    var writer = new StreamWriter(stream);
    await writer.WriteLineAsync(message);
    await writer.FlushAsync();
}

async Task HandleClientAsync(TcpClient tcpClient)
{
    var stream = tcpClient.GetStream();
    var reader = new StreamReader(tcpClient.GetStream());
    var writer = new StreamWriter(tcpClient.GetStream());

    string username = null;
    int attempts = 5;

    try
    {

        await writer.WriteLineAsync("input your username:");
        await writer.FlushAsync();
        username = (await reader.ReadLineAsync()).Trim().ToLower();

        if (string.IsNullOrEmpty(username) || clients.ContainsKey(username))
        {
            await writer.WriteLineAsync("incorrect type of name entry.");
            await writer.FlushAsync();
            tcpClient.Close();
            return;
        }

        clients.Add(username, tcpClient);
        clientNumbers[username] = random.Next(1, 101);
        Console.WriteLine($"Client '{username}' connected. Number to guess: {clientNumbers[username]}");

        await SendToClientAsync(tcpClient, "You have 5 attempts to guess the number between 1 and 100.");


        while (attempts > 0)
        {
            await writer.WriteLineAsync($"Attempts left: {attempts}. Input your guess:");
            await writer.FlushAsync();

            string guessStr = await reader.ReadLineAsync();
            if (int.TryParse(guessStr, out int guess))
            {
                int targetNumber = clientNumbers[username];

                if (guess < targetNumber)
                    await SendToClientAsync(tcpClient, "More");

                else if (guess > targetNumber)
                    await SendToClientAsync(tcpClient, "Less");

                else
                {
                    await SendToClientAsync(tcpClient, "Equal! You've guessed the number!");
                    break;
                }
            }
            else
                await SendToClientAsync(tcpClient, "Wrong input. Please input a number.");

            attempts--;
        }

        if (attempts == 0)
            await SendToClientAsync(tcpClient, $"You are out of attempts! The number was {clientNumbers[username]}.");

    }

    catch (Exception ex)
    { Console.WriteLine($"{ex.GetType().Name}: {ex.Message}"); }

    finally
    {
        if (username != null)
        {
            clients.Remove(username);
            clientNumbers.Remove(username);
            Console.WriteLine($"Client '{username}' disconnected.");
        }
        tcpClient.Close();
    }
}

while (true)
{
    var tcpClient = await tcpListener.AcceptTcpClientAsync();
    Console.WriteLine("Client accepted!");

    _ = Task.Run(() => HandleClientAsync(tcpClient));
}
