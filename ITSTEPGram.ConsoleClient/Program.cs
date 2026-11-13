using ITSTEPGram.Client;

// Тимчасовий консольний клієнт для перевірки сервера (поки немає інтеграції з вікном WPF).
internal class Program
{
    private const string serverIp = "127.0.0.1";
    private const int port = 4040;

    private static async Task Main(string[] args)
    {
        Console.Write("Your name: ");
        string name = Console.ReadLine() ?? "Guest";

        ChatClient chat = new ChatClient();
        chat.MessageReceived += (sender, e) =>
        {
            Console.WriteLine($"{e.Command.Sender}: {e.Command.Message}");
        };
        chat.Disconnected += (sender, e) =>
        {
            Console.WriteLine("Disconnected");
        };

        await chat.ConnectAsync(serverIp, port, name);
        Console.WriteLine("Type a message and press Enter. /exit - leave");

        while (true)
        {
            string? text = Console.ReadLine();
            if (text == null || text == "/exit")
            {
                await chat.LeaveAsync();
                break;
            }
            await chat.SendMessageAsync(text);
        }
    }
}
