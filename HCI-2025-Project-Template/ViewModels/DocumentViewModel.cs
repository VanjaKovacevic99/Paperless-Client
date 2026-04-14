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
            var documents = await _apiService.GetDocumentsAsync();
            var tags = await _apiService.GetTagsAsync();
            var types = await _apiService.GetDocumentTypesAsync();

            // Napravi rječnik za brzo mapiranje ID -> naziv
            var tagsLookup = tags.ToDictionary(t => t.Id, t => t.Name);
            var typesLookup = types.ToDictionary(t => t.Id, t => t.Name);


            foreach (var doc in documents)
            {
                // Mapiranje ID-eva na imena
                doc.TagNames = doc.Tags
                                  .Select(id => tagsLookup.ContainsKey(id) ? tagsLookup[id] : " ")
                                  .ToList();

                doc.DocumentTypeName = doc.DocumentType.HasValue &&
                               typesLookup.TryGetValue(doc.DocumentType.Value, out var typeName)
                               ? typeName
                               : " ";

                Documents.Add(doc);
            }
        }
    }
}
