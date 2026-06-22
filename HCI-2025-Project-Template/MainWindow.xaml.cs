
using HCI_2025_Project_Template.Models;
using HCI_2025_Project_Template.ViewModels;
using HCI_2025_Project_Template.Views;
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
        public MainWindow()
        {
            InitializeComponent();
            _viewModel = new DocumentsViewModel();
            this.DataContext = _viewModel;
            SetActive(BtnDocuments);




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

        private async void AddTag_Click(object sender, RoutedEventArgs e)
        {
            var vm = (DocumentsViewModel)DataContext;
            await vm.CreateTagAsync();
        }




        private async void DeleteTag_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var tag = button?.DataContext as Tag;  // direktno uzmi tag iz dugmeta

            if (tag == null) return;

            var vm = (DocumentsViewModel)DataContext;
            await vm.DeleteTagAsync(tag.Id);  // proslijedi ID
        }

        private async void EditTag_Click(object sender, RoutedEventArgs args)
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
        }
        private async void UpdateTag_Click(object sender, RoutedEventArgs args)
        {
            var vm = (DocumentsViewModel)DataContext;

            await vm.UpdateTagAsync();
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
    }
}
