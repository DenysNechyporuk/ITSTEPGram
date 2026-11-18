using System.Windows;
using System.Windows.Controls;

namespace ITSTEPGram
{
    public partial class ChatInfoWindow : Window
    {
        private ChatItem chat;

        public ChatInfoWindow(ChatItem chat)
        {
            InitializeComponent();
            this.chat = chat;

            AvatarText.Text = chat.Letter;
            ChatNameText.Text = chat.Name;
            InfoText.Text = chat.Info;

            if (chat.IsGroup && DataService.IsChatAdmin(chat.Id, Session.CurrentUser!.Id))
            {
                AddPanel.Visibility = Visibility.Visible;
            }
            else
            {
                AddPanel.Visibility = Visibility.Collapsed;
            }

            AddLoginInput.TextChanged += AddLoginInput_TextChanged;
            LoadMembers();
        }

        private void LoadMembers()
        {
            MembersList.ItemsSource = DataService.GetMembers(chat.Id);
        }

        private void AddLoginInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            AddHint.Visibility = AddLoginInput.Text == "" ? Visibility.Visible : Visibility.Collapsed;
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            string login = AddLoginInput.Text.Trim();
            if (login == "")
            {
                ErrorText.Text = "Введіть email";
                return;
            }

            string error = DataService.AddMember(chat.Id, login);
            if (error != "")
            {
                ErrorText.Text = error;
                return;
            }

            Session.SendNotice(login, "Вас додано до групи " + chat.Name);
            ErrorText.Text = "";
            AddLoginInput.Text = "";
            LoadMembers();
        }

        private async void LeaveButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Ви дійсно хочете покинути цей чат?", "ITSTEPGram",
                MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            DataService.LeaveChat(chat.Id, Session.CurrentUser!.Id);
            if (chat.IsGroup && Session.Network.IsConnected)
            {
                try
                {
                    await Session.Network.LeaveChatAsync(chat.Id);
                }
                catch (Exception)
                {
                }
            }
            Close();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
