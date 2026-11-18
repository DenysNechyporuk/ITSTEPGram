using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ITSTEPGram
{
    public partial class ContactsWindow : Window
    {
        public string OpenChatLogin { get; set; } = "";

        public ContactsWindow()
        {
            InitializeComponent();
            LoadContacts();
            SearchInput.Focus();
        }

        private void LoadContacts()
        {
            List<UserItem> contacts = DataService.GetContacts(Session.CurrentUser!.Login);
            ContactsList.ItemsSource = contacts;
            NoContactsText.Visibility = contacts.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void Search()
        {
            string query = SearchInput.Text.Trim();
            if (query == "")
            {
                ResultsList.ItemsSource = null;
                return;
            }
            ResultsList.ItemsSource = DataService.SearchUsers(Session.CurrentUser!.Login, query);
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            Search();
        }

        private void SearchInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Search();
            }
        }

        private void SearchInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            SearchHint.Visibility = SearchInput.Text == "" ? Visibility.Visible : Visibility.Collapsed;
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            int contactId = (int)button.Tag;

            DataService.AddContact(Session.CurrentUser!.Login, contactId);
            LoadContacts();
            Search();
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            int contactId = (int)button.Tag;

            DataService.RemoveContact(Session.CurrentUser!.Login, contactId);
            LoadContacts();
            Search();
        }

        private void WriteButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            OpenChatLogin = (string)button.Tag;
            DialogResult = true;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
