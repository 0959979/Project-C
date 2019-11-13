using System;
using System.Collections.Generic;

namespace zorgapp.Models
{
    public class Patient{

        // public Patient(){
        //     this.Doctors = new HashSet<Doctor>();
        // }
        public int PatientId { get; set; }
        public string Password { get; set;}      
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int PhoneNumber { get; set; }
        public string Email {get; set; }
        public List<int> SendMessages { get; set; }
        public List<int> RecievedMessages { get; set; }
        public List<int> DoctorIds{ get; set; }
      //  public ICollection<Doctor> Doctors {get;set;}


        

        // public Patient(int patientId, string firstname, string lastName, int phoneNumber, string userName, string password, string email){
        //     patientId = PatientId;
        //     firstname = FirstName;
        //     lastName = LastName;
        //     phoneNumber = PhoneNumber;
        //     userName = UserName;
        //     password = Password;
        //     email = Email;
        // }



        
    }

}
