using HCI_2025_Project.Services;
using HCI_2025_Project_Template.Views;
using System.Windows;

namespace HCI_2025_Project_Template
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var apiService = new PaperlessApiService();

            var loginWindow = new LoginWindow(apiService);
            loginWindow.Show();
        }

    }

}
