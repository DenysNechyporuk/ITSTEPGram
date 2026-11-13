namespace ITSTEPGram.Common
{
    // Команди, якими обмінюються клієнт і сервер
    public enum RequestCommand
    {
        Join, Leave, Message
    }

    public class ClientCommand
    {
        public RequestCommand Command { get; set; }
        public string Sender { get; set; } = "";
        public string Message { get; set; } = "";

        public ClientCommand()
        { }
        public ClientCommand(RequestCommand command)
        {
            Command = command;
        }
        public ClientCommand(string message, RequestCommand command = RequestCommand.Message)
        {
            Message = message;
            Command = command;
        }
    }
}
