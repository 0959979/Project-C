using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace zorgapp.Models
{
    public class Case
    {
		public static object Claims { get; internal set; }
		public string CaseId {get;set;}
        public string CaseInfo {get; set;}
        public string CaseName {get;set;}
        public int PatientId {get;set;}
        public int DoctorId {get;set;}

		
	}
}
