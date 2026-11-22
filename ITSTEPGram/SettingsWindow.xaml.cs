using System.Windows;
using System.Windows.Media;

namespace ITSTEPGram
{
    public partial class SettingsWindow : Window
    {
        public bool IsLogout { get; set; } = false;
        public bool IsSwitchAccount { get; set; } = false;

        public SettingsWindow()
        {
            InitializeComponent();

            Messenger_db.Client user = Session.CurrentUser!;
            NameInput.Text = user.Name;
            LoginText.Text = user.Login;
            BioInput.Text = DataService.GetBio(user.Login);
            AvatarText.Text = user.Name == "" ? "?" : user.Name.Substring(0, 1).ToUpper();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string name = NameInput.Text.Trim();
            string newPassword = NewPasswordInput.Password;

            if (name == "")
            {
                ShowMessage("Ім'я не може бути порожнім", true);
                return;
            }

            if (newPassword != "" && !DataService.IsPasswordStrong(newPassword))
            {
                ShowMessage("Пароль: від 6 символів, великі й малі латинські літери, цифра та _ або -", true);
                return;
            }

            DataService.UpdateProfile(Session.CurrentUser!.Id, name, BioInput.Text.Trim(), newPassword);
            Session.CurrentUser!.Name = name;
            AvatarText.Text = name.Substring(0, 1).ToUpper();
            NewPasswordInput.Password = "";
            ShowMessage("Збережено", false);
        }

        private void SwitchAccountButton_Click(object sender, RoutedEventArgs e)
        {
            IsSwitchAccount = true;
            Close();
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            IsLogout = true;
            Close();
        }

        private void ShowMessage(string text, bool isError)
        {
            MessageText.Text = text;

            if (isError)
            {
                MessageText.Foreground = (Brush)FindResource("DangerBrush");
            }
            else
            {
                MessageText.Foreground = (Brush)FindResource("AccentLightBrush");
            }
        }
    }
}
