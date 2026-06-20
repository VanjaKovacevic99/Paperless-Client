
using HCI_2025_Project.Services;
using HCI_2025_Project_Template.Models;
using HCI_2025_Project_Template.Services;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using MaterialDesignThemes.Wpf;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;



namespace HCI_2025_Project_Template.ViewModels
{
    public class DocumentsViewModel : INotifyPropertyChanged
    {
        private readonly SettingsService _settingsService = new SettingsService();
        private readonly PaperlessApiService _apiService;
        private string? _next;
        private string? _previous;
        private List<Tag> _tagsCache = new List<Tag>();
        private List<DocumentType> _typesCache = new List<DocumentType>();
        public int _pageSize { get; set; } = 10;

        public int PageSize
        {
            get => _pageSize;
            set
            {
                if (_pageSize != value)
                {
                    _pageSize = value;

                    CurrentPage = 1;
                    OnPropertyChanged();

                    _ = LoadDocumentsAsync();
                }
            }
        }



        public int CurrentPage { get; set; } = 1;

        public string PageInfo =>
            $"{(CurrentPage - 1) * PageSize + 1} - {CurrentPage * PageSize}";
        public bool HasNextPage => _next != null;
        public bool HasPreviousPage => _previous != null;

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText != value)
                {
                    if (value?.Length > 100)
                    {
                        MessageBox.Show((string)Application.Current.Resources["SearchTooLong"]);
                        return;
                    }
                    _searchText = value;
                    CurrentPage = 1;
                    _ = LoadDocumentsAsync();
                    OnPropertyChanged();
                }
            }
        }
        public ObservableCollection<Tag> Tags { get; set; } = new();
        public ObservableCollection<Tag> Tags1 { get; set; } = new();
        public ObservableCollection<DocumentType> DocumentTypes { get; set; } = new();
        public ObservableCollection<Correspondent> Correspondents { get; set; } = new();

        private Tag _selectedTag;
        public Tag SelectedTag
        {
            get => _selectedTag;
            set
            {
                if (_selectedTag != value)
                {
                    _selectedTag = value;
                    CurrentPage = 1;
                    _ = LoadDocumentsAsync();
                    OnPropertyChanged();
                }
            }
        }
        private DocumentType _selectedType;
        public DocumentType SelectedType
        {
            get => _selectedType;
            set
            {
                if (_selectedType != value)
                {
                    _selectedType = value;
                    CurrentPage = 1;
                    _ = LoadDocumentsAsync();
                    OnPropertyChanged();
                }
            }
        }

        private Correspondent _selectedCorrespondent;
        public Correspondent SelectedCorrespondent
        {
            get => _selectedCorrespondent;
            set
            {
                if (_selectedCorrespondent != value)
                {
                    _selectedCorrespondent = value;
                    CurrentPage = 1;
                    _ = LoadDocumentsAsync();
                    OnPropertyChanged();
                }
            }
        }
        public bool IsDocuments => CurrentView == AppView.Documents;
        public bool IsUpload => CurrentView == AppView.Upload;
        public bool IsTags => CurrentView == AppView.Tags;
        public bool IsSettings => CurrentView == AppView.Settings;

        public bool IsAnalytics => CurrentView == AppView.Analytics;

        private AppView _currentView = AppView.Documents;

        public AppView CurrentView
        {
            get => _currentView;
            set
            {
                _currentView = value;

                OnPropertyChanged();
                OnPropertyChanged(nameof(IsDocuments));
                OnPropertyChanged(nameof(IsUpload));
                OnPropertyChanged(nameof(IsTags));
                OnPropertyChanged(nameof(IsSettings));
                OnPropertyChanged(nameof(IsAnalytics));
            }
        }
        public async void Navigate(AppView view)
        {
            CurrentView = view;

        }

        private string _uploadFileName;
        public string UploadFileName
        {
            get => _uploadFileName;
            set
            {
                _uploadFileName = value;
                OnPropertyChanged(nameof(UploadFileName));
            }
        }



        private UploadDocumentModel _upload = new UploadDocumentModel();
        public UploadDocumentModel Upload
        {
            get => _upload;
            set
            {
                _upload = value;
                OnPropertyChanged();
            }
        }
        public DocumentsViewModel()
        {
            _apiService = new PaperlessApiService();
            Documents = new ObservableCollection<Document>();

            XAxes = new Axis[]
{
        new Axis
        {
            Labels = MonthLabels
        }
                };

            YAxes = new Axis[]
            {
    new Axis()
            };
        }

        public ObservableCollection<Document> Documents { get; }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));


        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }
        public async Task LoadDocumentsAsync()
        {
            IsLoading = true;
            Documents.Clear();

            int? tagId = (SelectedTag != null && SelectedTag.Id != 0)
    ? SelectedTag.Id
    : null;

            int? typeId = (SelectedType != null && SelectedType.Id != 0)
                ? SelectedType.Id
                : null;

            int? correspondentId = (SelectedCorrespondent != null && SelectedCorrespondent.Id != 0)
                ? SelectedCorrespondent.Id
                : null;

            try
            {
                var response = await _apiService.GetDocumentsAsync(
                    CurrentPage,
                    PageSize,
                    SearchText,
                    tagId,
                    typeId,
                    correspondentId
                );


                _next = response.next;
                _previous = response.previous;

                var documents = response.results;
                if (response?.results == null)
                    return;

                // load once
                if (_tagsCache.Count == 0)
                    _tagsCache = await _apiService.GetTagsAsync();

                if (_typesCache.Count == 0)
                    _typesCache = await _apiService.GetDocumentTypesAsync();

                var tagsLookup = _tagsCache.ToDictionary(t => t.Id, t => t.Name);
                var typesLookup = _typesCache.ToDictionary(t => t.Id, t => t.Name);

                foreach (var doc in documents)
                {
                    doc.TagNames = doc.Tags
                        .Select(id => tagsLookup.TryGetValue(id, out var name) ? name : " ")
                        .ToList();

                    doc.DocumentTypeName =
                        doc.DocumentType.HasValue &&
                        typesLookup.TryGetValue(doc.DocumentType.Value, out var typeName)
                        ? typeName
                        : " ";

                    Documents.Add(doc);

                }

                OnPropertyChanged(nameof(HasNextPage));
                OnPropertyChanged(nameof(HasPreviousPage));
                OnPropertyChanged(nameof(PageInfo));
            }
            catch
            {
                MessageBox.Show((string)Application.Current.Resources["NetworkError"]);
                return;
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task NextPageAsync()
        {
            if (HasNextPage)
            {
                CurrentPage++;
                IsLoading = true;
                try
                {
                    await LoadDocumentsAsync();
                }

                catch
                {
                    MessageBox.Show((string)Application.Current.Resources["NetworkError"]);
                    return;
                }
                finally
                {
                    IsLoading = false;

                }
            }
            OnPropertyChanged(nameof(PageInfo));
        }

        public async Task PreviousPageAsync()
        {
            if (HasPreviousPage && CurrentPage > 1)
            {
                CurrentPage--;
                IsLoading = true;
                try
                {
                    await LoadDocumentsAsync();

                }
                catch
                {
                    MessageBox.Show((string)Application.Current.Resources["NetworkError"]);
                    return;
                }
                finally
                {
                    IsLoading = false;
                }
            }
            OnPropertyChanged(nameof(PageInfo));
        }

        public async Task InitializeAsync()
        {
            IsLoading = true;
            try
            {
                Settings = _settingsService.Load();
                ApplyTheme();
                SetLanguage(Settings.Language);
                Tags = new ObservableCollection<Tag>(await _apiService.GetTagsAsync());
                Tags.Insert(0, new Tag { Id = 0, Name = "All tags" });
                OnPropertyChanged(nameof(Tags));

                DocumentTypes = new ObservableCollection<DocumentType>(await _apiService.GetDocumentTypesAsync());
                DocumentTypes.Insert(0, new DocumentType { Id = 0, Name = "All types" });

                OnPropertyChanged(nameof(DocumentTypes));

                Correspondents = new ObservableCollection<Correspondent>(await _apiService.GetCorrespondentsAsync());
                Correspondents.Insert(0, new Correspondent { Id = 0, Name = "All correspondents" });
                OnPropertyChanged(nameof(Correspondents));

                CurrentPage = 1;

                await LoadTagsAsync();
                await LoadDocumentsAsync();




            }
            catch
            {
                MessageBox.Show((string)Application.Current.Resources["NetworkError"]);
                return;
            }
            finally
            {
                IsLoading = false;

            }
        }

        public async Task LoadTagsAsync()
        {
            IsLoading = true;
            try
            {
                var tags = await _apiService.GetTagsAsync();

                Tags1 = new ObservableCollection<Tag>(tags);

                OnPropertyChanged(nameof(Tags1));
            }
            catch
            {
                MessageBox.Show((string)Application.Current.Resources["NetworkError"]);
                return;
            }
            finally
            {
                IsLoading = false;
            }
        }
        public async Task UploadAsync()
        {
            IsLoading = true;
            if (string.IsNullOrWhiteSpace(Upload.Title))
            {
                MessageBox.Show((string)Application.Current.Resources["TitleRequired"]);
                return;
            }

            if (Upload.FilePath == null)
            {
                MessageBox.Show((string)Application.Current.Resources["FileRequired"]);
                return;
            }

            if (!System.IO.File.Exists(Upload.FilePath))
            {
                MessageBox.Show((string)Application.Current.Resources["FileNotFound"]);
                return;
            }
            try
            {
                var success = await _apiService.UploadDocumentAsync(
                    Upload.FilePath,
                    Upload.Title,
                    Upload.Tag?.Id,
                    Upload.Type?.Id,
                    Upload.Correspondent?.Id
                );

                if (success)
                {
                    await LoadDocumentsAsync();
                }
                else
                {
                    MessageBox.Show((string)Application.Current.Resources["UploadFailed"]);
                }


            }
            catch
            {
                MessageBox.Show((string)Application.Current.Resources["NetworkError"]);
                return;
            }
            finally
            {
                IsLoading = false;
            }
        }


        public async Task DeleteTagAsync(int id)
        {
            IsLoading = true;
            try
            {
                var success = await _apiService.DeleteTagAsync(id);

                if (success)
                {
                    await LoadTagsAsync();
                }
                else
                {
                    MessageBox.Show((string)Application.Current.Resources["TagDeleteFailed"]);
                }
            }
            catch
            {
                MessageBox.Show((string)Application.Current.Resources["NetworkError"]);
                return;
            }
            finally { IsLoading = false; }
        }

        private string _newTagName;
        public string NewTagName
        {
            get => _newTagName;
            set { _newTagName = value; OnPropertyChanged(); }
        }

        private Color _newTagColor = Colors.LightGray;

        public Color NewTagColor
        {
            get => _newTagColor;
            set
            {
                _newTagColor = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(NewTagColorHex));
            }
        }
        public string NewTagColorHex =>
    $"#{NewTagColor.R:X2}{NewTagColor.G:X2}{NewTagColor.B:X2}";
        public async Task CreateTagAsync()
        {
            if (string.IsNullOrWhiteSpace(NewTagName))
            {
                MessageBox.Show(
                    (string)Application.Current.Resources["TagNameRequired"]);
                return;
            }
            if (NewTagName.Length < 2)
            {
                MessageBox.Show((string)Application.Current.Resources["TagNameTooShort"]);
                return;
            }

            if (NewTagName.Length > 30)
            {
                MessageBox.Show((string)Application.Current.Resources["TagNameTooLong"]);
                return;
            }
            IsLoading = true;

            try
            {
                bool success = await _apiService.CreateTagAsync(NewTagName, NewTagColorHex);
                if (success)
                {
                    await LoadTagsAsync();
                    NewTagName = "";
                    NewTagColor = Colors.LightGray;
                }
                else
                {
                    MessageBox.Show(
                         (string)Application.Current.Resources["TagCreatedError"]);
                }
            }
            catch
            {
                MessageBox.Show((string)Application.Current.Resources["NetworkError"]);
                return;
            }
            finally
            {
                IsLoading = false;
            }
        }



        private bool _hasSelectedTag;
        public bool HasSelectedTag
        {
            get => _hasSelectedTag;
            set
            {
                _hasSelectedTag = value;
                OnPropertyChanged();
            }
        }

        private Tag _crudTag;
        public Tag CrudTag
        {
            get => _crudTag;
            set
            {
                _crudTag = value;
                OnPropertyChanged();
                HasSelectedTag = _crudTag != null;
            }
        }
        private string NormalizeColor(string color)
        {
            Color c = (Color)ColorConverter.ConvertFromString(color);
            return $"#{c.R:X2}{c.G:X2}{c.B:X2}";
        }

        public async Task UpdateTagAsync()
        {
            if (CrudTag == null)
            {
                MessageBox.Show((string)Application.Current.Resources["NoTagSelected"]);
                return;
            }

            if (string.IsNullOrWhiteSpace(CrudTag.Name))
            {
                MessageBox.Show((string)Application.Current.Resources["TagNameRequired"]);
                return;
            }

            if (CrudTag.Name.Length < 2)
            {
                MessageBox.Show((string)Application.Current.Resources["TagNameTooShort"]);
                return;
            }
            if (NewTagName.Length > 30)
            {
                MessageBox.Show((string)Application.Current.Resources["TagNameTooLong"]);
                return;
            }
            string hexColor = NormalizeColor(CrudTag.Color);


            MessageBox.Show(hexColor);
            IsLoading = true;
            try
            {
                var success = await _apiService.UpdateTagAsync(
                    CrudTag.Id,
                    CrudTag.Name,
                    hexColor
                );

                MessageBox.Show(success ? (string)Application.Current.Resources["TagSussecUpdate"] : (string)Application.Current.Resources["TagFaliedUpdate"]);

                if (success)
                {
                    await LoadTagsAsync();
                    CrudTag = null;
                    HasSelectedTag = false;
                }
            }
            catch
            {
                MessageBox.Show((string)Application.Current.Resources["NetworkError"]);
                return;
            }
            finally
            {
                IsLoading = false;
            }
        }
        private AppSettings _settings;

        public AppSettings Settings
        {
            get => _settings;
            set
            {
                _settings = value;
                OnPropertyChanged();
            }
        }
        public void ApplyTheme()
        {
            var paletteHelper = new PaletteHelper();
            var theme = paletteHelper.GetTheme();

            if (Settings.Theme == "Dark")
                theme.SetBaseTheme(BaseTheme.Dark);
            else
                theme.SetBaseTheme(BaseTheme.Light);

            if (!string.IsNullOrWhiteSpace(Settings.PrimaryColor))
            {
                var color = (Color)ColorConverter.ConvertFromString(Settings.PrimaryColor);
                theme.SetPrimaryColor(color);
            }

            paletteHelper.SetTheme(theme);
        }

        public void SaveSettings()
        {
            _settingsService.Save(Settings);
        }



        private Color _selectedColor;

        public Color SelectedColor
        {
            get => _selectedColor;
            set
            {
                _selectedColor = value;
                OnPropertyChanged();

                Settings.PrimaryColor = value.ToString();
            }
        }



        public void SetLanguage(string lang)
        {
            var dict = new ResourceDictionary();

            dict.Source = new Uri(
                $"Resources/Strings.{lang}.xaml",
                UriKind.Relative);

            var dictionaries = Application.Current.Resources.MergedDictionaries;

            var oldDict = dictionaries.FirstOrDefault(d =>
                d.Source != null &&
                d.Source.OriginalString.Contains("Strings"));

            if (oldDict != null)
                dictionaries.Remove(oldDict);

            dictionaries.Add(dict);

            Settings.Language = lang;
        }

        private ISeries[] _pieSeries = Array.Empty<ISeries>();

        public ISeries[] PieSeries
        {
            get => _pieSeries;
            set
            {
                _pieSeries = value;
                OnPropertyChanged();
            }
        }



        private List<Document> _allDocuments = new();
        private bool _analyticsLoaded = false;

        public async Task LoadAnalyticsAsync()
        {
            if (_analyticsLoaded)
            {
                BuildPieChartFromCache();
                return;
            }

            IsLoading = true;

            try
            {
                var types = await _apiService.GetDocumentTypesAsync();
                var typeMap = types.ToDictionary(x => x.Id, x => x.Name);

                _allDocuments = await _apiService.GetAllDocumentsAsync();

                foreach (var d in _allDocuments)
                {
                    d.DocumentTypeName =
                        d.DocumentType.HasValue && typeMap.TryGetValue(d.DocumentType.Value, out var name)
                        ? name
                        : "Unknown";
                }
                Years = new ObservableCollection<int>(
               _allDocuments
            .Select(d => d.Created.Year)
            .Distinct()
            .OrderByDescending(x => x)
           );

                OnPropertyChanged(nameof(Years));

                SelectedYear = Years.FirstOrDefault(); // auto izabere prvu

                _analyticsLoaded = true;

                BuildPieChartFromCache();
                BuildBarChart();
                LoadTagCloud();
            }
            finally
            {
                IsLoading = false;
            }
        }
        public void BuildPieChartFromCache()
        {
            var stats = _allDocuments
                .GroupBy(d => d.DocumentTypeName ?? "Unknown")
                .ToDictionary(g => g.Key, g => g.Count());

            var total = stats.Values.Sum();

            PieSeries = stats.OrderByDescending(k => k.Value).Select(x =>
            {
                var percent = (double)x.Value / total * 100;

                return new PieSeries<double>
                {
                    Values = new[] { (double)x.Value },
                    Name = $"{x.Key} ({percent:0.0}%)"
                };
            }).ToArray();
        }

        //bar chart

        public ObservableCollection<int> Years { get; set; } = new();

        private int _selectedYear;
        public int SelectedYear
        {
            get => _selectedYear;
            set
            {
                _selectedYear = value;
                OnPropertyChanged();
                BuildBarChart(); // svaki put kad promijeniš godinu
            }


        }
        public string[] MonthLabels { get; set; } =
[
             "Jan", "Feb", "Mar", "Apr", "May", "Jun",
             "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"
            ];

        private ISeries[] _barSeries;
        public ISeries[] BarSeries
        {
            get => _barSeries;
            set
            {
                _barSeries = value;
                OnPropertyChanged();
            }
        }

        public void BuildBarChart()
        {
            var filtered = _allDocuments
                .Where(d => d.Created.Year == SelectedYear)
                .GroupBy(d => d.Created.Month)
                .ToDictionary(g => g.Key, g => g.Count());

            var values = new int[12];

            for (int i = 1; i <= 12; i++)
            {
                values[i - 1] = filtered.ContainsKey(i) ? filtered[i] : 0;
            }

            BarSeries = new ISeries[]
            {
        new ColumnSeries<int>
        {
            Values = values

        }
            };
        }


        private Axis[] _xAxes;
        public Axis[] XAxes
        {
            get => _xAxes;
            set
            {
                _xAxes = value;

                OnPropertyChanged();
            }
        }

        private Axis[] _yAxes;
        public Axis[] YAxes
        {
            get => _yAxes;
            set
            {
                _yAxes = value;
                OnPropertyChanged();
            }
        }

        public List<KeyValuePair<string, int>> _tagCloud = new();
        public void LoadTagCloud()
        {
            Random rnd = new Random();
            _tagCloud = _allDocuments
       .GroupBy(d => d.DocumentTypeName)
       .Select(g => new KeyValuePair<string, int>(g.Key, g.Count()))
       .OrderByDescending(x => x.Value)
       .ToList();



        }

    }
}