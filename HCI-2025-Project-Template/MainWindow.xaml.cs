using HCI_2025_Project.Services;
using HCI_2025_Project_Template.ViewModels;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Windows;

namespace HCI_2025_Project_Template
{
    
    public partial class MainWindow : Window
    {
        private readonly DocumentsViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();
            _viewModel = new DocumentsViewModel();
            this.DataContext = _viewModel;
           

        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await _viewModel.LoadDocumentsAsync();
            DocumentsGrid.ItemsSource = _viewModel.Documents;
            DocumentsCards.ItemsSource = _viewModel.Documents;
        }

    }
}
