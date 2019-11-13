using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace zorgapp.Models
{
    public class Patient{

        public int PatientId { get; set; }
        public string Password { get; set;}      
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int PhoneNumber { get; set; }
        public string Email {get; set; }
        public List<string> Messages { get; set; }
        // public List<int> DoctorIds{ get; set; }
        // public List<int> PracticeIds{ get; set; }
        // // public Doctor Doctor {get; set;}

        // public List<int> PatientPracticeIds{ get; set; }
        // public ICollection <Doctor> Doctors {get; set;}
        // public int DoctorId {get; set;}
        // public ICollection <PatientsDoctors> PatientsDoctorss {get; set;}

        public Doctor Doctor {get;set;}
        public ICollection<PatientsDoctors> PatientsDoctorss {get;set;}



        
    }

}
