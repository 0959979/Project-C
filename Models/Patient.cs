using System;

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


        public Patient(int patientId, string name, string lastName, int phoneNumber, string userName, string password, string email){
            patientId = PatientId;
            name = FirstName;
            lastName = LastName;
            phoneNumber = PhoneNumber;
            userName = UserName;
            password = Password;
            email = Email;
        }



        
    }

}