using HCI_2025_Project.Services;
using HCI_2025_Project_Template.ViewModels;
using HCI_2025_Project_Template.Views;
using HCI_2025_Project_Template.Models;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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
            var vm = (DocumentsViewModel)DataContext;

            await vm.InitializeAsync();
        }

        private async void Next_Click(object sender, RoutedEventArgs e) { 
            var vm = (DocumentsViewModel)DataContext;

            vm.CurrentPage++;
            await vm.LoadDocumentsAsync();
        }

        private async void Prev_Click(object sender, RoutedEventArgs e)
        {
            var vm = (DocumentsViewModel)DataContext;

            if (vm.CurrentPage > 1)
            {
                vm.CurrentPage--;
                await vm.LoadDocumentsAsync();
            }
        }

        private async void PageSizeChanged(object sender, SelectionChangedEventArgs e)
        {
            var vm = DataContext as DocumentsViewModel;
            if (vm == null) return;

            var combo = (ComboBox)sender;
            var selectedItem = (ComboBoxItem)combo.SelectedItem;

            if (int.TryParse(selectedItem.Content.ToString(), out int selectedValue))
            {
                vm.PageSize = selectedValue;
                vm.CurrentPage = 1;

                await vm.LoadDocumentsAsync();
            }
        }

        private void ResetFilters_Click(object sender, RoutedEventArgs e)
        {
            var vm = (DocumentsViewModel)DataContext;

            vm.SelectedTag = vm.Tags[0];
            vm.SelectedType = vm.DocumentTypes[0];
            vm.SelectedCorrespondent = vm.Correspondents[0];
            vm.SearchText = "";
        }

        private void DocumentsGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var grid = sender as DataGrid;

            if (grid?.SelectedItem is Document selectedDocument)
            {
                var previewWindow = new DocumentPreviewWindow(selectedDocument);
                previewWindow.ShowDialog();
            }
        }

        private void Card_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var card = sender as FrameworkElement;
            if (card == null) return;

            var doc = card.DataContext as Document;
            if (doc == null) return;

            var preview = new DocumentPreviewWindow(doc);
            preview.Show();
        }

        private void Documents_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.Navigate(AppView.Documents);
        }

        private void Upload_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.Navigate(AppView.Upload);
        }

        private void Tags_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.Navigate(AppView.Tags);
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.Navigate(AppView.Settings);
        }
    }
}
