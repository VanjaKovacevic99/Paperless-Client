using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace HCI_2025_Project_Template.Models
{
    internal class Document
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public DateTime Created { get; set; }
        public List<Tag> Tags { get; set; } = new List<Tag>();
        public Correspondent? Correspondent { get; set; }
        public DocumentType? DocumentType { get; set; }
    }
}
