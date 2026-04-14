using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml.Linq;

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
    }
}
