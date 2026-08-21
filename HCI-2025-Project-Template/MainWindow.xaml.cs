
using HCI_2025_Project.Services;
using HCI_2025_Project_Template.Models;
using HCI_2025_Project_Template.ViewModels;
using HCI_2025_Project_Template.Views;
using LiveChartsCore.Kernel;
using LiveChartsCore.Kernel.Sketches;
using MaterialDesignThemes.Wpf;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace HCI_2025_Project_Template
{

    public partial class MainWindow : Window
    {
        private readonly DocumentsViewModel _viewModel;
        private bool _tagCloudReady = false;
        private readonly PaperlessApiService _apiService;
        public MainWindow(PaperlessApiService apiService, string username)
        {
            InitializeComponent();

            _apiService = apiService;
            _viewModel = new DocumentsViewModel(_apiService, username);
            DataContext = _viewModel;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var vm = (DocumentsViewModel)DataContext;

            await vm.InitializeAsync();
        }

        private async void Next_Click(object sender, RoutedEventArgs e)
        {
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

            vm.ResetFilters();
        }

        private void DocumentsGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var grid = sender as DataGrid;

            if (grid?.SelectedItem is Document selectedDocument)
            {
                var previewWindow = new DocumentPreviewWindow(selectedDocument, _apiService);
                previewWindow.ShowDialog();
            }
        }

        private void Card_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var card = sender as FrameworkElement;
            if (card == null) return;

            var doc = card.DataContext as Document;
            if (doc == null) return;

            var preview = new DocumentPreviewWindow(doc, _apiService);
            preview.Show();
        }

        private void Documents_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.Navigate(AppView.Documents);
            _viewModel.ResetFilters();
            SetActive(BtnDocuments);
        }

        private void Upload_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.Navigate(AppView.Upload);
            SetActive(BtnUpload);
        }

        private void Tags_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.Navigate(AppView.Tags);
            SetActive(BtnTags);
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.Navigate(AppView.Settings);
            SetActive(BtnSettings);
        }
        private async void Analytics_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.Navigate(AppView.Analytics);
            SetActive(BtnAnalytics);
            await _viewModel.LoadAnalyticsAsync();


            DrawTagCloud(TagPanel, _viewModel._tagCloud, 12, 30, 5);


        }

        public void DrawTagCloud(Canvas canvas, List<KeyValuePair<string, int>> tags,
                           double minFontSize = 12, double maxFontSize = 28,
                           double spacing = 4.0)
        {
            if (canvas == null || tags == null || tags.Count == 0)
                return;

            canvas.Children.Clear();

            double widthGrid = canvas.ActualWidth;
            double heightGrid = canvas.ActualHeight;

            double centerX = widthGrid / 2;
            double centerY = heightGrid / 2;

            if (centerX <= 0 || centerY <= 0)
            {
                centerX = 400;
                centerY = 100;
            }

            var cleanTags = tags
                .Where(t => !string.IsNullOrWhiteSpace(t.Key) &&
                            !t.Key.Equals("Unknown", StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(t => t.Value)
                .ToList();

            if (cleanTags.Count == 0)
                return;

            int maxWeight = cleanTags.Max(t => t.Value);
            int minWeight = cleanTags.Min(t => t.Value);

            List<Rect> occupiedRects = new List<Rect>();

            int index = 0;
            int total = cleanTags.Count;

            foreach (var tag in cleanTags)
            {

                double fontSize;

                double ratio = (double)index / total;

                if (ratio < 0.33)
                    fontSize = maxFontSize;                         // najvažniji
                else if (ratio < 0.66)
                    fontSize = (minFontSize + maxFontSize) / 2;     // srednji
                else
                    fontSize = minFontSize;                        // najmanji


                SolidColorBrush brush;

                if (fontSize == maxFontSize)
                    brush = new SolidColorBrush(Colors.DarkBlue);
                else if (fontSize == (minFontSize + maxFontSize) / 2)
                    brush = new SolidColorBrush(Colors.MediumPurple);
                else
                    brush = new SolidColorBrush(Colors.Gray);

                TextBlock tb = new TextBlock
                {
                    Text = tag.Key,
                    FontSize = fontSize,
                    FontWeight = FontWeights.Bold,
                    Foreground = brush
                };
                tb.Cursor = Cursors.Hand;
                tb.MouseLeftButtonUp += TagCloudItem_Click;
                tb.Tag = tag.Key;

                tb.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

                double width = tb.DesiredSize.Width + spacing;
                double height = tb.DesiredSize.Height + spacing;

                double angle = 0;
                double radius = 0;

                const double angleStep = 0.25;
                const double radiusStep = 0.8;
                const int maxAttempts = 3000;

                bool placed = false;
                int attempts = 0;

                while (!placed && attempts < maxAttempts)
                {
                    double x = centerX + radius * Math.Cos(angle) - width / 2;
                    double y = centerY + radius * Math.Sin(angle) - height / 2;

                    Rect candidate = new Rect(x, y, width, height);

                    bool overlaps = false;

                    foreach (var occupied in occupiedRects)
                    {
                        if (candidate.IntersectsWith(occupied))
                        {
                            overlaps = true;
                            break;
                        }
                    }

                    if (!overlaps)
                    {
                        occupiedRects.Add(candidate);

                        Canvas.SetLeft(tb, x);
                        Canvas.SetTop(tb, y);
                        canvas.Children.Add(tb);

                        placed = true;
                    }
                    else
                    {
                        angle += angleStep;
                        radius += radiusStep / (1 + angle * 0.01);
                        attempts++;
                    }
                }

                if (!placed)
                {
                    double fallbackX = centerX + 50 * Math.Cos(attempts * 0.5);
                    double fallbackY = centerY + 50 * Math.Sin(attempts * 0.5);

                    Canvas.SetLeft(tb, fallbackX - width / 2);
                    Canvas.SetTop(tb, fallbackY - height / 2);
                    canvas.Children.Add(tb);
                }

                index++;
            }
        }

        private void TagCloudContainer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            DrawTagCloud(TagPanel, _viewModel._tagCloud);
        }

        private async void TagCloudItem_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is TextBlock tb)
            {
                string tagName = tb.Tag?.ToString();

                var vm = (DocumentsViewModel)DataContext;

                var selectedTag = vm.Tags.FirstOrDefault(t => t.Name == tagName);

                if (selectedTag != null)
                {
                    vm.SelectedTag = selectedTag;

                    vm.CurrentPage = 1;

                }
            }
        }


        private void PieChart_DataPointerDown(IChartView chart, IEnumerable<ChartPoint> points)
        {
            if (points == null || !points.Any()) return;

            var vm = (DocumentsViewModel)DataContext;
            var point = points.First();
            if (point == null) return;


            string seriesName = point.Context.Series.Name;
            if (string.IsNullOrEmpty(seriesName)) return;


            int idx = seriesName.IndexOf('(');
            string key = idx > 0 ? seriesName.Substring(0, idx).Trim() : seriesName;


            if (vm.PieMap != null && vm.PieMap.TryGetValue(key, out var selected))
            {
                vm.SelectedType = selected;
                vm.CurrentPage = 1;
                _ = vm.LoadDocumentsAsync();
            }
            else
            {

                selected = vm.DocumentTypes.FirstOrDefault(t => t.Name == key);
                if (selected != null)
                {
                    vm.SelectedType = selected;
                    vm.CurrentPage = 1;
                    _ = vm.LoadDocumentsAsync();
                }
                else
                {
                    MessageBox.Show($"Nije pronađen tip: {key}");
                }
            }
        }
        private void BarChart_DataPointerDown(LiveChartsCore.Kernel.Sketches.IChartView chart, System.Collections.Generic.IEnumerable<LiveChartsCore.Kernel.ChartPoint> points)
        {
            if (points == null || !points.Any()) return;

            var point = points.FirstOrDefault();
            if (point == null) return;

            // Dohvati indeks stupca (0 = Jan, 1 = Feb, ...)
            int monthIndex = (int)point.Index;
            int month = monthIndex + 1; // 1 = Jan, 2 = Feb, ...

            var vm = (DocumentsViewModel)DataContext;
            vm.OnBarClicked(month);
        }

        private void ChooseFile_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Filter = "All files (*.*)|*.*";

            if (dialog.ShowDialog() == true)
            {
                var vm = (DocumentsViewModel)DataContext;
                vm.Upload.FilePath = dialog.FileName;
                vm.UploadFileName = System.IO.Path.GetFileName(dialog.FileName);
            }
        }

        private void File_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                var vm = (DocumentsViewModel)DataContext;
                vm.Upload.FilePath = files[0];
                vm.UploadFileName = System.IO.Path.GetFileName(files[0]);


            }
        }

        private async void UploadFile_Click(object sender, RoutedEventArgs e)
        {
            var vm = (DocumentsViewModel)DataContext;

            if (string.IsNullOrWhiteSpace(vm.Upload.FilePath))
            {
                MessageBox.Show((string)Application.Current.Resources["SelectFile"]);
                return;
            }


            if (string.IsNullOrWhiteSpace(vm.Upload.Title))
            {
                MessageBox.Show((string)Application.Current.Resources["EnterDocumentTitle"]);
                return;
            }


            await vm.UploadAsync();
            MessageBox.Show((string)Application.Current.Resources["UploadSussesiful"]);

        }

        private async void SaveTag_Click(object sender, RoutedEventArgs e)
        {
            var vm = (DocumentsViewModel)DataContext;

            if (vm.HasSelectedTag && vm.CrudTag != null)
            {
                await vm.UpdateTagAsync();
            }
            else
            {
                await vm.CreateTagAsync();
            }

            vm.HasSelectedTag = false;
            vm.CrudTag = null;
        }


        private void CancelTagEdit_Click(object sender, RoutedEventArgs e)
        {
            var vm = (DocumentsViewModel)DataContext;

            vm.CrudTag = null;
            vm.HasSelectedTag = false;
        }

        private async void DeleteTag_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var tag = button?.DataContext as Tag;

            if (tag == null) return;

            string message = string.Format(
                (string)Application.Current.Resources["DeleteTagConfirmation"],
                tag.Name);

            string title =
                (string)Application.Current.Resources["DeleteConfirmation"];

            var result = MessageBox.Show(
                message,
                title,
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;

            var vm = (DocumentsViewModel)DataContext;
            await vm.DeleteTagAsync(tag.Id);
        }
        private void EditTag_Click(object sender, RoutedEventArgs args)
        {
            var button = sender as Button;
            var tag = button?.DataContext as Tag;

            if (tag == null) return;

            var vm = (DocumentsViewModel)DataContext;

            vm.CrudTag = new Tag
            {
                Id = tag.Id,
                Name = tag.Name,
                Color = tag.Color
            };

            vm.HasSelectedTag = true;

            // Osvježi formu
            vm.OnTagFormChanged();
        }

        private async void SaveSettings_Click(object sender, RoutedEventArgs e)
        {
            var vm = (DocumentsViewModel)DataContext;
            MessageBox.Show("klik");
            vm.ApplyTheme();
            vm.SetLanguage(vm.Settings.Language);
            vm.SaveSettings(); // snimi u JSON
        }

        private void SetActive(Button active)
        {
            var buttons = new[] { BtnDocuments, BtnUpload, BtnTags, BtnAnalytics, BtnSettings };

            foreach (var b in buttons)
            {
                b.Background = Brushes.Transparent;
                b.BorderBrush = Brushes.Transparent;
            }

            active.Background = new SolidColorBrush(Color.FromArgb(60, 0, 122, 204));
            active.BorderBrush = Brushes.DodgerBlue;
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            // Vrati Material Design temu na default
            var paletteHelper = new PaletteHelper();
            var theme = paletteHelper.GetTheme();

            theme.SetPrimaryColor(Colors.Indigo);

            paletteHelper.SetTheme(theme);

            var loginWindow = new LoginWindow(_apiService);
            loginWindow.Show();

            Close();
        }

        private async void UploadAllDocuments_Click(object sender, RoutedEventArgs e)
        {
            var vm = (DocumentsViewModel)DataContext;

            if (vm.UploadQueue.Count == 0)
            {
                MessageBox.Show(
                (string)Application.Current.Resources["UploadQueueEmpty"],
                "Paperless Client",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

                return;
            }

            var documents = vm.UploadQueue.ToList();

            int total = documents.Count;
            int current = 0;
            int successful = 0;
            int failed = 0;

            vm.IsLoading = true;

            try
            {
                foreach (var document in documents)
                {
                    current++;

                    // Prikaz npr. Upload 1/3
                    vm.UploadProgressText = string.Format(
                    (string)Application.Current.Resources["UploadProgress"],
                     current,
                    total);

                    // Procenat
                    vm.UploadProgress = ((double)current / total) * 100;

                    // Omogući UI-u da prikaže promjenu
                    await Task.Delay(50);

                    try
                    {
                        if (string.IsNullOrWhiteSpace(document.FilePath) ||
                            !File.Exists(document.FilePath))
                        {
                            failed++;
                            continue;
                        }

                        int? tagId = document.Tags.FirstOrDefault();

                        if (tagId == 0)
                            tagId = null;

                        bool success = await _apiService.UploadDocumentAsync(
                            document.FilePath,
                            document.Title,
                            tagId,
                            document.DocumentType,
                            document.Correspondent
                        );

                        if (success)
                        {
                            vm.UploadQueue.Remove(document);
                            successful++;
                        }
                        else
                        {
                            failed++;
                        }
                    }
                    catch
                    {
                        failed++;
                    }
                }

                await vm.LoadDocumentsAsync();
            }
            finally
            {
                vm.IsLoading = false;
                vm.UploadProgressText = "";
                vm.UploadProgress = 0;
            }

            string message = string.Format(
     (string)Application.Current.Resources["UploadResult"],
     successful,
     failed);

            MessageBox.Show(
                message,
                "Paperless Client",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void RemoveDocumentFromQueue_Click(object sender, RoutedEventArgs e)
        {
            var vm = (DocumentsViewModel)DataContext;

            if (sender is Button button &&
                button.DataContext is Document document)
            {
                vm.UploadQueue.Remove(document);
            }
        }
        private void ClearPendingDocuments_Click(object sender, RoutedEventArgs e)
        {
            var vm = (DocumentsViewModel)DataContext;

            if (vm.UploadQueue.Count == 0)
                return;

            var result = MessageBox.Show(
    (string)Application.Current.Resources["ClearUploadQueueConfirmation"],
    "Paperless Client",
    MessageBoxButton.YesNo,
    MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                vm.UploadQueue.Clear();
            }
        }

        private void AddDocumentToQueue_Click(object sender, RoutedEventArgs e)
        {
            var vm = (DocumentsViewModel)DataContext;

            if (string.IsNullOrWhiteSpace(vm.Upload.FilePath))
            {
                MessageBox.Show(
                    (string)FindResource("SelectFile"),
                    "Paperless Client",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            if (string.IsNullOrWhiteSpace(vm.Upload.Title))
            {
                MessageBox.Show(
                    (string)FindResource("TitleRequired"),
                    "Paperless Client",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            if (!File.Exists(vm.Upload.FilePath))
            {
                MessageBox.Show(
                    (string)FindResource("FileNotFound"),
                    "Paperless Client",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            var document = new Document
            {
                Title = vm.Upload.Title,
                FilePath = vm.Upload.FilePath,
                Correspondent = vm.Upload.Correspondent?.Id,
                DocumentType = vm.Upload.Type?.Id
            };

            if (vm.Upload.Tag != null)
            {
                document.Tags.Add(vm.Upload.Tag.Id);
            }

            vm.UploadQueue.Add(document);

            // Očisti formu
            vm.Upload = new UploadDocumentModel();
            vm.UploadFileName = string.Empty;
        }



    }
}
