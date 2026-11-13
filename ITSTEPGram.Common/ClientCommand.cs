namespace ITSTEPGram.Common
{
    // Команди, якими обмінюються клієнт і сервер.
    public enum RequestCommand
    {
        Join, Leave,
        Message,        // усі до всіх
        PrivateMessage, // один до одного
        ChatMessage,    // один до багатьох
        CreateChat,     // створити чат
        JoinChat,       // вступити в чат
        LeaveChat       // вийти з чату
    }

    public class ClientCommand
    {
        public const int GeneralChatId = 0;

        public RequestCommand Command { get; set; }
        public string Sender { get; set; } = "";
        public string Message { get; set; } = "";
        public string Receiver { get; set; } = "";
        public int ChatId { get; set; } = GeneralChatId;

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
