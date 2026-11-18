using System.Windows;

namespace ITSTEPGram
{
    public partial class RegisterWindow : Window
    {
        public string RegisteredLogin { get; set; } = "";

        public RegisterWindow()
        {
            InitializeComponent();
            NameInput.Focus();
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            string name = NameInput.Text.Trim();
            string login = LoginInput.Text.Trim();
            string password = PasswordInput.Password;

            if (name == "" || login == "" || password == "")
            {
                ErrorText.Text = "Заповніть усі поля";
                return;
            }
            if (password != RepeatPasswordInput.Password)
            {
                ErrorText.Text = "Паролі не збігаються";
                return;
            }

            string error = DataService.Register(name, login, password);
            if (error != "")
            {
                ErrorText.Text = error;
                return;
            }

            RegisteredLogin = login;
            MessageBox.Show("Акаунт створено! Тепер увійдіть.", "ITSTEPGram");
            DialogResult = true;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
