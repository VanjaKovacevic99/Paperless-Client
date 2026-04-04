using HCI_2025_Project.Services;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Windows;

namespace HCI_2025_Project_Template
{
    
    public partial class MainWindow : Window
    {
       

        public MainWindow()
        {
            InitializeComponent();
           

        }


        private async void TestButton_Click(object sender, RoutedEventArgs e)
        {
            var apiService = new PaperlessApiService();
            var data = await apiService.GetDocumentsAsync();
            MessageBox.Show(data.Substring(0, 500));
        }
       
    }
}
