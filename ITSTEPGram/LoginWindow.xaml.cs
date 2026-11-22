using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ITSTEPGram
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();

            ContextMenu menu = new ContextMenu();

            List<string> accounts = AccountStorage.Load();

            if (accounts.Count > 0)
            {
                foreach (string account in accounts)
                {
                    MenuItem item = new MenuItem
                    {
                        Header = account,
                        Tag = account
                    };

                    item.Click += SavedAccount_Click;
                    menu.Items.Add(item);
                }

                LoginInput.ContextMenu = menu;
                LoginInput.Text = accounts[0];
            }

            LoginInput.Focus();
            LoginInput.CaretIndex = LoginInput.Text.Length;
        }

        private void SavedAccount_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem item && item.Tag is string login)
            {
                LoginInput.Text = login;
                PasswordInput.Password = "";
                PasswordInput.Focus();
            }
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

            AccountStorage.Save(login);
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
