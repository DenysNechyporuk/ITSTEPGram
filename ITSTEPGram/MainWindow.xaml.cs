using ITSTEPGram.Client;
using ITSTEPGram.Common;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace ITSTEPGram
{
    public partial class MainWindow : Window
    {
        private ChatItem? selectedChat;
        private bool isLoadingChats = false;
        private List<int> joinedRooms = new List<int>();
        private DispatcherTimer requestTimer;
        private HashSet<int> knownRequestIds = new HashSet<int>();
        private bool isChangingAccount = false;

        public MainWindow()
        {
            InitializeComponent();

            UserNameText.Text = Session.CurrentUser!.Name;
            StatusText.Text = "підключення...";

            Session.Network.MessageReceived += Network_MessageReceived;
            Session.Network.Disconnected += Network_Disconnected;

            knownRequestIds = DataService.GetIncomingRequestIds(Session.CurrentUser.Login).ToHashSet();

            requestTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(5)
            };

            requestTimer.Tick += RequestTimer_Tick;
            requestTimer.Start();

            LoadChats();
            ConnectToServer();
        }

        private void RequestTimer_Tick(object? sender, EventArgs e)
        {
            if (Session.CurrentUser == null)
            {
                return;
            }

            List<int> currentIds = DataService.GetIncomingRequestIds(Session.CurrentUser.Login);
            List<int> newIds = currentIds.Where(id => !knownRequestIds.Contains(id)).ToList();

            knownRequestIds = currentIds.ToHashSet();

            if (newIds.Count > 0)
            {
                StatusText.Text = "Нова заявка в контакти";
                MessageBox.Show(
                    "Ви отримали нову заявку в контакти.",
                    "ITSTEPGram",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }

        private async void ConnectToServer()
        {
            try
            {
                await Session.Network.ConnectAsync(
                    Session.ServerIp,
                    Session.Port,
                    Session.CurrentUser!.Login);

                StatusText.Text = "в мережі";
                JoinRooms();
            }
            catch (Exception)
            {
                StatusText.Text = "сервер недоступний";
            }
        }

        private async void JoinRooms()
        {
            if (!Session.Network.IsConnected || Session.CurrentUser == null)
            {
                return;
            }

            foreach (int chatId in DataService.GetGroupChatIds(Session.CurrentUser.Id))
            {
                if (!joinedRooms.Contains(chatId))
                {
                    joinedRooms.Add(chatId);

                    try
                    {
                        await Session.Network.JoinChatAsync(chatId);
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }

        private void Network_MessageReceived(object? sender, MessageReceivedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                if (e.Command.Sender == "Server")
                {
                    if (e.Command.Command != RequestCommand.ChatMessage)
                    {
                        StatusText.Text = e.Command.Message;
                    }

                    LoadChats();
                    return;
                }

                LoadChats();
                LoadMessages();
            });
        }

        private void Network_Disconnected(object? sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                StatusText.Text = "відключено від сервера";
            });
        }

        private void LoadChats()
        {
            isLoadingChats = true;

            List<ChatItem> chats = DataService.GetChats(
                Session.CurrentUser!.Id,
                SearchBox.Text.Trim());

            ChatsList.ItemsSource = chats;

            bool isFound = false;

            if (selectedChat != null)
            {
                foreach (ChatItem chat in chats)
                {
                    if (chat.Id == selectedChat.Id)
                    {
                        ChatsList.SelectedItem = chat;
                        selectedChat = chat;
                        ChatInfoText.Text = chat.Info;
                        isFound = true;
                    }
                }
            }

            if (selectedChat != null && !isFound && SearchBox.Text.Trim() == "")
            {
                CloseChat();
            }

            NoChatsText.Visibility =
                chats.Count == 0 && SearchBox.Text.Trim() == ""
                    ? Visibility.Visible
                    : Visibility.Collapsed;

            isLoadingChats = false;
            JoinRooms();
        }

        private void ChatsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isLoadingChats)
            {
                return;
            }

            ChatItem? chat = ChatsList.SelectedItem as ChatItem;

            if (chat != null)
            {
                OpenChat(chat);
            }
        }

        private void OpenChat(ChatItem chat)
        {
            selectedChat = chat;

            ChatTitleText.Text = chat.Name;
            ChatInfoText.Text = chat.Info;
            ChatAvatarText.Text = chat.Letter;

            EmptyPanel.Visibility = Visibility.Collapsed;
            ChatPanel.Visibility = Visibility.Visible;

            LoadMessages();
            MessageInput.Focus();
        }

        private void SelectChat(int chatId)
        {
            SearchBox.Text = "";
            selectedChat = new ChatItem { Id = chatId };

            LoadChats();

            if (ChatsList.SelectedItem is ChatItem chat)
            {
                OpenChat(chat);
            }
        }

        private void CloseChat()
        {
            selectedChat = null;
            MessagesList.ItemsSource = null;
            ChatPanel.Visibility = Visibility.Collapsed;
            EmptyPanel.Visibility = Visibility.Visible;
        }

        private void LoadMessages()
        {
            if (selectedChat == null)
            {
                return;
            }

            ChatTitleText.Text = selectedChat.Name;
            ChatInfoText.Text = selectedChat.Info;

            MessagesList.ItemsSource = DataService.GetMessages(
                selectedChat.Id,
                Session.CurrentUser!.Id,
                selectedChat.IsGroup);

            MessagesScroll.UpdateLayout();
            MessagesScroll.ScrollToEnd();
        }

        private async void SendMessage()
        {
            string text = MessageInput.Text.Trim();

            if (text == "" || selectedChat == null)
            {
                return;
            }

            ChatItem chat = selectedChat;

            DataService.AddMessage(chat.Id, Session.CurrentUser!.Id, text);

            MessageInput.Text = "";

            LoadChats();
            LoadMessages();

            if (!Session.Network.IsConnected)
            {
                return;
            }

            try
            {
                if (chat.IsGroup)
                {
                    await Session.Network.SendChatMessageAsync(chat.Id, text);
                }
                else if (chat.OtherLogin != "")
                {
                    await Session.Network.SendPrivateMessageAsync(chat.OtherLogin, text);
                }
            }
            catch (Exception)
            {
                StatusText.Text = "сервер недоступний";
            }
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            SendMessage();
        }

        private void MessageInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SendMessage();
            }
        }

        private void MessageInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            MessageHint.Visibility =
                MessageInput.Text == ""
                    ? Visibility.Visible
                    : Visibility.Collapsed;
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            SearchHint.Visibility =
                SearchBox.Text == ""
                    ? Visibility.Visible
                    : Visibility.Collapsed;

            if (IsLoaded)
            {
                LoadChats();
            }
        }

        private void NewChatButton_Click(object sender, RoutedEventArgs e)
        {
            NewChatWindow newChat = new NewChatWindow();
            newChat.Owner = this;

            if (newChat.ShowDialog() != true)
            {
                return;
            }

            if (newChat.IsGroup)
            {
                foreach (MemberItem member in DataService.GetMembers(newChat.CreatedChatId))
                {
                    if (member.Login != Session.CurrentUser!.Login)
                    {
                        Session.SendNotice(
                            member.Login,
                            "Вас додано до групи " + newChat.GroupName);
                    }
                }
            }

            SelectChat(newChat.CreatedChatId);
        }

        private void ContactsButton_Click(object sender, RoutedEventArgs e)
        {
            ContactsWindow contacts = new ContactsWindow();
            contacts.Owner = this;

            if (contacts.ShowDialog() != true)
            {
                LoadChats();
                return;
            }

            string error;

            int chatId = DataService.CreatePrivateChat(
                Session.CurrentUser!.Id,
                contacts.OpenChatLogin,
                out error);

            if (chatId == 0)
            {
                MessageBox.Show(error, "ITSTEPGram");
                return;
            }

            SelectChat(chatId);
        }

        private void ChatInfoButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedChat == null)
            {
                return;
            }

            ChatInfoWindow info = new ChatInfoWindow(selectedChat);
            info.Owner = this;
            info.ShowDialog();

            LoadChats();
            LoadMessages();
        }

        private async void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow settings = new SettingsWindow();
            settings.Owner = this;
            settings.ShowDialog();

            if (settings.IsLogout || settings.IsSwitchAccount)
            {
                await SwitchToLogin();
                return;
            }

            UserNameText.Text = Session.CurrentUser!.Name;
            LoadChats();
        }

        private async Task SwitchToLogin()
        {
            isChangingAccount = true;

            requestTimer.Stop();

            ChatClient oldNetwork = Session.Network;

            oldNetwork.MessageReceived -= Network_MessageReceived;
            oldNetwork.Disconnected -= Network_Disconnected;

            if (Session.CurrentUser != null)
            {
                DataService.SetOffline(Session.CurrentUser.Login);
            }

            try
            {
                await oldNetwork.LeaveAsync();
            }
            catch (Exception)
            {
            }

            Session.CurrentUser = null;
            Session.Network = new ChatClient();

            LoginWindow login = new LoginWindow();
            login.Show();

            Close();
        }

        private async void Window_Closed(object sender, EventArgs e)
        {
            if (isChangingAccount)
            {
                return;
            }

            requestTimer.Stop();

            Session.Network.MessageReceived -= Network_MessageReceived;
            Session.Network.Disconnected -= Network_Disconnected;

            if (Session.CurrentUser != null)
            {
                DataService.SetOffline(Session.CurrentUser.Login);
                Session.CurrentUser = null;
            }

            ChatClient oldNetwork = Session.Network;
            Session.Network = new ChatClient();

            try
            {
                await oldNetwork.LeaveAsync();
            }
            catch (Exception)
            {
            }
        }
    }
}
