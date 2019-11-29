using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using zorgapp.Models;
using zorgapp.ViewModels;

namespace zorgapp.Controllers {

    public class DoctorController : Controller {
        private readonly DatabaseContext _context;

        public DoctorController(DatabaseContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "Admin")]
        public IActionResult CreateAccount() => View();

        [Authorize(Roles = "Admin")]
        public IActionResult SubmitDoctorAccount(string firstname, string lastname, string email, string phonenumber, string specialism, string localid, string username, string password)
        {
            if (username != null && password != null)
            {
                bool valid = true;
                {
                    Doctor user = _context.Doctors.FirstOrDefault(u => u.Email == email);
                    if (user != null)
                    {
                        ViewBag.emptyfield1 = "this E-mail is already in use";
                        valid = false;
                    }
                }

                {
                    Doctor user = _context.Doctors.FirstOrDefault(u => u.UserName == username);
                    if (user != null)
                    {
                        ViewBag.emptyfield3 = "this username is already in use";
                        valid = false;
                    }
                }
                if (!valid)
                {
                    return View();
                }



                Doctor doctor = new Doctor()
                {
                    FirstName = firstname,
                    LastName = lastname,
                    LocalId = new List<string>(),
                    Email = email,
                    PhoneNumber = phonenumber,
                    Specialism = specialism,
                    UserName = username,
                    Password = Program.Hash256bits(password),
                };
                doctor.LocalId.Add(localid);
                _context.Doctors.Add(doctor);
                _context.SaveChanges();


                ViewData["FirstName"] = doctor.FirstName;
                ViewData["LastName"] = doctor.LastName;

                return View();

            }
  
            return View();
        }
        


        [Authorize(Roles = "Doctor")]
        public ActionResult CreateCase(string caseid, string casename, int patientid)
        {
            if (caseid != null)
            {
                Doctor user = _context.Doctors.FirstOrDefault(u => u.UserName == User.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value);
                int doctorid = user.DoctorId;
                Case newcase = new Case()
                {
                    CaseId = caseid,
                    CaseName = casename,
                    DoctorId = doctorid,
                    PatientId = patientid
                };
                _context.Cases.Add(newcase);
                _context.SaveChanges();

                return RedirectToAction("CreateAppointment", "Doctor");
            }

            return View();
        }
[Authorize(Roles="Doctor")]
        public ActionResult Agenda (string Previous, int dayoffset, string Next, int starthour, int endhour, string Apply)
        {
            {
                System.Diagnostics.Debug.WriteLine("starthour: " + starthour.ToString());
                System.Diagnostics.Debug.WriteLine("endhour: " + endhour.ToString());

                if (!string.IsNullOrEmpty(Next))
                {
                    dayoffset += 7;
                    //ViewBag.Recieved = "Next, dayoffset = "+dayoffset.ToString();
                }
                else if (!string.IsNullOrEmpty(Previous))
                {
                    dayoffset -= 7;
                    //ViewBag.Recieved = "Previous, dayoffset = " + dayoffset.ToString();
                }
                else if (!string.IsNullOrEmpty(Apply))
                {
                    dayoffset += 0;
                    //ViewBag.Recieved = "Previous, dayoffset = " + dayoffset.ToString();
                }
                else
                {
                    dayoffset = 0;
                    //ViewBag.Recieved = "None";
                }
                if (endhour <= starthour)
                {
                    starthour = 6;
                    endhour = 20;
                }
                System.Diagnostics.Debug.WriteLine("starthour: " + starthour.ToString());
                System.Diagnostics.Debug.WriteLine("endhour: " + endhour.ToString());
                bool AmPm = true;
                List<string> Day = new List<string>();
                List<string> Date = new List<string>();
                List<string> Hour = new List<string>();
                List<int> Houri = new List<int>();
                List<int> Minute = new List<int>();
                List<Case> Case = new List<Case>();
                List<Appointment> Appointment = new List<Appointment>();
                //List<List<int>> AppointmentDatesHours = new List<List<int>>();
                //List<string> AppointmentStrings = new List<string>();
                DateTime Today = DateTime.Now;//new DateTime(2020, 4, 13);
                Today = Today.AddDays(dayoffset);
                DateTime FWeekday;
                int offset;

                //the days and dates
                {
                    DayOfWeek WeekDay = Today.DayOfWeek;
                    offset = 0;
                    switch (WeekDay.ToString())
                    {
                        case "Monday":
                            offset = 0;
                            break;
                        case "Tuesday":
                            offset = 1;
                            break;
                        case "Wednesday":
                            offset = 2;
                            break;
                        case "Thursday":
                            offset = 3;
                            break;
                        case "Friday":
                            offset = 4;
                            break;
                        case "Saturday":
                            offset = 5;
                            break;
                        case "Sunday":
                            offset = 6;
                            break;
                        default:
                            offset = 0;
                            break;
                    }
                    //offset ensures that the agenda starts at monday. To get it to start at sunday uncomment:
                    //offset += 1; //Higher offset means the first point on the agenda starts earlier
                    int d;
                    FWeekday = new DateTime(Today.Year,Today.Month,Today.Day);
                    FWeekday = FWeekday.AddDays(-offset);
                    for (d = 0; d < 7; d++)
                    {
                        DateTime day = Today;//DateTime.Now;
                        day = day.AddDays(d - offset);
                        Day.Add(day.DayOfWeek.ToString());
                        Date.Add(day.Date.ToShortDateString());
                        //System.Diagnostics.Debug.WriteLine("RonanDayList: " + day.ToString());
                    }
                }
                //The hours and minutes
                {
                    for(int h = starthour;h<=endhour;h++)
                    {
                        Houri.Add(h);
                        if (AmPm)
                        {
                            if (h > 12)
                            {
                                int u;
                                u = h - 12;
                                Hour.Add(u.ToString()+" pm");
                            }
                            else
                            {
                                Hour.Add(h.ToString() + " am");
                            }
                        }
                        else
                        {
                            Hour.Add(h.ToString());
                        }
                    }
                    for (int m = 0; m < 60; m += 5)
                    {
                        Minute.Add(m);
                    }
                }
                //The appointments of that week
                { //query door Pelle geschreven
                    Doctor user = _context.Doctors.FirstOrDefault(u => u.UserName == User.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value);
                    int doctorid = user.DoctorId;

                    var cases = from c in _context.Cases where c.DoctorId == doctorid select c;
                    var Tempappointments = new List<Appointment>();
                    //var appointments = from a in _context.Appointments where a.select a;

                    foreach (var item in cases)
                    {
                        Case.Add(item);
                    }
                    foreach (var item in cases)
                    {
                        var appointment = from c in _context.Appointments where c.CaseId == item.CaseId select c;
                        foreach (var app in appointment)
                        {
                            Tempappointments.Add(app);
                        }
                        Tempappointments = FilterWeek(Tempappointments, Today,7);
                    }
                    foreach (var item in Tempappointments)
                    {
                        string infosub;
                        infosub = item.Info.Trim();
                        if (infosub.Length > 18)
                        {
                            infosub = infosub.Substring(0, 16) + "...";
                        }
                        /*DateTime AppTime = new DateTime();
                        AppTime.AddHours(item.Date.Hour);
                        AppTime.AddMinutes(item.Date.Minute);*/
                        Appointment.Add(new Appointment()
                        {
                            AppointmentId = item.AppointmentId,
                            Date = item.Date,
                            Info = infosub,
                            CaseId = item.CaseId,
                        }
                        );
                    }
                }
                bool sWeek;
                DateTime c_date;
                c_date = DateTime.Now;
                sWeek = SameWeek(Today,c_date);
                AgendaViewModel agendamodel = new AgendaViewModel
                {
                    Days = Day,
                    Dates = Date,
                    Hours = Hour,
                    Hoursi = Houri,
                    Minutes = Minute,
                    CurrentDate = offset,
                    dayOffset = dayoffset,
                    sameWeek = sWeek,
                    Cases = Case,
                    Appointments = Appointment
                };
                
                return View(agendamodel);
            }

        }
        [Authorize(Roles= "Doctor")]
        public IActionResult EditCase(string caseId, string caseNotes, string Save, string Load)
        {
            if (!string.IsNullOrEmpty(Save))
            {

                //ensure the case belongs to the doctor
                var CaseQ = from c in _context.Cases where c.CaseId == caseId select c;
                Case curCase = CaseQ.FirstOrDefault();
                if (curCase == null)
                {
                    ViewBag.SaveText = " Could not find case with caseId: " + caseId;
                }
                else
                {
                    Doctor doc = _context.Doctors.FirstOrDefault(u => u.UserName == User.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value);
                    int docId = doc.DoctorId;
                    if (!(curCase.DoctorId == docId))
                    {
                        return RedirectToAction("Doctor", "CreateCase");
                    }
                    ViewBag.SaveText = " Changes Saved";
                    curCase.CaseInfo = caseNotes;
                    _context.Update(curCase);
                    _context.SaveChanges();
                }
                //save case in db
            }
            Case currentCase;
            List<Case> caseList = new List<Case>();
            List<Medicine> medicineList = new List<Medicine>();
            List<Appointment> appointments = new List<Appointment>();
            List<Appointment> upcomingAppointments = new List<Appointment>();
            List<Appointment> passedAppointments = new List<Appointment>();
            DateTime today = DateTime.Now;
            Appointment nextAppointment;

            //get the logged in doctor
            Doctor user = _context.Doctors.FirstOrDefault(u => u.UserName == User.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value);
            int doctorId = user.DoctorId;

            //get the case
            var CaseList = from l in _context.Cases where l.DoctorId == doctorId select l;
            var currentCaseList = from c in CaseList where c.CaseId == caseId.ToString() select c;
            currentCase = currentCaseList.FirstOrDefault();
            if (currentCase == null)
            {
                return RedirectToAction("Doctor","CreateCase");
            }
            foreach (Case c in CaseList)
            {
                caseList.Add(c);
            }
            //get the medicine from the case's patient
            var medicineL = from m in _context.Medicines where m.PatientId == currentCase.PatientId select m; //_context.Medicine in ERD
            foreach(Medicine med in medicineL)
            {
                medicineList.Add(med);
            }

            //get the appointments of that case
            var AppointmentL = from a in _context.Appointments where a.CaseId == currentCase.CaseId orderby a.Date ascending select a;
            foreach(Appointment app in AppointmentL)
            {
                appointments.Add(app);
            }
            if (appointments.Count() <= 0)
            {
                return RedirectToAction("Doctor", "CreateAppointment");
            }

            //find which appointment is next
            nextAppointment = appointments.First();
            foreach (Appointment app in appointments)
            {
                if (DateTime.Compare(today,app.Date) <= 0) //if true, then app is at a DateTime later than or the same as today, and thus eligeble to be in upcomingAppointments
                {
                    upcomingAppointments.Add(app);
                }
                else
                {
                    passedAppointments.Add(app);
                }
            }

            //ViewBag.emptyfield = caseId.ToString();

            //upcomingAppointments = OrderByDate(upcomingAppointments);
            //passedAppointments = OrderByDate(passedAppointments);

            CaseViewModel casemodel = new CaseViewModel
            {
                CurrentCase = currentCase,
                CaseList = caseList,
                MedicineList = medicineList,
                //Appointments = appointments,
                UpcomingAppointments = upcomingAppointments,
                PassedAppointments = passedAppointments,
                Today = today
            };

            return View(casemodel);
        }

        public List<Appointment> FilterWeek(List<Appointment> List, DateTime dateTime, int Days)
        {
            List<Appointment> NewList = new List<Appointment>();
            foreach (var app in List)
            {
                DateTime day = new DateTime(dateTime.Year,dateTime.Month,dateTime.Day);
                if (SameWeek(day,app.Date))//(day.AddDays(-(int)day.DayOfWeek) == app.Date.AddDays(-(int)app.Date.DayOfWeek))
                {
                    NewList.Add(app);
                }
                /*for (int d = 0; d < 7; d++)
                {
                    day = day.AddDays(1);
                    if (day.Year == app.Date)
                    //System.Diagnostics.Debug.WriteLine("RonanDayList: " + day.ToString());
                }*/
            }

            return NewList;
        }
        public bool SameWeek(DateTime day1, DateTime day2)
        {
            DateTime Day1;
            DateTime Day2;
            int offset1;
            int offset2;

            Day1 = new DateTime(day1.Year, day1.Month, day1.Day);
            Day2 = new DateTime(day2.Year, day2.Month, day2.Day);

            switch (Day1.DayOfWeek.ToString())//offset to bring the day to monday
            {
                case "Monday":
                    offset1 = 0;
                    break;
                case "Tuesday":
                    offset1 = -1;
                    break;
                case "Wednesday":
                    offset1 = -2;
                    break;
                case "Thursday":
                    offset1 = -3;
                    break;
                case "Friday":
                    offset1 = -4;
                    break;
                case "Saturday":
                    offset1 = -5;
                    break;
                case "Sunday":
                    offset1 = -6;
                    break;
                default:
                    offset1 = 0;
                    break;
            }

            switch (Day2.DayOfWeek.ToString())//offset to bring the day to monday
            {
                case "Monday":
                    offset2 = 0;
                    break;
                case "Tuesday":
                    offset2 = -1;
                    break;
                case "Wednesday":
                    offset2 = -2;
                    break;
                case "Thursday":
                    offset2 = -3;
                    break;
                case "Friday":
                    offset2 = -4;
                    break;
                case "Saturday":
                    offset2 = -5;
                    break;
                case "Sunday":
                    offset2 = -6;
                    break;
                default:
                    offset2 = 0;
                    break;
            }

            Day1 = Day1.AddDays(offset1);
            Day2 = Day2.AddDays(offset2);
            return (Day1 == Day2);
        }

        public List<Appointment> OrderByDate(List<Appointment> List) //Sorts the appointments based on their Date. Earliest to Latest. Does not work for same day
        {
            List<Appointment> newList = new List<Appointment>();
            foreach(Appointment app in List)
            {
                if (List.Count == 0)
                {
                    newList.Add(app);
                }
                else
                {
                    int e = 0;
                    bool added;
                    added = false;
                    while(e < List.Count())
                    {
                        if (DateTime.Compare(app.Date,List[e].Date) < 0)
                        {
                            newList.Insert(e, app);
                            added = true;
                            break;
                        }
                        e++;
                    }
                    if (!added)
                    {
                        newList.Add(app);
                    }
                }
            }
            return newList;
        }

 	    [Authorize(Roles="Doctor")]
		public ActionResult CreateAppointment(string caseid,DateTime date, string info)
		{
			if (caseid != null)
			{
				Appointment appointment = new Appointment()
				{
					CaseId = caseid,
					Date = date,
					Info = info
				};
				_context.Appointments.Add(appointment);
				_context.SaveChanges();

				return RedirectToAction("Profile", "Doctor");
			}
			List<Case> caseList = new List<Case>();

			Doctor user = _context.Doctors.FirstOrDefault(u => u.UserName == User.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value);
			int doctorid = user.DoctorId;

			var cases = from c in _context.Cases where c.DoctorId == doctorid select c;

			foreach (Case c in cases)
			{
				caseList.Add(c);
			}

			NewAppointmentViewModel model;
			model = new NewAppointmentViewModel
			{
				Cases = caseList
			};
            return View(model);
        }

        
        
      public ActionResult AppointmentList(string caseid)
        {
            if (caseid != null)
            {
                Doctor user = _context.Doctors.FirstOrDefault(u => u.UserName == User.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value);
                int doctorid = user.DoctorId;

                var cases = from c in _context.Cases where c.DoctorId == doctorid select c;
                var Case = new List<Case>();

                var Appointment = new List<Appointment>();
                var appointments = from a in _context.Appointments where a.CaseId == caseid select a;

                ViewBag.ID = caseid;

                foreach (var item in cases)
                {
                    Case.Add(item);
                }
                foreach (var item in appointments)
                {
                    Appointment.Add(item);
                }

                AppointmentViewModel caseappointments = new AppointmentViewModel
                {
                    Cases = Case,
                    Appointments = Appointment
                };

                return View(caseappointments);
            }
            else
            {
                Doctor user = _context.Doctors.FirstOrDefault(u => u.UserName == User.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value);
                int doctorid = user.DoctorId;

                var cases = from c in _context.Cases where c.DoctorId == doctorid select c;
                var Case = new List<Case>();

                var Appointment = new List<Appointment>();
                var appointments = from a in _context.Appointments where a.CaseId == caseid select a;

                foreach (var item in cases)
                {
                    Case.Add(item);
                }
                foreach (var item in appointments)
                {
                    Appointment.Add(item);
                }

                AppointmentViewModel caseappointments = new AppointmentViewModel
                {
                    Cases = Case,
                    Appointments = Appointment
                };

                return View(caseappointments);
            }
                                 
        }

            public ActionResult Link(){
            if (TempData["message"]!= null){
            ViewBag.Message = TempData["message"].ToString();
            TempData.Remove("message");
            }
            return View();
        }
        //links patient with doctor 
        public ActionResult SubmitLink(int patientid, int doctorid){
            Doctor doctor = _context.Doctors.FirstOrDefault(m => m.DoctorId == doctorid);
            Patient patient = _context.Patients.FirstOrDefault(y => y.PatientId == patientid);
            string docName = doctor.FirstName;
            string patName = patient.FirstName;
            PatientsDoctors patientsDoctors_ = _context.PatientsDoctorss.FirstOrDefault(
                p => p.PatientId == patientid && p.DoctorId == doctorid
            );

            bool linkmade = _context.PatientsDoctorss.Contains(patientsDoctors_);

            PatientsDoctors patientsDoctors = new PatientsDoctors(){
                PatientId = patientid,
                DoctorId = doctorid
            };

            if(!linkmade){
                _context.PatientsDoctorss.Add(patientsDoctors);
                _context.SaveChanges();
            }
            else if(linkmade){
                TempData["message"] = "Link has already been made";
                return RedirectToAction("Link","Admin");
            }

            ViewData["Doctor"] = docName;
            ViewData["Patient"] = patName;

            return View();
        }


        //Doctorlist Page
        //Authorizes the page so only users with the role Doctor can view it
        [Authorize(Roles = "Admin")]
        public IActionResult DoctorList()
        {
            var doctors = from d in _context.Doctors select d;

            return View(doctors);
        }

        public IActionResult PatientList()
        {
            string username_ = User.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value;
            Doctor doctor = _context.Doctors.FirstOrDefault(u => u.UserName == username_);
            int docId = doctor.DoctorId;
            var Pat = new List<Patient>();
            var patientsDoctors = from d in _context.PatientsDoctorss where d.DoctorId == docId select d;

            foreach (var item in patientsDoctors)
            {   
                if (item.PatientId != null){
                var patient_ = _context.Patients.FirstOrDefault(p => p.PatientId == item.PatientId);
                Pat.Add(patient_);
                };

            }
            return View(Pat);
        }

        public IActionResult PatProfile (IFormCollection form)
        {
            string id = form["patientid"].ToString();
            int id_ = int.Parse(id);
            Patient patient = _context.Patients.FirstOrDefault(u => u.PatientId == id_);
            
            return View(patient);
        }



        public IActionResult AddLocalId (int patientid, string localid){
            Patient patient = _context.Patients.FirstOrDefault(u => u.PatientId == patientid);
            patient.LocalId.Add(localid);
            _context.SaveChanges();
            
            return RedirectToAction("PatientList","Doctor");
        }


        [Authorize(Roles = "Doctor")]
        public ActionResult Message(string reciever, string subject, string text ,string reply)
        {
            Patient patient = _context.Patients.FirstOrDefault(u => u.UserName == reciever);
                if (reply != null)
                {              
                     ViewBag.reply = reply;
                }

            if (patient != null)
            {
                if (text != null && text != "")
                {
                    var username = User.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value;
                    Doctor doctor = _context.Doctors.FirstOrDefault(u => u.UserName == username);
                    Message message = new Message()
                    {
                        Sender = username,
                        Receiver = reciever,
                        Subject = subject,
                        Text = text,
                        Date = DateTime.Now,
                        DoctorToPatient = true
                        };

                    _context.Messages.Add(message);
                    _context.SaveChanges();
                    return RedirectToAction("MessageSend", "Doctor");
                }
                else
                {
                    ViewBag.emptyfield = "You need to type in a message to send it.";
                }
            }
            else if (reciever != null)
            {
                ViewBag.emptyfield = "User not found";
            }
            return View();
        }

        [Authorize(Roles = "Doctor")]
        public ActionResult Inbox()
        {
            Doctor user = _context.Doctors.FirstOrDefault(u => u.UserName == User.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value);
            var message = from m in _context.Messages where m.Text == "" select m;
            var check = from m in _context.Messages where m.DoctorToPatient == false && m.Receiver == User.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value select m;
            if (message != check)
            {
                message = from m in _context.Messages where m.DoctorToPatient == false && m.Receiver == User.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value orderby m.Date descending select m;
            }
            return View(message);
        }

        [Authorize(Roles = "Doctor")]
        public ActionResult SentMessages()
        {
            Doctor user = _context.Doctors.FirstOrDefault(u => u.UserName == User.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value);
            var message = from m in _context.Messages where m.Text == "" select m;
            var check = from m in _context.Messages where m.DoctorToPatient == true && m.Sender == User.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value select m;
            if (message != check)
            {
                message = from m in _context.Messages where m.DoctorToPatient == true && m.Sender == User.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value orderby m.Date descending select m;
            }
            return View(message);
        }
        public ActionResult Reply(IFormCollection form)
        {
            string reply = form["reply"].ToString();
            ViewBag.reply = reply;                   
            return View();
        }

        [Authorize(Roles = "Doctor")]
        public ActionResult MessageSend()
        {
            return View();
        }

        [Authorize(Roles = "Doctor")]
        public ActionResult Profile()
        {
            //Gets the username of the logged in user and sends it to the view
			var username = User.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value;
			ViewBag.username = username;
			var user = _context.Doctors.FirstOrDefault(u => u.UserName == username);
			string email = user.Email.ToString();
			ViewBag.email = email;
			var specialism = user.Specialism.ToString();
			ViewBag.specialism = specialism;
			var phonenumber = user.PhoneNumber;
			ViewBag.phonenumber = phonenumber;
			var firstname = user.FirstName.ToString();
			ViewBag.firstname = firstname;
			var lastname = user.LastName.ToString();
			ViewBag.lastname = lastname;
			return View();
        }

        [Authorize(Roles = "Doctor")]
        public IActionResult UpdateAccount(string firstname, string lastname, string email, string phonenumber, string specialism)
		{
			if (firstname != null)
			{
				var USERNAME = User.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value;
				var USER = _context.Doctors.FirstOrDefault(u => u.UserName == USERNAME);
				USER.FirstName = firstname;
				USER.LastName = lastname;
				USER.Email = email;
				USER.PhoneNumber = phonenumber.ToString();//DIT NIET MERGEN, IS TIJDELIJK
                USER.Specialism = specialism;
				_context.SaveChanges();
				return RedirectToAction("Profile", "Doctor");
			}
			return View();
		}

        [Authorize(Roles = "Doctor")]
        public IActionResult UpdateDoctorAccount()
		{
			string firstname = TempData["MyTempData"].ToString();
			ViewData["FirstName"] = firstname;
			return View();
        }
		public ActionResult AddMedicines(string name, DateTime start_date, DateTime end_date, int amount, int patient_id, float mg)
		{
			List<Patient> Patientslist = new List<Patient>();

			var USERNAME = User.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value;
			var USER = _context.Doctors.FirstOrDefault(u => u.UserName == USERNAME);
			int Id = USER.DoctorId;
			var patientslist = from m in _context.PatientsDoctorss where m.DoctorId == Id select m;
			foreach (var item in patientslist)
				{
				if (item.PatientId != null)
				{
					var patientname = _context.Patients.FirstOrDefault(p => p.PatientId == item.PatientId);
					Patientslist.Add (patientname) ;
				};

			}
			Medicine medicine_ = new Medicine()
			{
				Name = name,
				DateStart = start_date,
				DateEnd = end_date,
				Amount = amount,
				PatientId = patient_id,
				Mg = mg
			};
			
			_context.Medicines.Add(medicine_);
			_context.SaveChanges();


			return View(Patientslist);
		}
	}
}