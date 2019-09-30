using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace zorgapp.Models
{
    public class Document
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public int PatientID { get; set; }
    }
}
