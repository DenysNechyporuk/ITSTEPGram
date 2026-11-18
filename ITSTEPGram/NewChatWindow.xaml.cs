using System.Windows;

namespace ITSTEPGram
{
    public partial class NewChatWindow : Window
    {
        public int CreatedChatId { get; set; }
        public bool IsGroup { get; set; }
        public string GroupName { get; set; } = "";

        public NewChatWindow()
        {
            InitializeComponent();
            LoginInput.Focus();
        }

        private void TypeRadio_Checked(object sender, RoutedEventArgs e)
        {
            if (PrivatePanel == null || GroupPanel == null)
            {
                return;
            }

            if (PrivateRadio.IsChecked == true)
            {
                PrivatePanel.Visibility = Visibility.Visible;
                GroupPanel.Visibility = Visibility.Collapsed;
            }
            else
            {
                PrivatePanel.Visibility = Visibility.Collapsed;
                GroupPanel.Visibility = Visibility.Visible;
            }
            ErrorText.Text = "";
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            string error;

            if (PrivateRadio.IsChecked == true)
            {
                string login = LoginInput.Text.Trim();
                if (login == "")
                {
                    ErrorText.Text = "Введіть email користувача";
                    return;
                }
                CreatedChatId = DataService.CreatePrivateChat(Session.CurrentUser!.Id, login, out error);
            }
            else
            {
                string name = GroupNameInput.Text.Trim();
                if (name == "")
                {
                    ErrorText.Text = "Введіть назву групи";
                    return;
                }
                CreatedChatId = DataService.CreateGroup(Session.CurrentUser!.Id, name, MembersInput.Text, out error);
                IsGroup = true;
                GroupName = name;
            }

            if (CreatedChatId == 0)
            {
                ErrorText.Text = error;
                return;
            }
            if (error != "")
            {
                MessageBox.Show(error, "ITSTEPGram");
            }
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
