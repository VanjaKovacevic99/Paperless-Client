using System.Text.Json.Serialization;
using System.Windows.Media.Imaging;

namespace HCI_2025_Project_Template.Models
{
    public class Document
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("created")]
        public DateTime Created { get; set; }
        public string CreatedDisplay => Created.ToString("dd.MM.yyyy");

        [JsonPropertyName("tags")]
        public List<int> Tags { get; set; } = new List<int>();

        public List<string> TagNames { get; set; } = new List<string>();
        public string TagNamesDisplay => string.Join(", ", TagNames);

        [JsonPropertyName("correspondent")]
        public int? Correspondent { get; set; }

        [JsonPropertyName("document_type")]
        public int? DocumentType { get; set; }

        public string? DocumentTypeName { get; set; }

        [JsonIgnore]
        public BitmapImage? Thumbnail { get; set; }

        [JsonIgnore]
        public string? FilePath { get; set; }

        [JsonIgnore]
        public string? FileName =>
            string.IsNullOrWhiteSpace(FilePath)
                ? null
                : System.IO.Path.GetFileName(FilePath);
    }
}
