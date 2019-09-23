using System;

namespace zorgapp.Models
{
    public class Patient{
        public int PatientId { get; set; }
        public string Password { get; set;}      
        public string UserName { get; set; }
        public string Name { get; set; }
        public string LastName { get; set; }
        public int PhoneNumber { get; set; }
        public int DocumentId { get; set; }
        public int AppointmentId { get; set; }
        public int ReportId { get; set; }
        public int MedicineId { get; set; }

        public Patient(int patientId, string name, string lastName, int phoneNumber, string userName, string password){
            patientId = PatientId;
            name = Name;
            lastName = LastName;
            phoneNumber = PhoneNumber;
            userName = UserName;
            password = Password;
        }



        
    }

}