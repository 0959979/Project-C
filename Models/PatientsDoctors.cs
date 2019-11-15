using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace zorgapp.Models
{
    public class PatientsDoctors{
        public int DoctorId {get; set;}
        public int PatientId {get; set;}

        public Doctor Doctor {get; set;}
        public Patient Patient {get; set;}

        // public ICollection<Patient> Patients {get; set;}
        // public ICollection<Doctor> Doctors {get; set;}


    }
}