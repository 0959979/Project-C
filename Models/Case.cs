using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace zorgapp.Models
{
    public class Case
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public int DoctorId { get; set; }
        public int PatientId { get; set; }
    }
}
