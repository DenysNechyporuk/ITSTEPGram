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

class Chat
{
    List<Member> members = new List<Member>();

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
            lock (members)
            {
                members.Add(member);
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
                        Broadcast(command);
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

    private void Broadcast(ClientCommand command)
    {
        string json = JsonSerializer.Serialize<ClientCommand>(command);
        lock (members)
        {
            foreach (var member in members)
            {
                try
                {
                    member.Writer.WriteLine(json);
                    member.Writer.Flush();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\t Cannot send to {member.Name}: {ex.Message}");
                }
            }
        }
    }
}
