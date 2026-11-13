using ITSTEPGram.Client;
using ITSTEPGram.Common;

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
            ClientCommand c = e.Command;
            switch (c.Command)
            {
                case RequestCommand.PrivateMessage:
                    Console.WriteLine($"[private] {c.Sender} -> {c.Receiver}: {c.Message}");
                    break;
                case RequestCommand.ChatMessage:
                    Console.WriteLine($"[chat {c.ChatId}] {c.Sender}: {c.Message}");
                    break;
                default:
                    Console.WriteLine($"{c.Sender}: {c.Message}");
                    break;
            }
        };
        chat.Disconnected += (sender, e) =>
        {
            Console.WriteLine("Disconnected");
        };

        await chat.ConnectAsync(serverIp, port, name);
        PrintHelp();

        while (true)
        {
            string? text = Console.ReadLine();
            if (text == null || text == "/exit")
            {
                await chat.LeaveAsync();
                break;
            }

            if (text == "/help")
            {
                PrintHelp();
            }
            else if (text.StartsWith("/pm "))
            {
                string rest = text.Substring(4);
                int space = rest.IndexOf(' ');
                if (space < 0)
                    Console.WriteLine("Usage: /pm <name> <text>");
                else
                    await chat.SendPrivateMessageAsync(rest.Substring(0, space), rest.Substring(space + 1));
            }
            else if (text.StartsWith("/chat "))
            {
                string rest = text.Substring(6);
                int space = rest.IndexOf(' ');
                if (space < 0 || !int.TryParse(rest.Substring(0, space), out int chatId))
                    Console.WriteLine("Usage: /chat <id> <text>");
                else
                    await chat.SendChatMessageAsync(chatId, rest.Substring(space + 1));
            }
            else if (text.StartsWith("/create "))
            {
                await chat.CreateChatAsync(text.Substring(8));
            }
            else if (text.StartsWith("/join "))
            {
                if (int.TryParse(text.Substring(6), out int chatId))
                    await chat.JoinChatAsync(chatId);
                else
                    Console.WriteLine("Usage: /join <id>");
            }
            else if (text.StartsWith("/leavechat "))
            {
                if (int.TryParse(text.Substring(11), out int chatId))
                    await chat.LeaveChatAsync(chatId);
                else
                    Console.WriteLine("Usage: /leavechat <id>");
            }
            else
            {
                await chat.SendMessageAsync(text);
            }
        }
    }

    private static void PrintHelp()
    {
        Console.WriteLine("text                - message to everybody (general chat)");
        Console.WriteLine("/pm <name> <text>   - private message");
        Console.WriteLine("/create <title>     - create a chat (you become a member)");
        Console.WriteLine("/join <id>          - join a chat");
        Console.WriteLine("/chat <id> <text>   - message to chat members");
        Console.WriteLine("/leavechat <id>     - leave a chat");
        Console.WriteLine("/exit               - leave");
    }
}
