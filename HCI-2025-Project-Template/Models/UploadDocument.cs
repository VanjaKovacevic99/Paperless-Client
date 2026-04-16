using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HCI_2025_Project_Template.Models
{
    public class UploadDocumentModel
    {
        public string ?Title { get; set; }

        public string? FilePath { get; set; }

        public string? FileName { get; set; }

        public Tag? Tag { get; set; }

        public DocumentType ?Type { get; set; }

        public Correspondent? Correspondent { get; set; }
    }
}
