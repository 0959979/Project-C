using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using zorgapp.Models;

namespace zorgapp.ViewModels
{
	public class PatInfoviewModel
	{	
		public Case CurrentCase;
		public List<Case> CaseList;
		public List<Medicine> MedicineList;
		
		public List<Appointment> UpcomingAppointments;
		//public List<Appointment> PassedAppointments;
		public DateTime Today;
	}
	
}
