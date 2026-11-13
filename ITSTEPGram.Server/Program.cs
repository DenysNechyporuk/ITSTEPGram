using ITSTEPGram.Common;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;

internal class Program
{
    private const string localIp = "127.0.0.1";
    private const int port = 4040;

    private static void Main(string[] args)
    {
        IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Parse(localIp), port);
        TcpListener server = new TcpListener(localEndPoint);
        Chat chat = new Chat();
        server.Start();
        Console.WriteLine($"Server started {localIp}:{port}");

        while (true)
        {
            Console.WriteLine("Waiting for a client ... ");
            TcpClient client = server.AcceptTcpClient();
            Console.WriteLine("\t Got a TcpClient");

            Task.Run(() => chat.ServeClient(client));
        }
    }
}

class Member
{
    public TcpClient Client { get; }
    public StreamReader Reader { get; }
    public StreamWriter Writer { get; }
    public string Name { get; set; } = "";

    public Member(TcpClient client)
    {
        Client = client;
        NetworkStream ns = client.GetStream();
        Reader = new StreamReader(ns);
        Writer = new StreamWriter(ns);
    }
}

class ChatRoom
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public List<Member> Members { get; } = new List<Member>();
}

class Chat
{
    List<Member> members = new List<Member>();
    Dictionary<int, ChatRoom> rooms = new Dictionary<int, ChatRoom>();
    int nextChatId = 1;

    public void ServeClient(TcpClient client)
    {
        Member member = new Member(client);
        bool isJoined = false;
        try
        {
            ClientCommand? first = GetClientCommand(member);
            if (first == null || first.Command != RequestCommand.Join)
                return;

            member.Name = string.IsNullOrWhiteSpace(first.Sender) ? "Guest" : first.Sender;
            bool isNameTaken = false;
            lock (members)
            {
                foreach (var m in members)
                {
                    if (m.Name == member.Name)
                        isNameTaken = true;
                }
                if (!isNameTaken)
                    members.Add(member);
            }
            if (isNameTaken)
            {
                lock (members)
                {
                    Send(member, ServerMessage($"Ім'я {member.Name} вже зайняте"));
                }
                return;
            }
            isJoined = true;
            Console.WriteLine($"\t {member.Name} joined");
            Broadcast(new ClientCommand($"{member.Name} приєднався до чату") { Sender = "Server" });

            bool isExit = false;
            while (!isExit)
            {
                ClientCommand? command = GetClientCommand(member);
                if (command == null)
                    break;

                switch (command.Command)
                {
                    case RequestCommand.Leave:
                        isExit = true;
                        break;
                    case RequestCommand.Message:
                        command.Sender = member.Name;
                        command.ChatId = ClientCommand.GeneralChatId;
                        Broadcast(command);
                        break;
                    case RequestCommand.PrivateMessage:
                        SendPrivate(member, command);
                        break;
                    case RequestCommand.ChatMessage:
                        SendToChat(member, command);
                        break;
                    case RequestCommand.CreateChat:
                        CreateChat(member, command);
                        break;
                    case RequestCommand.JoinChat:
                        JoinChat(member, command);
                        break;
                    case RequestCommand.LeaveChat:
                        LeaveChat(member, command);
                        break;
                    default:
                        break;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\t Error with {member.Name}: {ex.Message}");
        }
        finally
        {
            lock (members)
            {
                members.Remove(member);
                foreach (var pair in rooms)
                {
                    pair.Value.Members.Remove(member);
                }
            }
            member.Client.Close();

            if (isJoined)
            {
                Console.WriteLine($"\t {member.Name} left");
                Broadcast(new ClientCommand($"{member.Name} залишив чат") { Sender = "Server" });
            }
        }
    }

    private ClientCommand? GetClientCommand(Member member)
    {
        string? line = member.Reader.ReadLine();
        if (line == null)
            return null;
        return JsonSerializer.Deserialize<ClientCommand>(line);
    }

    private ClientCommand ServerMessage(string text)
    {
        return new ClientCommand(text) { Sender = "Server" };
    }

    private void Send(Member member, ClientCommand command)
    {
        try
        {
            member.Writer.WriteLine(JsonSerializer.Serialize<ClientCommand>(command));
            member.Writer.Flush();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\t Cannot send to {member.Name}: {ex.Message}");
        }
    }

    // Усі до всіх
    private void Broadcast(ClientCommand command)
    {
        lock (members)
        {
            foreach (var member in members)
            {
                Send(member, command);
            }
        }
    }

    // Один до одного
    private void SendPrivate(Member sender, ClientCommand command)
    {
        command.Sender = sender.Name;
        lock (members)
        {
            Member? receiver = null;
            foreach (var m in members)
            {
                if (m.Name == command.Receiver)
                    receiver = m;
            }
            if (receiver == null)
            {
                Send(sender, ServerMessage($"Користувача {command.Receiver} не знайдено"));
                return;
            }
            Send(receiver, command);
            if (receiver != sender)
                Send(sender, command);
        }
    }

    // Один до багатьох
    private void SendToChat(Member sender, ClientCommand command)
    {
        command.Sender = sender.Name;
        lock (members)
        {
            if (!rooms.TryGetValue(command.ChatId, out ChatRoom? room))
            {
                Send(sender, ServerMessage($"Чату {command.ChatId} не існує"));
                return;
            }
            if (!room.Members.Contains(sender))
            {
                Send(sender, ServerMessage($"Ви не є учасником чату {command.ChatId}"));
                return;
            }
            foreach (var m in room.Members)
            {
                Send(m, command);
            }
        }
    }

    private void CreateChat(Member creator, ClientCommand command)
    {
        lock (members)
        {
            ChatRoom room = new ChatRoom();
            room.Id = nextChatId;
            nextChatId++;
            room.Name = string.IsNullOrWhiteSpace(command.Message) ? $"Chat {room.Id}" : command.Message;
            room.Members.Add(creator);
            rooms.Add(room.Id, room);
            Console.WriteLine($"\t {creator.Name} created chat {room.Id} '{room.Name}'");
            Send(creator, ServerMessage($"Чат '{room.Name}' створено, ChatId = {room.Id}"));
        }
    }

    private void JoinChat(Member member, ClientCommand command)
    {
        lock (members)
        {
            if (!rooms.TryGetValue(command.ChatId, out ChatRoom? room))
            {
                Send(member, ServerMessage($"Чату {command.ChatId} не існує"));
                return;
            }
            if (room.Members.Contains(member))
            {
                Send(member, ServerMessage($"Ви вже в чаті {room.Id}"));
                return;
            }
            room.Members.Add(member);
            NotifyChat(room, $"{member.Name} приєднався до чату '{room.Name}'");
        }
    }

    private void LeaveChat(Member member, ClientCommand command)
    {
        lock (members)
        {
            if (!rooms.TryGetValue(command.ChatId, out ChatRoom? room) || !room.Members.Contains(member))
            {
                Send(member, ServerMessage($"Ви не є учасником чату {command.ChatId}"));
                return;
            }
            room.Members.Remove(member);
            Send(member, ServerMessage($"Ви вийшли з чату '{room.Name}'"));
            NotifyChat(room, $"{member.Name} залишив чат '{room.Name}'");
        }
    }

    private void NotifyChat(ChatRoom room, string text)
    {
        ClientCommand notice = new ClientCommand(text, RequestCommand.ChatMessage)
        {
            Sender = "Server",
            ChatId = room.Id
        };
        foreach (var m in room.Members)
        {
            Send(m, notice);
        }
    }
}
