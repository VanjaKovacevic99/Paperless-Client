using HCI_2025_Project.Services;
using HCI_2025_Project_Template.ViewModels;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Windows;
using System.Windows.Controls;

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

            await vm.LoadDocumentsAsync();
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

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
