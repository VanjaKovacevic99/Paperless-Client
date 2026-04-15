using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HCI_2025_Project_Template.Models
{
    public class ApiResponse<T>
    {
        public int count { get; set; }
        public string? next { get; set; }
        public string? previous { get; set; }
        public List<T>? results { get; set; }
    }
}
