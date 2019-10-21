using System;
using System.Collections.Generic;


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

        // public Doctor (string name, string lastName, int phoneNumber, string specialism, string username, string password)
        // {
        //     Name = name;
        //     LastName = lastName;
        //     PhoneNumber = phoneNumber;
        //     Specialism = specialism;
        //     Username = username;
        //     Password = password;
        // }
    }
}