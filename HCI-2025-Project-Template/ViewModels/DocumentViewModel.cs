using HCI_2025_Project.Services;
using HCI_2025_Project_Template.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace HCI_2025_Project_Template.ViewModels
{
    public class DocumentsViewModel : INotifyPropertyChanged
    {
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

        public DocumentsViewModel()
        {
            _apiService = new PaperlessApiService();
            Documents = new ObservableCollection<Document>();
        }

        public ObservableCollection<Document> Documents { get; }

        public event PropertyChangedEventHandler ? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public async Task LoadDocumentsAsync()
        {
            Documents.Clear();

            var response = await _apiService.GetDocumentsAsync(CurrentPage,PageSize,SearchText);

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


    }
}
