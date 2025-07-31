using System.Net.WebSockets;
using System.Text;

class Program
{
    private static ClientWebSocket _webSocket = new ClientWebSocket();

    static async Task Main(string[] args)
    {
        Console.WriteLine("Connecting to server...");
        await ConnectToServer();

        _ = Task.Run(ReceiveMessages);

        Console.WriteLine("Type a message (or 'exit' to quit):");
        while (true)
        {
            string input = Console.ReadLine();
            if (input.ToLower() == "exit")
            {
                await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                break;
            }
            await SendMessage(input);
        }
    }

    static async Task ConnectToServer()
    {
        try
        {
            // connecting to server
            await _webSocket.ConnectAsync(new Uri("ws://localhost:5118/ws"), CancellationToken.None);
            Console.WriteLine("Connected to server!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Connection failed: {ex.Message}");
        }
    }

    static async Task SendMessage(string message)
    {
        try
        {
            var bytes = Encoding.UTF8.GetBytes(message);

            await _webSocket.SendAsync(
                new ArraySegment<byte>(bytes),
                WebSocketMessageType.Text,
                true,
                CancellationToken.None);
            Console.WriteLine($"Sent: {message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Send error: {ex.Message}");
        }
    }

    static async Task ReceiveMessages()
    {
        var buffer = new byte[1024 * 4]; // 4 KB size or 4096 bytes
        while (_webSocket.State == WebSocketState.Open)
        {
            try
            {
                var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Text)
                {
                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    Console.WriteLine($"Received: {message}");
                }
                else if (result.MessageType == WebSocketMessageType.Close)
                {
                    Console.WriteLine("Connection closed by server");
                    break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Receive error: {ex.Message}");
                break;
            }
        }
    }
}