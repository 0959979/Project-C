using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;


namespace zorgapp.Models

{
    public class Doctor
    {
        public int DoctorId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public int PhoneNumber { get; set; }
        public string Specialism { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public List<string> Messages { get; set; }
        // public List<int> PatientIds { get; set; }
        // public List<int> PracticeIds{ get; set; }
        // public Patient Patient {get; set;}

        // public ICollection<Patient> Patients {get; set;}
        // public int PatientId {get; set;}
        // public ICollection <PatientsDoctors> PatientsDoctorss {get; set;}

        public ICollection<PatientsDoctors> PatientsDoctorss {get;set;}
    }
}