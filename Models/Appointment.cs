﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace zorgapp.Models
{
    public class Appointment
    {
        public int AppointmentId { get; set; }
        public DateTime Date { get; set; }
        public string Info { get; set; }

        public string CaseId { get; set; }
        //public int PatientId { get; set; }
        //public int DoctorId { get; set;}

    }
}
