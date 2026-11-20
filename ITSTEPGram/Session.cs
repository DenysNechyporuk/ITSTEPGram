using ITSTEPGram.Client;

namespace ITSTEPGram
{
    public static class Session
    {
        public const string ServerIp = "127.0.0.1";
        public const int Port = 4040;

        public static Messenger_db.Client? CurrentUser { get; set; }
        public static ChatClient Network { get; set; } = new ChatClient();

        public static async void SendNotice(string login, string text)
        {
            if (!Network.IsConnected)
            {
                return;
            }

            try
            {
                await Network.SendPrivateMessageAsync(login, text);
            }
            catch (Exception)
            {
            }
        }
    }
}
