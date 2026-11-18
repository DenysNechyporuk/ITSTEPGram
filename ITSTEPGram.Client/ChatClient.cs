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

        // Один до одного
        public Task SendPrivateMessageAsync(string receiver, string text)
        {
            return SendAsync(new ClientCommand(text, RequestCommand.PrivateMessage) { Receiver = receiver });
        }

        // Один до багатьох
        public Task SendChatMessageAsync(int chatId, string text)
        {
            return SendAsync(new ClientCommand(text, RequestCommand.ChatMessage) { ChatId = chatId });
        }

        public Task CreateChatAsync(string name)
        {
            return SendAsync(new ClientCommand(name, RequestCommand.CreateChat));
        }

        public Task JoinChatAsync(int chatId)
        {
            return SendAsync(new ClientCommand(RequestCommand.JoinChat) { ChatId = chatId });
        }

        public Task LeaveChatAsync(int chatId)
        {
            return SendAsync(new ClientCommand(RequestCommand.LeaveChat) { ChatId = chatId });
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
                    if (command != null && (command.Command == RequestCommand.Message
                        || command.Command == RequestCommand.PrivateMessage
                        || command.Command == RequestCommand.ChatMessage))
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
