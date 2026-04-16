using CommunityToolkit.Mvvm.DependencyInjection;
using HCI_2025_Project.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Document = HCI_2025_Project_Template.Models.Document;
using IOPath = System.IO.Path;

namespace HCI_2025_Project_Template.Views
{
    /// <summary>
    /// Interaction logic for DocumentPreviewWindow.xaml
    /// </summary>
    public partial class DocumentPreviewWindow : Window
    {
        private readonly PaperlessApiService _apiService= new PaperlessApiService();
        private readonly Document _doc;

        public DocumentPreviewWindow(Document doc)
        {
            InitializeComponent();
            DataContext = doc;
            _doc = doc;
            LoadPdf(doc.Id);
        }

        private async void LoadPdf(int id)
        {
            try
            {
                var bytes = await _apiService.DownloadDocumentAsync(_doc.Id);

                string file = System.IO.Path.Combine(
                    System.IO.Path.GetTempPath(),
                    $"doc_{_doc.Id}.pdf");

                File.WriteAllBytes(file, bytes);

                PdfView.Source = new Uri(file);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cannot load preview: " + ex.Message);
            }

        }
    }
}

