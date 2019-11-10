using System;
using System.Collections.Generic;

namespace zorgapp.Models
{

    public class Practice{
        public int PracticeId { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public List<int> DoctorIds{ get; set; }
        public List<int> PatientIds{ get; set; }
        public List<int> PatientPracticeIds{ get; set; }
    }
}
