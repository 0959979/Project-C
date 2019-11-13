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
        public int LocalId {get;set;}
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public int PhoneNumber { get; set; }
        public string Specialism { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public ICollection<PatientsDoctors> PatientsDoctorss {get;set;}
    }
}