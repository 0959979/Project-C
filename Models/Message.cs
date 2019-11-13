using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace zorgapp.Models
{
    public class Message
    {
        public int Id { get; set; }
        public string Sender { get; set; }
        public string Reciever { get; set; }
        public string Subject { get; set; }
        public string Text { get; set; }
        public DateTime Date { get; set; }
        public bool DoctorToPatient { get; set; }
    }
}
