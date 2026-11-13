using ITSTEPGram.Common;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;

namespace ITSTEPGram.Client
{
    public class MessageReceivedEventArgs : EventArgs
    {
        public ClientCommand Command { get; set; } = new ClientCommand();
    }

    public class ChatClient
    {
        private TcpClient client = new TcpClient();
        private StreamReader? reader;
        private StreamWriter? writer;

        public bool IsConnected => client.Connected;

        public event EventHandler<MessageReceivedEventArgs>? MessageReceived;
        public event EventHandler? Disconnected;

        public async Task ConnectAsync(string ip, int port, string name)
        {
            if (client.Connected) return;

            client = new TcpClient();
            await client.ConnectAsync(IPAddress.Parse(ip), port);

            NetworkStream ns = client.GetStream();
            reader = new StreamReader(ns);
            writer = new StreamWriter(ns);

            await SendAsync(new ClientCommand(RequestCommand.Join) { Sender = name });
            ServerListener();
        }

        public Task SendMessageAsync(string text)
        {
            return SendAsync(new ClientCommand(text));
        }

        public async Task LeaveAsync()
        {
            if (!client.Connected) return;

            await SendAsync(new ClientCommand(RequestCommand.Leave));
            client.Close();
        }

        private async Task SendAsync(ClientCommand command)
        {
            string json = JsonSerializer.Serialize<ClientCommand>(command);

            await writer!.WriteLineAsync(json);
            await writer.FlushAsync();
        }

        private async void ServerListener()
        {
            try
            {
                while (true)
                {
                    string? line = await reader!.ReadLineAsync();
                    if (line == null) break;

                    ClientCommand? command = JsonSerializer.Deserialize<ClientCommand>(line);
                    if (command != null && command.Command == RequestCommand.Message)
                    {
                        MessageReceived?.Invoke(this, new MessageReceivedEventArgs { Command = command });
                    }
                }
            }
            catch (Exception)
            {
            }
            Disconnected?.Invoke(this, EventArgs.Empty);
        }
    }
}
