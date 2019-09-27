namespace zorgapp.Models
{
    public class Doctor
    {
        public string Name;
        public string LastName;
        public int PhoneNumber;
        public string Specialism;
        public string Username;
        public string Password;

        public Doctor (string name, string lastName, int phoneNumber, string specialism, string username, string password)
        {
            Name = name;
            LastName = lastName;
            PhoneNumber = phoneNumber;
            Specialism = specialism;
            Username = username;
            Password = password;
        }
    }
}