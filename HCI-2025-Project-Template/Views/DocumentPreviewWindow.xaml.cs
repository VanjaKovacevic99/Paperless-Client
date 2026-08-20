using HCI_2025_Project.Services;
using System.IO;
using System.Windows;
using Document = HCI_2025_Project_Template.Models.Document;

namespace HCI_2025_Project_Template.Views
{
    /// <summary>
    /// Interaction logic for DocumentPreviewWindow.xaml
    /// </summary>
    public partial class DocumentPreviewWindow : Window
    {
        private readonly PaperlessApiService _apiService;
        private readonly Document _doc;

        public DocumentPreviewWindow(Document doc, PaperlessApiService apiService)
        {
            InitializeComponent();
            DataContext = doc;
            _doc = doc;
            _apiService = apiService;
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

