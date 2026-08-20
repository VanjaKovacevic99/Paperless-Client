using HCI_2025_Project.Services;
using System.Windows;

namespace HCI_2025_Project_Template.Views
{
    public partial class LoginWindow : Window
    {
        private readonly PaperlessApiService _apiService;

        public LoginWindow(PaperlessApiService apiService)
        {
            InitializeComponent();

            _apiService = apiService;

        }

        private async void Login_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text.Trim();
            string password = PasswordBox.Password;

            ErrorText.Text = "";

            if (string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(password))
            {
                ErrorText.Text = "Unesite korisničko ime i lozinku.";
                return;
            }

            var token = await _apiService.LoginAsync(username, password);

            if (token != null)
            {
                MainWindow mainWindow = new MainWindow(_apiService, UsernameTextBox.Text);
                mainWindow.Show();

                Close();
            }
            else
            {
                ErrorText.Text = "Pogrešno korisničko ime ili lozinka.";
            }
        }
    }
}