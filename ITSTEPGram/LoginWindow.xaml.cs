using System.Windows;
using System.Windows.Input;

namespace ITSTEPGram
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
            LoginInput.Focus();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            TryLogin();
        }

        private void PasswordInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                TryLogin();
            }
        }

        private void TryLogin()
        {
            string login = LoginInput.Text.Trim();
            string password = PasswordInput.Password;

            if (login == "" || password == "")
            {
                ErrorText.Text = "Введіть email і пароль";
                return;
            }

            Messenger_db.Client? user = DataService.Login(login, password);
            if (user == null)
            {
                ErrorText.Text = "Неправильний email або пароль";
                return;
            }

            Session.CurrentUser = user;
            MainWindow main = new MainWindow();
            main.Show();
            Close();
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            RegisterWindow register = new RegisterWindow();
            register.Owner = this;
            if (register.ShowDialog() == true)
            {
                LoginInput.Text = register.RegisteredLogin;
                PasswordInput.Password = "";
                ErrorText.Text = "";
                PasswordInput.Focus();
            }
        }
    }
}
