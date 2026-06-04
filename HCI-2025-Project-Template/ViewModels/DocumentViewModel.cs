using HCI_2025_Project.Services;
using HCI_2025_Project_Template.Models;
using HCI_2025_Project_Template.Services;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
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
            }
        }
        public void Navigate(AppView view)
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
        }

        public ObservableCollection<Document> Documents { get; }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public async Task LoadDocumentsAsync()
        {
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

        public async Task NextPageAsync()
        {
            if (HasNextPage)
            {
                CurrentPage++;
                await LoadDocumentsAsync();
            }
            OnPropertyChanged(nameof(PageInfo));
        }

        public async Task PreviousPageAsync()
        {
            if (HasPreviousPage && CurrentPage > 1)
            {
                CurrentPage--;
                await LoadDocumentsAsync();
            }
            OnPropertyChanged(nameof(PageInfo));
        }

        public async Task InitializeAsync()
        {
            Settings = _settingsService.Load();
            ApplyTheme();
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

        public async Task LoadTagsAsync()
        {
            var tags = await _apiService.GetTagsAsync();

            Tags1 = new ObservableCollection<Tag>(tags);

            OnPropertyChanged(nameof(Tags1));
        }
        public async Task UploadAsync()
        {
            var success = await _apiService.UploadDocumentAsync(
                Upload.FilePath,
                Upload.Title,
                Upload.Tag?.Id,
                Upload.Type?.Id,
                Upload.Correspondent?.Id
            );

            if (success)
                await LoadDocumentsAsync();
        }


        public async Task DeleteTagAsync(int id)
        {


            var success = await _apiService.DeleteTagAsync(id);

            if (success)
                await LoadTagsAsync();
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
                MessageBox.Show("Unesite naziv taga.");
                return;
            }


            MessageBox.Show("aa" + NewTagColorHex);



            bool success = await _apiService.CreateTagAsync(NewTagName, NewTagColorHex);
            if (success)
            {
                await LoadTagsAsync();
                NewTagName = "";
                NewTagColor = Colors.LightGray;
            }
            else
            {
                MessageBox.Show("Greška pri kreiranju taga.");
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
                return;

            string hexColor = NormalizeColor(CrudTag.Color);


            MessageBox.Show(hexColor);

            var success = await _apiService.UpdateTagAsync(
                CrudTag.Id,
                CrudTag.Name,
                hexColor
            );

            MessageBox.Show(success ? "UPDATED OK" : "UPDATE FAILED");

            if (success)
            {
                await LoadTagsAsync();
                CrudTag = null;
                HasSelectedTag = false;
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
    } 
}