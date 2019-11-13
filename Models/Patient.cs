using System;
using System.Collections.Generic;

namespace zorgapp.Models
{
    public class Patient{

        public int PatientId { get; set; }
        public string LocalId { get; set; }
        public string Password { get; set; }
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public int CanSeeMeId { get; set; }
        public int ICanSeeId { get; set; }
        public string LinkCode { get; set; }
        public string LinkUses { get; set; }
        public Doctor Doctor { get; set; }
        public ICollection<PatientsDoctors> PatientsDoctorss { get; set; }

    }

}
