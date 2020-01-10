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
using zorgapp.Models;
using zorgapp.ViewModels;

namespace zorgapp.Controllers
{

    public class DoctorController : Controller
    {
        private readonly DatabaseContext _context;

        public DoctorController(DatabaseContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "Admin")]
        // return the view for create account only for the admin
        public IActionResult CreateAccount() => View();

        [Authorize(Roles = "Admin")]
        // creates a new doctor account
        public IActionResult SubmitDoctorAccount(string firstname, string lastname, string email, string phonenumber, string specialism, string localid, string username, string password)
        {
            // check if all parameters are filled in
            if (firstname != null && lastname != null && email != null && phonenumber != null && specialism != null && localid != null && username != null && password != null)
            {
                bool valid = true;
                {
                    // check if the email is already in use by another doctor
                    Doctor user = _context.Doctors.FirstOrDefault(u => u.Email == email);
                    if (user != null)
                    {
                        ViewBag.emptyfield1 = "this E-mail is already in use";
                        valid = false;
                    }
                }

                { // check if the username is already in use by another doctor
                    Doctor user = _context.Doctors.FirstOrDefault(u => u.UserName == username);
                    if (user != null)
                    {
                        ViewBag.emptyfield3 = "this username is already in use";
                        valid = false;
                    }
                }

                { // check if the password is 8 characters long
                    if (password.Count() < 8)
                    {
                        ViewBag.emptyfield3 = "Password should be more than 8 characters long";
                        valid = false;
                    }
                }

                if (!valid)
                {
                    return View();
                }
                // create new doctor with inserted values

                Doctor doctor = new Doctor()
                {
                    FirstName = firstname,
                    LastName = lastname,
                    LocalId = new List<string>(),
                    Email = email,
                    PhoneNumber = phonenumber,
                    Specialism = specialism,
                    UserName = username,
                    Password = Program.Hash256bits(username.ToLower()+password),
                };
                // add localid to new doctor and then add the doctor to the database
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
            if (caseid != null && casename != null)
            {
                Doctor user = _context.Doctors.FirstOrDefault(u => u.UserName == User.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value);
                int doctorid = user.DoctorId;
                PatientsDoctors linkedpatient = _context.PatientsDoctorss.FirstOrDefault(u => u.DoctorId == doctorid && u.PatientId == patientid);
                if (linkedpatient == null)
                {
                    return View();
                }
                Case ecase = _context.Cases.FirstOrDefault(c => c.CaseId == caseid && c.DoctorId == doctorid);
                if (ecase != null)
                {
                    ViewBag.emptyfield1 = "A case with case Id '"+ecase.CaseId+"' already exists!";
                    return View();
                }
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
        [Authorize(Roles = "Doctor")]
        public ActionResult Agenda(string Previous, int dayoffset, string Next, int starthour, int endhour, string Apply)
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
                    FWeekday = new DateTime(Today.Year, Today.Month, Today.Day);
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
                    for (int h = starthour; h <= endhour; h++)
                    {
                        Houri.Add(h);
                        if (AmPm)
                        {
                            if (h > 12)
                            {
                                int u;
                                u = h - 12;
                                Hour.Add(u.ToString() + " pm");
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
                        appointment = from c in appointment where c.DoctorId == item.DoctorId select c; //only the appointments of that doctorId
                        foreach (var app in appointment)
                        {
                            Tempappointments.Add(app);
                        }
                        Tempappointments = FilterWeek(Tempappointments, Today, 7);
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
                sWeek = SameWeek(Today, c_date);
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
        [Authorize(Roles = "Doctor")]
        public IActionResult EditCase(string caseId, string caseNotes, string Save, string Load, string name, DateTime start_date, DateTime end_date, int amount, float mg, string Add)
        {
            if (!string.IsNullOrEmpty(Save) || !string.IsNullOrEmpty(Add)) //check if the doctor pressed the Save or Add button, if so, run this codeblock
            {
                //ensure the case belongs to the doctor
                Doctor luser = _context.Doctors.FirstOrDefault(u => u.UserName == User.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value);
                int ldoctorId = luser.DoctorId;
                var CaseQ = from c in _context.Cases where c.CaseId == caseId select c;
                Case curCase = CaseQ.FirstOrDefault(c => c.DoctorId == ldoctorId);
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
                        return RedirectToAction("CreateCase", "Doctor");
                    }
                    //medicine
                    if (name != null && end_date != null && mg != null)
                    {
                        int patId;
                        patId = curCase.PatientId;
                        if (start_date == null || start_date.Year < DateTime.Now.Year)
                        {
                            start_date = DateTime.Now;
                        }
                        if (end_date.Year < DateTime.Now.Year)
                        {
                            start_date = DateTime.Now;
                        }
                        if (amount == null)
                        {
                            amount = 1;
                        }
                        Medicine newMedicine = new Medicine()
                        {
                            Name = name,
                            DateStart = start_date,
                            DateEnd = end_date,
                            Amount = amount,
                            PatientId = patId,
                            Mg = mg
                        };
                        _context.Medicines.Add(newMedicine);
                    }
                    ViewBag.SaveText = " Changes Saved";
                    curCase.CaseInfo = caseNotes;
                    _context.Update(curCase);
                    _context.SaveChanges();
                }
                //save case in db
            }
            //if the doctor has not pressed Save or Add on the editcase page...
            Case currentCase;
            string patientName;
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
            Case FirstCase = CaseList.FirstOrDefault();
            if (FirstCase == null) //True if the doctor has no cases, he will be sent to the page to create a case
            {
                return RedirectToAction("Createcase", "Doctor");
            }
            if (caseId == null)
            {
                caseId = FirstCase.CaseId;
            }
            var currentCaseList = from c in CaseList where c.CaseId == caseId.ToString() select c;
            currentCase = currentCaseList.FirstOrDefault();
            if (currentCase == null) //if there is no case with the given CaseId, the doctor will be sent to the create case page
            {
                return RedirectToAction("CreateCase", "Doctor");
            }
            foreach (Case c in CaseList)
            {
                caseList.Add(c);
            }
            //get the medicine from the case's patient
            var medicineL = from m in _context.Medicines where m.PatientId == currentCase.PatientId select m; //_context.Medicine in ERD
            foreach (Medicine med in medicineL)
            {
                medicineList.Add(med);
            }

            //get the appointments of that case
            var AppointmentL = from a in _context.Appointments where a.CaseId == currentCase.CaseId && a.DoctorId == currentCase.DoctorId orderby a.Date ascending select a;
            foreach(Appointment app in AppointmentL)
            {
                appointments.Add(app);
            }
            if (appointments.Count() <= 0)
            {
                return RedirectToAction("CreateAppointment", "Doctor");
            }

            //find which appointment is next
            nextAppointment = appointments.First();
            foreach (Appointment app in appointments)
            {
                if (DateTime.Compare(today, app.Date) <= 0) //if true, then app is at a DateTime later than or the same as today, and thus eligeble to be in upcomingAppointments
                {
                    upcomingAppointments.Add(app);
                }
                else
                {
                    passedAppointments.Add(app);
                }
            }

            var PatientL = from p in _context.Patients where p.PatientId == currentCase.PatientId select p;
            Patient pat = PatientL.FirstOrDefault();
            if (pat != null)
            {
                patientName = pat.FirstName + " " + pat.LastName;
            }
            else
            {
                patientName = "Error: Patient with patientId '" + currentCase.PatientId.ToString() + "' not found!";
            }

            CaseViewModel casemodel = new CaseViewModel //all the information that is needed for the appointment page
            {
                CurrentCase = currentCase,
                PatientName = patientName,
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
                DateTime day = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day);
                if (SameWeek(day, app.Date))//(day.AddDays(-(int)day.DayOfWeek) == app.Date.AddDays(-(int)app.Date.DayOfWeek))
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
        public static bool SameWeek(DateTime day1, DateTime day2)
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
            foreach (Appointment app in List)
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
                    while (e < List.Count())
                    {
                        if (DateTime.Compare(app.Date, List[e].Date) < 0)
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

            Doctor user = _context.Doctors.FirstOrDefault(u => u.UserName == User.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value);
			int doctorid = user.DoctorId;
			if (caseid != null) //this is the case when something is submitted
			{
				Appointment appointment = new Appointment()
				{
					CaseId = caseid,
					Date = date,
					Info = info,
                    DoctorId = doctorid
				};
				_context.Appointments.Add(appointment);
				_context.SaveChanges();

				return RedirectToAction("Profile", "Doctor");
			}
            //the code bellow happens when the page is first visited without an appointment being made
            //the code makes a list of all cases belonging to that doctor
			List<Case> caseList = new List<Case>();


            var cases = from c in _context.Cases where c.DoctorId == doctorid select c;

            foreach (Case c in cases)
            {
                caseList.Add(c);
            }

            NewAppointmentViewModel model;
			model = new NewAppointmentViewModel //make a model containing a list of cases to pass on to the view
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
                appointments = from c in appointments where c.DoctorId == doctorid select c; //only the appointments of that doctorId

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

  public ActionResult Link()
        {
            // return the view of Link and if a tempdata named "message" is not null, put the tempdata in a viewbag to return to the view
            if (TempData["message"] != null)
            {
                ViewBag.Message = TempData["message"].ToString();
                TempData.Remove("message");
            }
            return View();
        }
        //links patient with doctor 
        public ActionResult SubmitLink(int patientid, int doctorid)
        {
            // search for doctor with doctor id filled in by user
            Doctor doctor = _context.Doctors.FirstOrDefault(m => m.DoctorId == doctorid);
            // search for patient with patient id filled in by user
            Patient patient = _context.Patients.FirstOrDefault(y => y.PatientId == patientid);
            // create the link between the doctor and patient
            PatientsDoctors patientsDoctors_ = _context.PatientsDoctorss.FirstOrDefault(
                p => p.PatientId == patientid && p.DoctorId == doctorid
            );

            // check if the link has already been made

            bool linkmade = _context.PatientsDoctorss.Contains(patientsDoctors_);

            // create a new link for the patient and doctor

            PatientsDoctors patientsDoctors = new PatientsDoctors()
            {
                PatientId = patientid,
                DoctorId = doctorid
            };
            //check if patient exists, if the patient does not exists, warn the user 
            if (patient == null)
            {
                if (TempData != null)
                { TempData["message"] = "PatientId does not exist"; }
                return RedirectToAction("Link", "Admin");
            }
            //check if doctor exists, if the doctor does not exists, warn the user 
            if (doctor == null)
            {
                if (TempData != null)
                { TempData["message"] = "Doctorid does not exist"; }
                return RedirectToAction("Link", "Admin");
            }

            // if the link is not made, add the link to the database

            if (!linkmade)
            {
                _context.PatientsDoctorss.Add(patientsDoctors);
                _context.SaveChanges();
            }
            // if the link is made, do not add the link and warn the user

            else if (linkmade)
            {
                if (TempData != null)
                {TempData["message"] = "Link has already been made";}
                return RedirectToAction("Link", "Admin");
            }

            ViewData["Doctor"] = doctor.FirstName;
            ViewData["Patient"] = patient.FirstName;

            return View();
        }

        //Doctorlist Page
        //Authorizes the page so only users with the role admin can view it
        [Authorize(Roles = "Admin")]
        public IActionResult DoctorList()
        {
            //selects all the doctors from the database
            var doctors = from d in _context.Doctors select d;

            return View(doctors);
        }

        public IActionResult PatientList()
        {
            // take the username of the doctor logged in
            string username_ = User.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value;
            // search for the doctor with that username
            Doctor doctor = _context.Doctors.FirstOrDefault(u => u.UserName == username_);
            // take the id of that doctor
            int docId = doctor.DoctorId;
            // create new list of patients
            var Pat = new List<Patient>();
            // create list of patientsdoctors (links) where the doctor is linked with 0 to many patients
            var patientsDoctors = from d in _context.PatientsDoctorss where d.DoctorId == docId select d;

            // for every item of patientsdoctors find the patient and add it to the list of patients
            foreach (var item in patientsDoctors)
            {
                if (item.PatientId != null)
                {
                    var patient_ = _context.Patients.FirstOrDefault(p => p.PatientId == item.PatientId);
                    Pat.Add(patient_);
                };

            }

            // return the view with the list of patients linked to the doctor logged in

            return View(Pat);
        }

        public IActionResult PatProfile(IFormCollection form)
        {

            string id = form["patientid"].ToString();
            int id_ = int.Parse(id);
            Patient patient = _context.Patients.FirstOrDefault(u => u.PatientId == id_);
            string doctorusername = User.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value;
            Doctor doctor = _context.Doctors.FirstOrDefault(u => u.UserName == doctorusername);
            int doctorid = doctor.DoctorId;

            var medicines_ = from m in _context.Medicines where m.PatientId == id_ select m;
            var emptymedicine = _context.Medicines.FirstOrDefault(m => m.MedicineId == 0);
            List<Medicine> medicines = new List<Medicine>();
            if (medicines_ == null)
            {
                medicines.Add(emptymedicine);
            }
            else
            {
                foreach (var item in medicines_)
                {
                    medicines.Add(item);
                }
            }

            var cases_ = from c in _context.Cases where c.PatientId == id_ && c.DoctorId == doctorid select c;
            var emptycase = _context.Cases.FirstOrDefault(m => m.CaseId == "");
            List<Case> cases = new List<Case>();
            if (cases_ == null)
            {
                cases.Add(emptycase);
            }
            else
            {
                foreach (var item in cases_)
                {
                    cases.Add(item);
                }
            }


            ProfileViewModel profiledata = new ProfileViewModel
            {
                UserInfo = patient,
                Cases = cases,
                Medicines = medicines
            };
            return View(profiledata);
        }



        public IActionResult AddLocalId(int patientid, string localid)
        {
            Patient pat = _context.Patients.FirstOrDefault(u => u.PatientId == patientid);
            bool localidExists = false;

            if (pat.LocalId == null)
            {
                pat.LocalId = new List<string>();
                _context.SaveChanges();
            }
            
            var localids = pat.LocalId;

            foreach (var item in localids)
            {
                if (item == localid)
                {
                    localidExists = true;
                }
            }

            if (localid == null )
            {   if (TempData != null){
                TempData["message"] = "Local id cannot be empty";
                }
                return RedirectToAction("PatientList", "Doctor");

            }
            if (localidExists)
            {
                if (TempData != null){
                TempData["message"] = "Local id is already added";
                }
                return RedirectToAction("PatientList", "Doctor");
            }

            if (localid != null && !localidExists)
            {
                Patient patient = _context.Patients.FirstOrDefault(u => u.PatientId == patientid);
                patient.LocalId.Add(localid);
                _context.SaveChanges();

                return RedirectToAction("PatientList", "Doctor");
            }
            else
            {
                return RedirectToAction("PatientList", "Doctor");
            }

            return View();

        }


        [Authorize(Roles = "Doctor")]
        public ActionResult Message(string reciever, string subject, string text)
        {
            Patient patient = _context.Patients.FirstOrDefault(u => u.UserName == reciever);               
            if (patient != null)
            {
                if (string.IsNullOrEmpty(subject))
                {
                    ViewBag.emptyfield = "You need enter a subject to send a message.";
                }
                else if (string.IsNullOrEmpty(text))
                {
                    ViewBag.emptyfield = "You need to enter a message to send it.";                    
                }
                else

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
            }
            else if (string.IsNullOrEmpty(reciever))
            {
                ViewBag.emptyfield = "You need to enter a receiver to send a message";
            }
            else if (!(string.IsNullOrEmpty(reciever)))
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
        // public ActionResult AddMedicines(string name, DateTime start_date, DateTime end_date, int amount, int patient_id, float mg)
        // {
        //     List<Patient> Patientslist = new List<Patient>();

        //     var USERNAME = User.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value;
        //     var USER = _context.Doctors.FirstOrDefault(u => u.UserName == USERNAME);
        //     int Id = USER.DoctorId;
        //     var patientslist = from m in _context.PatientsDoctorss where m.DoctorId == Id select m;
        //     foreach (var item in patientslist)
        //     {
        //         if (item.PatientId != null)
        //         {
        //             var patientname = _context.Patients.FirstOrDefault(p => p.PatientId == item.PatientId);
        //             Patientslist.Add(patientname);
        //         };

		// 	}

        //     if (name != null)
        //     {
        //         Medicine medicine_ = new Medicine()
        //         {
        //             Name = name,
        //             DateStart = start_date,
        //             DateEnd = end_date,
        //             Amount = amount,
        //             PatientId = patient_id,
        //             Mg = mg
        //         };

        //         _context.Medicines.Add(medicine_);
        //         _context.SaveChanges();
        //     }


        //     return View(Patientslist);
		// }

        public ActionResult AddMedicines(string name, DateTime start_date, DateTime end_date, int amount, int patient_id, float mg)
        {
            
            if (name != null && start_date != null && end_date != null  )
            {

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
            }


            return View();
        }
    


        //TESTING
        public DatabaseContext getContext()
        {
            return _context;
        }
        public IActionResult noAccess()
        {
            return View();
        }
        [Authorize(Roles = "Doctor")]
        public IActionResult TestPage()
        {
            if (User.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value.ToLower() != "admin")
            {
                return RedirectToAction("Login", "Patient");
            }
            List<Tuple<string, string>> tupleList = new List<Tuple<string, string>>();

            List<DoctorTest> testlist = new List<DoctorTest>();
            {
                testlist.Add(new CreateCaseTest1(this));
                testlist.Add(new CreateCaseTest2(this));
                testlist.Add(new CreateCaseTest3(this));
                testlist.Add(new CreateCaseTest4(this));
                testlist.Add(new CreateCaseTest5(this));
                testlist.Add(new CreateCaseTest6(this));
                testlist.Add(new AddMedicinesAlreadyLinkedTest1(this));
                testlist.Add(new AddMedicinesMissingParameterTest1(this));
                testlist.Add(new AddMedicinesPatientNullTest1(this));
                testlist.Add(new AddMedicinesNotAlreadyLinkedTest1(this));
                testlist.Add(new MakeAppointmentAlreadyLinkedTest2(this));
                testlist.Add(new MakeAppointmentMissesParameterTest2(this));
                testlist.Add(new MakeAppointmentNotAlreadyLinkedTest2(this));
                testlist.Add(new LinkAlreadyLinkedTest3(this));
                testlist.Add(new LinkDoctorNullTest3(this));
                testlist.Add(new LinkPatientNullTest3(this));
                testlist.Add(new LinkDocPatsNullTest3(this));
            }
            foreach (DoctorTest T in testlist)
            {
                tupleList.Add(new Tuple<string, string>(T.Id, T.Id));
            }

            TestListViewModel testlistmodel = new TestListViewModel { tuples = tupleList };
            return View(testlistmodel);
        }

        public IActionResult StartTest(string TestId)
        {
            List<DoctorTest> testlist = new List<DoctorTest>();
            {
                testlist.Add(new CreateCaseTest1(this));
                testlist.Add(new CreateCaseTest2(this));
                testlist.Add(new CreateCaseTest3(this));
                testlist.Add(new CreateCaseTest4(this));
                testlist.Add(new CreateCaseTest5(this));
                testlist.Add(new CreateCaseTest6(this));
                testlist.Add(new AddMedicinesAlreadyLinkedTest1(this));
                testlist.Add(new AddMedicinesMissingParameterTest1(this));
                testlist.Add(new AddMedicinesPatientNullTest1(this));
                testlist.Add(new AddMedicinesNotAlreadyLinkedTest1(this));
                testlist.Add(new MakeAppointmentAlreadyLinkedTest2(this));
                testlist.Add(new MakeAppointmentMissesParameterTest2(this));
                testlist.Add(new MakeAppointmentNotAlreadyLinkedTest2(this));
                testlist.Add(new LinkAlreadyLinkedTest3(this));
                testlist.Add(new LinkDoctorNullTest3(this));
                testlist.Add(new LinkPatientNullTest3(this));
                testlist.Add(new LinkDocPatsNullTest3(this));
            }
            DoctorTest testobj = testlist.FirstOrDefault();
            foreach (DoctorTest T in testlist)
            {
                if (T.Id == TestId)
                {
                    testobj = T;
                }
            }
            TestViewModel Model = testobj.Run();
            return View(Model);
        }
    }
    internal abstract class DoctorTest
    {
        public abstract TestViewModel Run();
        public DoctorController testController;
        public string Id;
        public string Description;
        public string Steps;
        public string Criteria;
        public string Inputstr;
        public string Aresult;
        public string Eresult;
        public bool Pass;
    }
    internal class TemplateTest : DoctorTest
    {
        public TemplateTest(DoctorController tc)
        {
            testController = tc;
            Id = ".Integration.";
            Description = " test 1";
            Steps = "Check if true == true";
            Criteria = "true must be true";
            Inputstr = "true";
            Aresult = "";
            Eresult = "True";
        }

        public override TestViewModel Run()
        {
            TestViewModel model;

            //arrange
            bool Pass = false;
            DoctorController controller = testController;

            //act
            try
            {
                if (true == true)
                {
                    Aresult = "True";
                }
            }
            catch (Exception e)
            {
                Pass = false;
                Aresult = e.ToString();
                model = new TestViewModel()
                {
                    id = Id,
                    time = DateTime.Now,
                    description = Description,
                    steps = Steps,
                    criteria = Criteria,
                    input = Inputstr,
                    aresult = Aresult,
                    eresult = Eresult,
                    pass = Pass
                };
                return model;
            }

            //assert
            if (Aresult == Eresult)
            {
                Pass = true;
            }
            else
            {
                Pass = false;
            }

            model = new TestViewModel()
            {
                id = Id,
                time = DateTime.Now,
                description = Description,
                steps = Steps,
                criteria = Criteria,
                input = Inputstr,
                aresult = Aresult,
                eresult = Eresult,
                pass = Pass
            };
            return model;
        }
    }
    internal class CreateCaseTest1 : DoctorTest
    {
        public CreateCaseTest1(DoctorController tc)
        {
            testController = tc;
            Id = "D5.Integration.CC1";
            Description = "Create Case test 1";
            Steps = "Make sure the doctor is linked and the case does not already exist, then create the case.";
            Criteria = "The case must not be present before the run phase, and present after this phase.";
            Inputstr = "All fields filled in";
            Aresult = "";
            Eresult = "Case with Id " + "TCC1" + " and name " + "Test Case CC1";
        }

        public override TestViewModel Run()
        {
            TestViewModel model;

            //arrange
            bool Pass = false;
            DoctorController controller = testController;
            DatabaseContext _context = controller.getContext();
            string cId = "TCC1";

            Doctor loggedDoctor = _context.Doctors.FirstOrDefault(u => u.UserName.ToLower() == "Admin".ToLower());
            int docId = loggedDoctor.DoctorId;

            //make sure the link is present
            try
            {
                var links = from l in _context.PatientsDoctorss where l.DoctorId == docId select l;
                PatientsDoctors pl = links.FirstOrDefault(u => u.PatientId == 1);
                if (pl == null)
                {
                    pl = new PatientsDoctors()
                    {
                        PatientId = 1,
                        DoctorId = docId
                    };
                    _context.PatientsDoctorss.Add(pl);
                    _context.SaveChanges();
                }
            }
            catch (Exception e)
            {
                Pass = false;
                Aresult = e.ToString();
                model = new TestViewModel()
                {
                    id = Id,
                    time = DateTime.Now,
                    description = Description,
                    steps = Steps,
                    criteria = Criteria,
                    input = Inputstr,
                    aresult = Aresult,
                    eresult = Eresult,
                    pass = Pass
                };
                return model;
            }
            //make sure the case does not already exist
            try
            {
                var cases = from c in _context.Cases where c.DoctorId == docId select c;
                Case pl = cases.FirstOrDefault(u => u.CaseId == cId);
                if (pl != null)                {
                    _context.Cases.Remove(pl);
                    _context.SaveChanges();
                }
            }
            catch (Exception e)
            {
                Pass = false;
                Aresult = e.ToString();
                model = new TestViewModel()
                {
                    id = Id,
                    time = DateTime.Now,
                    description = Description,
                    steps = Steps,
                    criteria = Criteria,
                    input = Inputstr,
                    aresult = Aresult,
                    eresult = Eresult,
                    pass = Pass
                };
                return model;
            }

            //act
            try
            {
                controller.CreateCase(cId,"Test Case CC1",1);
            }
            catch (Exception e)
            {
                Pass = false;
                Aresult = e.ToString();
                model = new TestViewModel()
                {
                    id = Id,
                    time = DateTime.Now,
                    description = Description,
                    steps = Steps,
                    criteria = Criteria,
                    input = Inputstr,
                    aresult = Aresult,
                    eresult = Eresult,
                    pass = Pass
                };
                return model;
            }

            //assert
            var casel = from c in _context.Cases where c.DoctorId == docId select c;
            Case ncase = casel.FirstOrDefault(u => u.CaseId == cId);

            if (ncase == null)
            {
                Aresult = "Case with Id "+cId+" is null";
            }
            else
            {
                Aresult = "Case with Id " + ncase.CaseId + " and name "+ncase.CaseName;
            }

            if (Aresult == Eresult)
            {
                Pass = true;
            }
            else
            {
                Pass = false;
            }

            model = new TestViewModel()
            {
                id = Id,
                time = DateTime.Now,
                description = Description,
                steps = Steps,
                criteria = Criteria,
                input = Inputstr,
                aresult = Aresult,
                eresult = Eresult,
                pass = Pass
            };
            return model;
        }
    }
    internal class CreateCaseTest2 : DoctorTest
    {
        public CreateCaseTest2(DoctorController tc)
        {
            testController = tc;
            Id = "D5.Integration.CC2";
            Description = "Create Case test 2";
            Steps = "Make sure the doctor is linked and the case does not already exist, then create the case.";
            Criteria = "The case must not be present before the run phase, and still not after this phase.";
            Inputstr = "All fields filled in except caseId, which is null";
            Aresult = "";
            Eresult = "Case with Id " + "TCC2" + " is null";
        }

        public override TestViewModel Run()
        {
            TestViewModel model;

            //arrange
            bool Pass = false;
            DoctorController controller = testController;
            DatabaseContext _context = controller.getContext();
            string cId = "TCC2";
            string caseName = "Test Case CC2";

            Doctor loggedDoctor = _context.Doctors.FirstOrDefault(u => u.UserName.ToLower() == "Admin".ToLower());
            int docId = loggedDoctor.DoctorId;

            //make sure the link is present
            try
            {
                var links = from l in _context.PatientsDoctorss where l.DoctorId == docId select l;
                PatientsDoctors pl = links.FirstOrDefault(u => u.PatientId == 1);
                if (pl == null)
                {
                    pl = new PatientsDoctors()
                    {
                        PatientId = 1,
                        DoctorId = docId
                    };
                    _context.PatientsDoctorss.Add(pl);
                    _context.SaveChanges();
                }
            }
            catch (Exception e)
            {
                Pass = false;
                Aresult = e.ToString();
                model = new TestViewModel()
                {
                    id = Id,
                    time = DateTime.Now,
                    description = Description,
                    steps = Steps,
                    criteria = Criteria,
                    input = Inputstr,
                    aresult = Aresult,
                    eresult = Eresult,
                    pass = Pass
                };
                return model;
            }
            //make sure the case does not already exist
            try
            {
                var cases = from c in _context.Cases where c.DoctorId == docId select c;
                Case pl = cases.FirstOrDefault(u => u.CaseName == caseName);
                if (pl != null)
                {
                    _context.Cases.Remove(pl);
                    _context.SaveChanges();
                }
            }
            catch (Exception e)
            {
                Pass = false;
                Aresult = e.ToString();
                model = new TestViewModel()
                {
                    id = Id,
                    time = DateTime.Now,
                    description = Description,
                    steps = Steps,
                    criteria = Criteria,
                    input = Inputstr,
                    aresult = Aresult,
                    eresult = Eresult,
                    pass = Pass
                };
                return model;
            }

            //act
            try
            {
                controller.CreateCase(null, caseName, 1);
            }
            catch (Exception e)
            {
                Pass = false;
                Aresult = e.ToString();
                model = new TestViewModel()
                {
                    id = Id,
                    time = DateTime.Now,
                    description = Description,
                    steps = Steps,
                    criteria = Criteria,
                    input = Inputstr,
                    aresult = Aresult,
                    eresult = Eresult,
                    pass = Pass
                };
                return model;
            }

            //assert
            var casel = from c in _context.Cases where c.DoctorId == docId select c;
            Case ncase = casel.FirstOrDefault(u => u.CaseName == caseName);

            if (ncase == null)
            {
                Aresult = "Case with Id " + cId + " is null";
            }
            else
            {
                Aresult = "Case with Id " + ncase.CaseId + " and name " + ncase.CaseName;
            }

            if (Aresult == Eresult)
            {
                Pass = true;
            }
            else
            {
                Pass = false;
            }

            model = new TestViewModel()
            {
                id = Id,
                time = DateTime.Now,
                description = Description,
                steps = Steps,
                criteria = Criteria,
                input = Inputstr,
                aresult = Aresult,
                eresult = Eresult,
                pass = Pass
            };
            return model;
        }
    }
    internal class CreateCaseTest3 : DoctorTest
    {
        public CreateCaseTest3(DoctorController tc)
        {
            testController = tc;
            Id = "D5.Integration.CC3";
            Description = "Create Case test 3";
            Steps = "Make sure the doctor is linked and the case does not already exist, then create the case.";
            Criteria = "The case must not be present before the run phase, and still not after this phase.";
            Inputstr = "All fields filled in except caseName, which is null";
            Aresult = "";
            Eresult = "Case with Id " + "TCC3" + " is null";
        }

        public override TestViewModel Run()
        {
            TestViewModel model;

            //arrange
            bool Pass = false;
            DoctorController controller = testController;
            DatabaseContext _context = controller.getContext();
            string cId = "TCC3";

            Doctor loggedDoctor = _context.Doctors.FirstOrDefault(u => u.UserName.ToLower() == "Admin".ToLower());
            int docId = loggedDoctor.DoctorId;

            //make sure the link is present
            try
            {
                var links = from l in _context.PatientsDoctorss where l.DoctorId == docId select l;
                PatientsDoctors pl = links.FirstOrDefault(u => u.PatientId == 1);
                if (pl == null)
                {
                    pl = new PatientsDoctors()
                    {
                        PatientId = 1,
                        DoctorId = docId
                    };
                    _context.PatientsDoctorss.Add(pl);
                    _context.SaveChanges();
                }
            }
            catch (Exception e)
            {
                Pass = false;
                Aresult = e.ToString();
                model = new TestViewModel()
                {
                    id = Id,
                    time = DateTime.Now,
                    description = Description,
                    steps = Steps,
                    criteria = Criteria,
                    input = Inputstr,
                    aresult = Aresult,
                    eresult = Eresult,
                    pass = Pass
                };
                return model;
            }
            //make sure the case does not already exist
            try
            {
                var cases = from c in _context.Cases where c.DoctorId == docId select c;
                Case pl = cases.FirstOrDefault(u => u.CaseId == cId);
                if (pl != null)
                {
                    _context.Cases.Remove(pl);
                    _context.SaveChanges();
                }
            }
            catch (Exception e)
            {
                Pass = false;
                Aresult = e.ToString();
                model = new TestViewModel()
                {
                    id = Id,
                    time = DateTime.Now,
                    description = Description,
                    steps = Steps,
                    criteria = Criteria,
                    input = Inputstr,
                    aresult = Aresult,
                    eresult = Eresult,
                    pass = Pass
                };
                return model;
            }

            //act
            try
            {
                controller.CreateCase(cId, null, 1);
            }
            catch (Exception e)
            {
                Pass = false;
                Aresult = e.ToString();
                model = new TestViewModel()
                {
                    id = Id,
                    time = DateTime.Now,
                    description = Description,
                    steps = Steps,
                    criteria = Criteria,
                    input = Inputstr,
                    aresult = Aresult,
                    eresult = Eresult,
                    pass = Pass
                };
                return model;
            }

            //assert
            var casel = from c in _context.Cases where c.DoctorId == docId select c;
            Case ncase = casel.FirstOrDefault(u => u.CaseId == cId);

            if (ncase == null)
            {
                Aresult = "Case with Id " + cId + " is null";
            }
            else
            {
                Aresult = "Case with Id " + ncase.CaseId + " and name " + ncase.CaseName;
            }

            if (Aresult == Eresult)
            {
                Pass = true;
            }
            else
            {
                Pass = false;
            }

            model = new TestViewModel()
            {
                id = Id,
                time = DateTime.Now,
                description = Description,
                steps = Steps,
                criteria = Criteria,
                input = Inputstr,
                aresult = Aresult,
                eresult = Eresult,
                pass = Pass
            };
            return model;
        }
    }
    internal class CreateCaseTest4 : DoctorTest
    {
        public CreateCaseTest4(DoctorController tc)
        {
            testController = tc;
            Id = "D5.Integration.CC4";
            Description = "Create Case test 4";
            Steps = "Make sure the doctor is not linked and the case does not already exist, then create the case.";
            Criteria = "The case must not be present before the run phase, and still not after this phase.";
            Inputstr = "All fields filled with the patientId being that of an unlinked patient, which is null";
            Aresult = "";
            Eresult = "Case with Id " + "TCC4" + " is null";
        }

        public override TestViewModel Run()
        {
            TestViewModel model;

            //arrange
            bool Pass = false;
            DoctorController controller = testController;
            DatabaseContext _context = controller.getContext();
            string cId = "TCC4";
            string caseName = "Test Case CC4";

            Doctor loggedDoctor = _context.Doctors.FirstOrDefault(u => u.UserName.ToLower() == "Admin".ToLower());
            Patient linkedPatient = _context.Patients.FirstOrDefault(u => u.UserName.ToLower() == "Admin".ToLower());
            int docId = loggedDoctor.DoctorId;
            int patId = linkedPatient.PatientId;

            //make sure the link is not present
            try
            {
                var links = from l in _context.PatientsDoctorss where l.DoctorId == docId select l;
                PatientsDoctors pl = links.FirstOrDefault(u => u.PatientId == patId);
                if (pl != null)
                {
                    _context.PatientsDoctorss.Remove(pl);
                    _context.SaveChanges();
                }
            }
            catch (Exception e)
            {
                Pass = false;
                Aresult = e.ToString();
                model = new TestViewModel()
                {
                    id = Id,
                    time = DateTime.Now,
                    description = Description,
                    steps = Steps,
                    criteria = Criteria,
                    input = Inputstr,
                    aresult = Aresult,
                    eresult = Eresult,
                    pass = Pass
                };
                return model;
            }
            //make sure the case does not already exist
            try
            {
                var cases = from c in _context.Cases where c.DoctorId == docId select c;
                Case pl = cases.FirstOrDefault(u => u.CaseId == cId);
                if (pl != null)
                {
                    _context.Cases.Remove(pl);
                    _context.SaveChanges();
                }
            }
            catch (Exception e)
            {
                Pass = false;
                Aresult = e.ToString();
                model = new TestViewModel()
                {
                    id = Id,
                    time = DateTime.Now,
                    description = Description,
                    steps = Steps,
                    criteria = Criteria,
                    input = Inputstr,
                    aresult = Aresult,
                    eresult = Eresult,
                    pass = Pass
                };
                return model;
            }

            //act
            try
            {
                controller.CreateCase(cId, caseName, patId);
            }
            catch (Exception e)
            {
                Pass = false;
                Aresult = e.ToString();
                model = new TestViewModel()
                {
                    id = Id,
                    time = DateTime.Now,
                    description = Description,
                    steps = Steps,
                    criteria = Criteria,
                    input = Inputstr,
                    aresult = Aresult,
                    eresult = Eresult,
                    pass = Pass
                };
                return model;
            }

            //assert
            var casel = from c in _context.Cases where c.DoctorId == docId select c;
            Case ncase = casel.FirstOrDefault(u => u.CaseId == cId);

            if (ncase == null)
            {
                Aresult = "Case with Id " + cId + " is null";
            }
            else
            {
                Aresult = "Case with Id " + ncase.CaseId + " and name " + ncase.CaseName;
            }

            if (Aresult == Eresult)
            {
                Pass = true;
            }
            else
            {
                Pass = false;
            }

            model = new TestViewModel()
            {
                id = Id,
                time = DateTime.Now,
                description = Description,
                steps = Steps,
                criteria = Criteria,
                input = Inputstr,
                aresult = Aresult,
                eresult = Eresult,
                pass = Pass
            };
            return model;
        }
    }
    internal class CreateCaseTest5 : DoctorTest
    {
        public CreateCaseTest5(DoctorController tc)
        {
            testController = tc;
            Id = "D5.Integration.CC5";
            Description = "Create Case test 5";
            Steps = "Make sure the doctor is not linked and the case does not already exist, then create the case.";
            Criteria = "The case must not be present before the run phase, and still not after this phase.";
            Inputstr = "All fields filled with the patientId being that of an unlinked patient, which is null";
            Aresult = "";
            Eresult = "No new cases with Id TCC5 have been added";
        }

        public override TestViewModel Run()
        {
            TestViewModel model;

            //arrange
            bool Pass = false;
            DoctorController controller = testController;
            DatabaseContext _context = controller.getContext();
            string cId = "TCC5";
            string caseName = "Test Case CC5";
            int caseamount = 0; //The amount of cases with the same Id. The test checks that if there are more than 0 cases, and we try to add a case, the amount of cases does not go up

            Doctor loggedDoctor = _context.Doctors.FirstOrDefault(u => u.UserName.ToLower() == "Admin".ToLower());
            Patient linkedPatient = _context.Patients.FirstOrDefault(u => u.UserName.ToLower() == "Admin".ToLower());
            int docId = loggedDoctor.DoctorId;
            int patId = linkedPatient.PatientId;

            //make sure the link is present
            try
            {
                var links = from l in _context.PatientsDoctorss where l.DoctorId == docId select l;
                PatientsDoctors pl = links.FirstOrDefault(u => u.PatientId == patId);
                if (pl == null)
                {
                    pl = new PatientsDoctors()
                    {
                        PatientId = patId,
                        DoctorId = docId
                    };
                    _context.PatientsDoctorss.Add(pl);
                    _context.SaveChanges();
                }
            }
            catch (Exception e)
            {
                Pass = false;
                Aresult = e.ToString();
                model = new TestViewModel()
                {
                    id = Id,
                    time = DateTime.Now,
                    description = Description,
                    steps = Steps,
                    criteria = Criteria,
                    input = Inputstr,
                    aresult = Aresult,
                    eresult = Eresult,
                    pass = Pass
                };
                return model;
            }
            //make sure the case already exists
            try
            {
                var casesq = from c in _context.Cases where c.DoctorId == docId select c;
                var caseswidq = from c in casesq where c.CaseId == cId select c;
                caseamount = caseswidq.Count();
                //Case pl = cases.FirstOrDefault(u => u.CaseId == cId);
                if (caseamount <= 0)
                {
                    Case newcase = new Case()
                    {
                        CaseId = cId,
                        CaseName = caseName,
                        DoctorId = docId,
                        PatientId = patId
                    };
                    _context.Cases.Add(newcase);
                    _context.SaveChanges();
                    casesq = from c in _context.Cases where c.DoctorId == docId select c;
                    caseswidq = from c in casesq where c.CaseId == cId select c;
                    caseamount = caseswidq.Count();
                }
            }
            catch (Exception e)
            {
                Pass = false;
                Aresult = e.ToString();
                model = new TestViewModel()
                {
                    id = Id,
                    time = DateTime.Now,
                    description = Description,
                    steps = Steps,
                    criteria = Criteria,
                    input = Inputstr,
                    aresult = Aresult,
                    eresult = Eresult,
                    pass = Pass
                };
                return model;
            }

            //act
            try
            {
                controller.CreateCase(cId, caseName, patId);
            }
            catch (Exception e)
            {
                Pass = false;
                Aresult = e.ToString();
                model = new TestViewModel()
                {
                    id = Id,
                    time = DateTime.Now,
                    description = Description,
                    steps = Steps,
                    criteria = Criteria,
                    input = Inputstr,
                    aresult = Aresult,
                    eresult = Eresult,
                    pass = Pass
                };
                return model;
            }

            //assert
            var cases = from c in _context.Cases where c.DoctorId == docId select c;
            var caseswid = from c in cases where c.CaseId == cId select c;
            int newcaseamount = caseswid.Count();

            if (newcaseamount < caseamount)
            {
                Aresult = (newcaseamount - caseamount).ToString() + " cases with Id TCC5 have been removed, from " + caseamount + " to " + newcaseamount;
            }
            else if (newcaseamount > caseamount)
            {
                Aresult = (caseamount - newcaseamount).ToString() + " new cases with Id TCC5 have been added, from " + caseamount + " to " + newcaseamount;
            }
            else
            {
                Aresult = "No new cases with Id TCC5 have been added";
            }

            if (Aresult == Eresult)
            {
                Pass = true;
            }
            else
            {
                Pass = false;
            }

            model = new TestViewModel()
            {
                id = Id,
                time = DateTime.Now,
                description = Description,
                steps = Steps,
                criteria = Criteria,
                input = Inputstr,
                aresult = Aresult,
                eresult = Eresult,
                pass = Pass
            };
            return model;
        }
    }
    internal class CreateCaseTest6 : DoctorTest
    {
        public CreateCaseTest6(DoctorController tc)
        {
            testController = tc;
            Id = "D5.Integration.CC6";
            Description = "Create Case test 6";
            Steps = "Make sure the doctor is linked and the case does not already exist, then create the case.";
            Criteria = "The case must not be present before the run phase, but present afterwards.";
            Inputstr = "All fields filled with the a caseId the logged doctor is not using, but a different doctor is.";
            Aresult = "";
            Eresult = "Case with Id " + "TCC6" + " and name " + "Test Case CC6";
        }

        public override TestViewModel Run()
        {
            TestViewModel model;

            //arrange
            bool Pass = false;
            DoctorController controller = testController;
            DatabaseContext _context = controller.getContext();
            string cId = "TCC6";
            string caseName = "Test Case CC6";

            Doctor loggedDoctor = _context.Doctors.FirstOrDefault(u => u.UserName.ToLower() == "Admin".ToLower());
            Doctor otherDoctor = _context.Doctors.FirstOrDefault(u => u.UserName.ToLower() == "Admin2".ToLower());
            Patient linkedPatient = _context.Patients.FirstOrDefault(u => u.UserName.ToLower() == "Admin".ToLower());
            int docId = loggedDoctor.DoctorId;
            int otherId = otherDoctor.DoctorId;
            int patId = linkedPatient.PatientId;

            //make sure the link is present
            try
            {
                var links = from l in _context.PatientsDoctorss where l.DoctorId == docId select l;
                PatientsDoctors pl = links.FirstOrDefault(u => u.PatientId == patId);
                if (pl == null)
                {
                    pl = new PatientsDoctors()
                    {
                        PatientId = patId,
                        DoctorId = docId
                    };
                    _context.PatientsDoctorss.Add(pl);
                    _context.SaveChanges();
                }

                links = from l in _context.PatientsDoctorss where l.DoctorId == otherId select l; //the links from the other doctor
                pl = links.FirstOrDefault(u => u.PatientId == patId);
                if (pl == null)
                {
                    pl = new PatientsDoctors()
                    {
                        PatientId = patId,
                        DoctorId = otherId
                    };
                    _context.PatientsDoctorss.Add(pl);
                    _context.SaveChanges();
                }
            }
            catch (Exception e)
            {
                Pass = false;
                Aresult = e.ToString();
                model = new TestViewModel()
                {
                    id = Id,
                    time = DateTime.Now,
                    description = Description,
                    steps = Steps,
                    criteria = Criteria,
                    input = Inputstr,
                    aresult = Aresult,
                    eresult = Eresult,
                    pass = Pass
                };
                return model;
            }
            //make sure the case does not already exist
            try
            {
                var cases = from c in _context.Cases where c.DoctorId == docId select c;
                Case pl = cases.FirstOrDefault(u => u.CaseId == cId);
                if (pl != null)
                {
                    _context.Cases.Remove(pl);
                    _context.SaveChanges();
                }
            }
            catch (Exception e)
            {
                Pass = false;
                Aresult = e.ToString();
                model = new TestViewModel()
                {
                    id = Id,
                    time = DateTime.Now,
                    description = Description,
                    steps = Steps,
                    criteria = Criteria,
                    input = Inputstr,
                    aresult = Aresult,
                    eresult = Eresult,
                    pass = Pass
                };
                return model;
            }
            //make sure the other doctor does have the case
            try
            {
                var cases = from c in _context.Cases where c.DoctorId == otherId select c;
                Case pl = cases.FirstOrDefault(u => u.CaseId == cId);
                if (pl == null)
                {
                    pl = new Case()
                    {
                        CaseId = cId,
                        CaseName = caseName,
                        DoctorId = otherId,
                        PatientId = patId,
                        CaseInfo = ""
                    };
                    _context.Cases.Add(pl);
                    _context.SaveChanges();
                }
            }
            catch (Exception e)
            {
                Pass = false;
                Aresult = e.ToString();
                model = new TestViewModel()
                {
                    id = Id,
                    time = DateTime.Now,
                    description = Description,
                    steps = Steps,
                    criteria = Criteria,
                    input = Inputstr,
                    aresult = Aresult,
                    eresult = Eresult,
                    pass = Pass
                };
                return model;
            }

            //act
            try
            {
                controller.CreateCase(cId, caseName, patId);
            }
            catch (Exception e)
            {
                Pass = false;
                Aresult = e.ToString();
                model = new TestViewModel()
                {
                    id = Id,
                    time = DateTime.Now,
                    description = Description,
                    steps = Steps,
                    criteria = Criteria,
                    input = Inputstr,
                    aresult = Aresult,
                    eresult = Eresult,
                    pass = Pass
                };
                return model;
            }

            //assert
            var casel = from c in _context.Cases where c.DoctorId == docId select c;
            Case ncase = casel.FirstOrDefault(u => u.CaseId == cId);

            if (ncase == null)
            {
                Aresult = "Case with Id " + cId + " is null";
            }
            else
            {
                Aresult = "Case with Id " + ncase.CaseId + " and name " + ncase.CaseName;
            }

            if (Aresult == Eresult)
            {
                Pass = true;
            }
            else
            {
                Pass = false;
            }

            model = new TestViewModel()
            {
                id = Id,
                time = DateTime.Now,
                description = Description,
                steps = Steps,
                criteria = Criteria,
                input = Inputstr,
                aresult = Aresult,
                eresult = Eresult,
                pass = Pass
            };
            return model;
        }
    } 



    internal class AddMedicinesAlreadyLinkedTest1 : DoctorTest
    {
        public AddMedicinesAlreadyLinkedTest1(DoctorController dc)
        {
            testController = dc;
            Id = "D6.Integration.AMT1";
            Description = "Add medicine when patient and doctor are linked";
            Steps = "Check if they are linked, try to add medecine";
            Criteria = "Pass: Medicine is added | Fail: exeption error, medicine not added";
            Inputstr = "All parameters and patient ID that is linked to doctor";
            Aresult = "";
            Eresult = "Medicine is added";
           

        }

        public override TestViewModel Run()
        {
            TestViewModel model;

            //arrange
            bool Pass = false;
            DatabaseContext Tcontext = testController.getContext();
            DoctorController controller = new DoctorController(testController.getContext());
            Doctor doctor = Tcontext.Doctors.FirstOrDefault();
            int DoctorID = doctor.DoctorId;
            var PatDocs = from d in Tcontext.PatientsDoctorss where d.DoctorId == DoctorID select d;
            PatientsDoctors patdoc = PatDocs.First();

            string name = "Paracetamol";
            DateTime start_date = DateTime.Now;
            DateTime end_date = DateTime.Now;
            int patient_id = 1;
            int amount = 3;
            float mg = 50;

            var medicinebefore = from d in Tcontext.Medicines where d.PatientId == patient_id select d;
            int count = medicinebefore.Count();


            try
            {
                controller.AddMedicines(name, start_date, end_date, amount, patient_id, mg);
            }

            catch (Exception e)
            {
                Pass = false;
                Aresult = e.Message;
                model = new TestViewModel()
                {
                    id = Id,
                    time = DateTime.Now,
                    description = Description,
                    steps = Steps,
                    criteria = Criteria,
                    input = Inputstr,
                    aresult = Aresult,
                    eresult = Eresult,
                    pass = Pass
                };
                return model;
            }

            //assert
            var medicine = from d in Tcontext.Medicines where d.PatientId == patient_id select d;
            int count2 = medicine.Count();
            if (medicine != null)
            {
                Aresult = "Medicine added";
                Pass = true;
            }
            else
            {
                Aresult = "Medicine not added";
                Pass = false;
            }

            model = new TestViewModel()
            {
                id = Id,
                time = DateTime.Now,
                description = Description,
                steps = Steps,
                criteria = Criteria,
                input = Inputstr,
                aresult = Aresult,
                eresult = Eresult,
                pass = Pass
            };
            return model;
        }
    }
    internal class AddMedicinesMissingParameterTest1 : DoctorTest
    {

        public AddMedicinesMissingParameterTest1(DoctorController dc)
        {
            testController = dc;
            Id = "D6.Integration.AMMPT2";
            Description = "Add medicine when missing a parameter";
            Steps = "Check if all parameters are inserted, try to add medecine";
            Criteria = "Pass: Exeption Error: Medicine is not added | Fail: Medicine is added";
            Inputstr = "All parameters and patient ID that is linked to doctor, except one parameter";
            Aresult = "";
            Eresult = "Medicine is not added";


        }

        public override TestViewModel Run()
        {
            TestViewModel model;

            //arrange
            bool Pass = false;
            DatabaseContext Tcontext = testController.getContext();
            DoctorController controller = new DoctorController(testController.getContext());           
            Doctor doctor = Tcontext.Doctors.FirstOrDefault();
            int DoctorID = doctor.DoctorId;
            var PatDocs = from d in Tcontext.PatientsDoctorss where d.DoctorId == DoctorID select d;
            PatientsDoctors patdoc = PatDocs.First();
            string name = null;
            DateTime start_date = DateTime.Now;
            DateTime end_date = DateTime.Now;
            int patient_id = 1;
            int amount = 3;
            float mg = 50;
            var medicinebefore = from d in Tcontext.Medicines where d.PatientId == patient_id select d;
            Medicine med = new Medicine()
            { 
                Name = name,
                DateStart = start_date,
                DateEnd = end_date,
                Amount = amount,
                Mg = mg
            };

            int count = medicinebefore.Count();
            try
            {
                controller.AddMedicines(name, start_date, end_date, amount, patient_id, mg);
            }

            catch (Exception e)
            {
                Pass = true;
                Aresult = "You are missing one or more parameters! " + e.Message;
                model = new TestViewModel()
                {
                    id = Id,
                    time = DateTime.Now,
                    description = Description,
                    steps = Steps,
                    criteria = Criteria,
                    input = Inputstr,
                    aresult = Aresult,
                    eresult = Eresult,
                    pass = Pass
                };
                return model;
            }

            //assert
          
            var medicinePat = from d in Tcontext.Medicines where d.PatientId == patient_id select d;
            bool contains = new bool();
            if (medicinePat.Contains(med))
            {
                contains = true;
            }
            if (!medicinePat.Contains(med))
             {
                contains = false;
            }
          
            if (contains )
            {
                Aresult = "Medicine added";
                Pass = false;
            }
            if (!contains)
            {
                Aresult = "Medicine not added";
                Pass = true;
            }

            model = new TestViewModel()
            {
                id = Id,
                time = DateTime.Now,
                description = Description,
                steps = Steps,
                criteria = Criteria,
                input = Inputstr,
                aresult = Aresult,
                eresult = Eresult,
                pass = Pass
            };
            return model;
        }
    }
    internal class AddMedicinesPatientNullTest1 : DoctorTest
    {

        public AddMedicinesPatientNullTest1(DoctorController dc)
        {
            testController = dc;
            Id = "D6.Integration.AMPNT3";
            Description = "Add medicine to a nonexisting patient";
            Steps = "Check if patient exists, try to add medecine";
            Criteria = "Pass: Exception error: Medicine is not added | Fail: Medicine is added";
            Inputstr = "All parameters and a nonexisting patient ID ";
            Aresult = "";
            Eresult = "Medicine is not added";


        }

        public override TestViewModel Run()
        {
            TestViewModel model;

            //arrange
            bool Pass = false;
            DatabaseContext Tcontext = testController.getContext();
            DoctorController controller = new DoctorController(testController.getContext());
            Doctor doctor = Tcontext.Doctors.FirstOrDefault();
            int DoctorID = doctor.DoctorId;
            var PatDocs = from d in Tcontext.PatientsDoctorss where d.DoctorId == DoctorID select d;
            PatientsDoctors patdoc = PatDocs.First();

            string name = "sertan";
            DateTime start_date = DateTime.Now;
            DateTime end_date = DateTime.Now;
            int patient_id = 9;
            int amount = 3;
            float mg = 50;

            var medicinebefore = from d in Tcontext.Medicines where d.PatientId == patient_id select d;
            int count = medicinebefore.Count();
            Doctor doc = Tcontext.Doctors.FirstOrDefault(x => x.DoctorId == DoctorID);
            Patient patient = Tcontext.Patients.FirstOrDefault(p => p.PatientId == patient_id);

            var patients = from d in Tcontext.Patients select d;
            bool patientExists = true;
            if (patient == null)
            {
                patientExists = false;
            }


            //loops untill patient is found that doesnt exist 
            for (int i = 1; patientExists; i++)
            {
                patient_id = i;
                patient = Tcontext.Patients.FirstOrDefault(p => p.PatientId == patient_id);
                if (patient == null)
                {
                    patientExists = false;
                }
            }


            try
            {
                controller.AddMedicines(name, start_date, end_date, amount, patient_id, mg);
            }

            catch (Exception e)
            {
                Pass = true;
                Aresult = "This patient does not exist" + e.Message;
                model = new TestViewModel()
                {
                    id = Id,
                    time = DateTime.Now,
                    description = Description,
                    steps = Steps,
                    criteria = Criteria,
                    input = Inputstr,
                    aresult = Aresult,
                    eresult = Eresult,
                    pass = Pass
                };
                return model;
            }

            //assert
            var patientsDoctors = from d in Tcontext.PatientsDoctorss where d.PatientId == patient_id select d;
            Patient pat = Tcontext.Patients.FirstOrDefault(x => x.PatientId == patient_id);

            //check if link is made in the database
            //foreach (var item in Medicines)
            //{
            if (patientExists == false)
            {
                Aresult = "Medicine is not added";
                Pass = true;
            }
            //    if (item.DoctorId == doctor_.DoctorId)
            //    {
            //        Aresult = "Link made for Patientid:" + PatientID + " and Doctorid:" + doctor_.DoctorId;
            //        Pass = false;
            //    }
            //}
            else
            {
                Aresult = "Medicine Added";
                Pass = true;
            }
            //}

            //var medicine = from d in Tcontext.Medicines where d.PatientId == patient_id select d;
            //var pat = from m in Tcontext.Patients where m.PatientId == patient_id select m;
            //int count2 = medicine.Count();

            //if (pat == null)
            //{
            //    Aresult = "Medicine is not added";
            //    Pass = false;
            //}
            //else
            //{
            //    Aresult = "Medicine not added";
            //    Pass = true;
            //}

            model = new TestViewModel()
            {
                id = Id,
                time = DateTime.Now,
                description = Description,
                steps = Steps,
                criteria = Criteria,
                input = Inputstr,
                aresult = Aresult,
                eresult = Eresult,
                pass = Pass
            };
            return model;
        }
    }
    internal class AddMedicinesNotAlreadyLinkedTest1 : DoctorTest
    {

        public AddMedicinesNotAlreadyLinkedTest1(DoctorController dc)
        {
            testController = dc;
            Id = "D6.Integration.AMNALT4";
            Description = "Try to add medicine when patient and doctor are not linked";
            Steps = "Check if they are linked, try to add medecine";
            Criteria = "Pass: Exception error: Medicine is not added | medicine  added";
            Inputstr = "All parameters and patient ID that is not linked to doctor";
            Aresult = "";
            Eresult = "Medicine is not added";


        }

        public override TestViewModel Run()
        {
            TestViewModel model;

            //arrange
            bool Pass = false;
            DatabaseContext Tcontext = testController.getContext();
            DoctorController controller = new DoctorController(testController.getContext());
            Doctor doctor = Tcontext.Doctors.FirstOrDefault();
            int DoctorID = doctor.DoctorId;
            var PatDocs = from d in Tcontext.PatientsDoctorss where d.DoctorId == DoctorID select d;
            PatientsDoctors patdoc = PatDocs.First();

            string name = "Paracetamol";
            DateTime start_date = DateTime.Now;
            DateTime end_date = DateTime.Now;
            int patient_id = -2;
            int amount = 3;
            float mg = 50;

            var medicinebefore = from d in Tcontext.Medicines where d.PatientId == patient_id select d;
            int count = medicinebefore.Count();


            try
            {
                controller.AddMedicines(name, start_date, end_date, amount, patient_id, mg);
            }

            catch (Exception e)
            {
                Pass = true;
                Aresult = "This patient is not linked to this doctor!  " + e.Message;
                model = new TestViewModel()
                {
                    id = Id,
                    time = DateTime.Now,
                    description = Description,
                    steps = Steps,
                    criteria = Criteria,
                    input = Inputstr,
                    aresult = Aresult,
                    eresult = Eresult,
                    pass = Pass
                };
                return model;
            }

            //assert

            Patient patient = Tcontext.Patients.FirstOrDefault(y => y.PatientId == patient_id);
            string docName = doctor.FirstName;
            string patName = patient.FirstName;
            PatientsDoctors patientsDoctors_ = Tcontext.PatientsDoctorss.FirstOrDefault(
                p => p.PatientId == patient_id && p.DoctorId == DoctorID
            );
            var medicine = from d in Tcontext.Medicines where d.PatientId == patient_id select d;
            int count2 = medicine.Count();
            bool linkmade = Tcontext.PatientsDoctorss.Contains(patientsDoctors_);

            PatientsDoctors patientsDoctors = new PatientsDoctors()
            {
                PatientId = patient_id,
                DoctorId =DoctorID
            };

            //if (!linkmade)
            //{
            //    Tcontext.PatientsDoctorss.Add(patientsDoctors);
            //    Tcontext.SaveChanges();
            //}

            if (linkmade == true)
            {
                Aresult = "Medicine Added";
                Pass = false;
            }
            else
            {
                Aresult = "Medicine is not added!";
                Pass = true;
            }

            //if (medicine != null)
            //{
            //    Aresult = "Medicine added";
            //    Pass = false;
            //}
            //else
            //{
            //    Aresult = "Medicine not added";
            //    Pass = true;
            //}

            model = new TestViewModel()
            {
                id = Id,
                time = DateTime.Now,
                description = Description,
                steps = Steps,
                criteria = Criteria,
                input = Inputstr,
                aresult = Aresult,
                eresult = Eresult,
                pass = Pass
            };
            return model;
        }
    }
    internal class MakeAppointmentAlreadyLinkedTest2 : DoctorTest
    {
        
        public MakeAppointmentAlreadyLinkedTest2(DoctorController dc)
        {
            testController = dc;
            Id = "D1.Integration.MAT1";
            Description = "Make appointment when patient and doctor are linked";
            Steps = "Check if they are linked, try to make an apointment";
            Criteria = "Pass:Appointment is made| Fail: exeption error, appointment is not added";
            Inputstr = "All parameters and patient ID that is linked to doctor";
            Aresult = "";
            Eresult = "Appointment is made";
        }

        public override TestViewModel Run()
        {
            TestViewModel model;

            //arrange
            bool Pass = false;
            DatabaseContext Tcontext = testController.getContext();
            DoctorController controller = testController;//new DoctorController(testController.getContext());
            
            Doctor user = Tcontext.Doctors.FirstOrDefault(u => u.UserName == "admin");
			int doctorid = user.DoctorId;

            // int PatientID = -1;
            // int DoctorID = -1;

            var PatDocs = from d in Tcontext.PatientsDoctorss select d;
            PatientsDoctors patdoc = PatDocs.First();
            int patientid = patdoc.PatientId;
            //int doctorid = patdoc.DoctorId;
            string caseid = "1";
            DateTime date = DateTime.Now;
            string info = "This is an appointment";
          
            var caseL = from c in Tcontext.Cases where c.CaseId == caseid && c.DoctorId == doctorid select c;
            Case case1 = caseL.FirstOrDefault();
            if (case1 == null)
            {
                case1 = new Case()
                {
                    
                    CaseId = caseid,
                    CaseInfo = "Test case 1",
                    CaseName = "Case name 1",
                    PatientId = patientid,
                    DoctorId = doctorid
                };
                Tcontext.Cases.Add(case1);
                Tcontext.SaveChanges();
            }

            var appointmentbefore = from d in Tcontext.Appointments where d.CaseId == caseid && d.DoctorId == doctorid select d;
            int count = appointmentbefore.Count();


            try
            {
                controller.CreateAppointment( caseid,  date,  info);
            }

            catch (Exception e)
            {
                Pass = false;
                Aresult = e.Message;
                model = new TestViewModel()
                {
                    id = Id,
                    time = DateTime.Now,
                    description = Description,
                    steps = Steps,
                    criteria = Criteria,
                    input = Inputstr,
                    aresult = Aresult,
                    eresult = Eresult,
                    pass = Pass
                };
                return model;
            }

            //assert
            var appointmet = from d in Tcontext.Appointments where d.CaseId == caseid select d;
            int count2 = appointmet.Count();
            if (count2 > count)
            {
                Aresult = "Appointment is made";
                Pass = true;
            }
            else
            {
                Aresult = "Appointment is not made";
                Pass = false;
            }

            model = new TestViewModel()
            {
                id = Id,
                time = DateTime.Now,
                description = Description,
                steps = Steps,
                criteria = Criteria,
                input = Inputstr,
                aresult = Aresult,
                eresult = Eresult,
                pass = Pass
            };
            return model;
        }
    }
    internal class MakeAppointmentMissesParameterTest2 : DoctorTest
    {

        public MakeAppointmentMissesParameterTest2(DoctorController dc)
        {
            testController = dc;
            Id = "D1.Integration.MAT2";
            Description = "Try to make an appointment when misses a parameter";
            Steps = "Check if all parameters are complete, try to make an apointment";
            Criteria = "Pass: Exeption error: Appointment is not made | Fail: Appointment is made";
            Inputstr = "All parameters and patient ID that is linked to doctor";
            Aresult = "";
            Eresult = "Appointment is not made ";
        }

        public override TestViewModel Run()
        {
            TestViewModel model;

            //arrange
            bool Pass = false;
            DatabaseContext Tcontext = testController.getContext();
            DoctorController controller = testController;//new DoctorController(testController.getContext());
            int PatientID = -1 ;
            Doctor user = Tcontext.Doctors.FirstOrDefault(u => u.UserName == "admin");
			int doctorid = user.DoctorId;

            var PatDocs = from d in Tcontext.PatientsDoctorss where d.DoctorId == doctorid select d;
            PatientsDoctors patdoc = PatDocs.FirstOrDefault();
            if (patdoc == null)
            {
                patdoc = new PatientsDoctors()
                {
                    PatientId = PatientID,
                    DoctorId = doctorid
                };
            }
            int patientid = patdoc.PatientId;
            string caseid = null;
            DateTime date = DateTime.Now;
            string info = "This is an appointment";



            var appointmentbefore = from d in Tcontext.Appointments where d.CaseId == caseid select d;
            int count = appointmentbefore.Count();


            try
            {
                controller.CreateAppointment(caseid, date, info);
            }

            catch (Exception e)
            {
                Pass = true;
                Aresult = "Missing a parameter, " + e.Message;
                model = new TestViewModel()
                {
                    id = Id,
                    time = DateTime.Now,
                    description = Description,
                    steps = Steps,
                    criteria = Criteria,
                    input = Inputstr,
                    aresult = Aresult,
                    eresult = Eresult,
                    pass = Pass
                };
                return model;
            }

            //assert
            var appointmet = from d in Tcontext.Appointments where d.CaseId == caseid select d;
            int count2 = appointmet.Count();
            if (count2 > count)
            {
                Aresult = "Appointment is made";
                Pass = false;
            }
            else
            {
                Aresult = "Appointment is not made";
                Pass = true;
            }

            model = new TestViewModel()
            {
                id = Id,
                time = DateTime.Now,
                description = Description,
                steps = Steps,
                criteria = Criteria,
                input = Inputstr,
                aresult = Aresult,
                eresult = Eresult,
                pass = Pass
            };
            return model;
        }
    }
 
    internal class MakeAppointmentNotAlreadyLinkedTest2 : DoctorTest
    {

        public MakeAppointmentNotAlreadyLinkedTest2(DoctorController dc)
        {
            testController = dc;
            Id = "D1.Integration.MAT4";
            Description = "Try to make an appointment with a patient not linked to the doctr";
            Steps = "Check if patient is linked or not, try to make an apointment";
            Criteria = "Pass: Exeption error: Appointment is not made | Fail: Appointment is made";
            Inputstr = "All parameters and patient ID that is linked to doctor";
            Aresult = "";
            Eresult = "Appointment is not made ";
        }

        public override TestViewModel Run()
        {
            TestViewModel model;

            //arrange
            bool Pass = false;
            DatabaseContext Tcontext = testController.getContext();
            DoctorController controller = testController;//new DoctorController(testController.getContext());
            Doctor user = Tcontext.Doctors.FirstOrDefault(u => u.UserName == "admin");
			int DoctorID = user.DoctorId;

            var PatDocs = from d in Tcontext.PatientsDoctorss where d.DoctorId == DoctorID select d;
            PatientsDoctors patdoc = PatDocs.First();
            int patientid = patdoc.PatientId;
            string caseid = null;
            DateTime date = DateTime.Now;
            string info = "This is an appointment";



            var appointmentbefore = from d in Tcontext.Appointments where d.CaseId == caseid select d;
            int count = appointmentbefore.Count();


            try
            {
                controller.CreateAppointment(caseid, date, info);
            }

            catch (Exception e)
            {
                Pass = false;
                Aresult = "There is no link, " + e.Message;
                model = new TestViewModel()
                {
                    id = Id,
                    time = DateTime.Now,
                    description = Description,
                    steps = Steps,
                    criteria = Criteria,
                    input = Inputstr,
                    aresult = Aresult,
                    eresult = Eresult,
                    pass = Pass
                };
                return model;
            }

            //assert
            var appointmet = from d in Tcontext.Appointments where d.CaseId == caseid select d;
            int count2 = appointmet.Count();
            if (count2 > count)
            {
                Aresult = "Appointment is made";
                Pass = false;
            }
            else
            {
                Aresult = "Appointment is not made";
                Pass = true;
            }

            model = new TestViewModel()
            {
                id = Id,
                time = DateTime.Now,
                description = Description,
                steps = Steps,
                criteria = Criteria,
                input = Inputstr,
                aresult = Aresult,
                eresult = Eresult,
                pass = Pass
            };
            return model;
        }
    }
    internal class LinkAlreadyLinkedTest3 : DoctorTest
    {

        public LinkAlreadyLinkedTest3(DoctorController dc)
        {
            testController = dc;
            Id = "D8.Integration.LALT1";
            Description = "Doctor links a patient to another doctor if patient is already linked";
            Steps = "Check if they are linked, try to link patient to another doctor";
            Criteria = "Pass: Patient is linked | Fail: exeption error, patient is not linked";
            Inputstr = "patient ID and new doctor ID";
            Aresult = "";
            Eresult = "Patient and doctor are linked";


        }

        public override TestViewModel Run()
        {
            TestViewModel model;

            //arrange
            bool Pass = false;
            DatabaseContext Tcontext = testController.getContext();
            DoctorController controller = new DoctorController(testController.getContext());         
            int patientid = -1;
            int doctorid = -1;
            Doctor doc = Tcontext.Doctors.FirstOrDefault(y => y.DoctorId == doctorid);
            Patient pat = Tcontext.Patients.FirstOrDefault(y => y.PatientId == patientid);

            try
            {
                controller.SubmitLink(patientid,doctorid);
            }

            catch (Exception e)
            {
                Pass = false;
                Aresult = e.ToString();
                model = new TestViewModel()
                {
                    id = Id,
                    time = DateTime.Now,
                    description = Description,
                    steps = Steps,
                    criteria = Criteria,
                    input = Inputstr,
                    aresult = Aresult,
                    eresult = Eresult,
                    pass = Pass
                };
                return model;
            }

         
            PatientsDoctors patientsDoctors_ = Tcontext.PatientsDoctorss.FirstOrDefault(
                p => p.PatientId == patientid && p.DoctorId == doctorid
            );

            bool linkmade = Tcontext.PatientsDoctorss.Contains(patientsDoctors_);

            PatientsDoctors patientsDoctors = new PatientsDoctors()
            {
                PatientId = patientid,
                DoctorId = doctorid
            };

            if (!linkmade)
            {
                Tcontext.PatientsDoctorss.Add(patientsDoctors);
                Tcontext.SaveChanges();
            }
          
            if (linkmade == true)
            {
                Aresult = "Link is made";
                Pass = true;
            }
            else
            {
                Aresult = "Link is not made";
                Pass = false;
            }

            model = new TestViewModel()
            {
                id = Id,
                time = DateTime.Now,
                description = Description,
                steps = Steps,
                criteria = Criteria,
                input = Inputstr,
                aresult = Aresult,
                eresult = Eresult,
                pass = Pass
            };
            return model;
        }
    }
    internal class LinkDoctorNullTest3 : DoctorTest
    {

        public LinkDoctorNullTest3 (DoctorController dc)
        {
            testController = dc;
            Id = "D8.Integration.LDNT2";
            Description = "Doctor links a patient to a nonexisting doctor";
            Steps = "Check if doctor exists, try to link him to patient";
            Criteria = "Pass: link is not made | Fail: exeption error, link is made";
            Inputstr = "patient ID and new doctor ID";
            Aresult = "";
            Eresult = "Patient and doctor are not linked";


        }

        public override TestViewModel Run()
        {
            TestViewModel model;

            //arrange
            bool Pass = false;
            DatabaseContext Tcontext = testController.getContext();
            DoctorController controller = new DoctorController(testController.getContext());
            int patientid = -1;
            int doctorid = -1000;



            try
            {
                controller.SubmitLink(patientid, doctorid);
            }

            catch (Exception e)
            {
                Pass = false;
                Aresult ="This doctor does not exist, " + e.Message;
                model = new TestViewModel()
                {
                    id = Id,
                    time = DateTime.Now,
                    description = Description,
                    steps = Steps,
                    criteria = Criteria,
                    input = Inputstr,
                    aresult = Aresult,
                    eresult = Eresult,
                    pass = Pass
                };
                return model;
            }

            PatientsDoctors patientsDoctors_ = Tcontext.PatientsDoctorss.FirstOrDefault(
                p => p.PatientId == patientid && p.DoctorId == doctorid
            );

            bool linkmade = Tcontext.PatientsDoctorss.Contains(patientsDoctors_);

            PatientsDoctors patientsDoctors = new PatientsDoctors()
            {
                PatientId = patientid,
                DoctorId = doctorid
            };

            if (!linkmade)
            {
                Aresult = "Link is not made";
                Pass = true;
            }

            if (linkmade)
            {
                Aresult = "Link is made";
                Pass = false;
            }

            model = new TestViewModel()
            {
                id = Id,
                time = DateTime.Now,
                description = Description,
                steps = Steps,
                criteria = Criteria,
                input = Inputstr,
                aresult = Aresult,
                eresult = Eresult,
                pass = Pass
            };
            return model;
        }
    }
    internal class LinkPatientNullTest3 : DoctorTest
    {

        public LinkPatientNullTest3(DoctorController dc)
        {
            testController = dc;
            Id = "D8.Integration.LPNT3";
            Description = "Doctor links a nonexisting patient to another doctor";
            Steps = "Check if Patient exists, try to link him to new doctor";
            Criteria = "Pass: link is not made | Fail: exeption error, link is made";
            Inputstr = "nonexisting patient ID and new doctor ID";
            Aresult = "";
            Eresult = "Patient and doctor are not linked";


        }

        public override TestViewModel Run()
        {
            TestViewModel model;

            //arrange
            bool Pass = false;
            DatabaseContext Tcontext = testController.getContext();
            DoctorController controller = new DoctorController(testController.getContext());
            int patientid = -1000;
            int doctorid = -1;



            try
            {
                controller.SubmitLink(patientid, doctorid);
            }

            catch (Exception e)
            {
                Pass = false;
                Aresult = "This Patient does not exist, " + e.Message;
                model = new TestViewModel()
                {
                    id = Id,
                    time = DateTime.Now,
                    description = Description,
                    steps = Steps,
                    criteria = Criteria,
                    input = Inputstr,
                    aresult = Aresult,
                    eresult = Eresult,
                    pass = Pass
                };
                return model;
            }


            PatientsDoctors patientsDoctors_ = Tcontext.PatientsDoctorss.FirstOrDefault(
                p => p.PatientId == patientid && p.DoctorId == doctorid
            );

            bool linkmade = Tcontext.PatientsDoctorss.Contains(patientsDoctors_);


            if (!linkmade)
            {
                Aresult = "Link is not made";
                Pass = true;
            }

            if (linkmade == true)
            {
                Aresult = "Link is made";
                Pass = false;
            }


            model = new TestViewModel()
            {
                id = Id,
                time = DateTime.Now,
                description = Description,
                steps = Steps,
                criteria = Criteria,
                input = Inputstr,
                aresult = Aresult,
                eresult = Eresult,
                pass = Pass
            };
            return model;
        }
    }
    internal class LinkDocPatsNullTest3 : DoctorTest
    {

        public LinkDocPatsNullTest3(DoctorController dc)
        {
            testController = dc;
            Id = "D8.Integration.LPDNT4";
            Description = "Doctor links a nonexisting patient to a nonexisting doctor";
            Steps = "Check if Patient and doctor exists, try to link them ";
            Criteria = "Pass: link is not made | Fail: exeption error, link is made";
            Inputstr = "nonexisting patient ID and nonexisting doctor ID";
            Aresult = "";
            Eresult = "Patient and doctor are not linked";


        }

        public override TestViewModel Run()
        {
            TestViewModel model;

            //arrange
            bool Pass = false;
            DatabaseContext Tcontext = testController.getContext();
            DoctorController controller = new DoctorController(testController.getContext());
            int patientid = -10000;
            int doctorid = -10000;



            try
            {
                controller.SubmitLink(patientid, doctorid);
            }

            catch (Exception e)
            {
                Pass = false;
                Aresult = "both patient and doctor do not exist, " + e.Message;
                model = new TestViewModel()
                {
                    id = Id,
                    time = DateTime.Now,
                    description = Description,
                    steps = Steps,
                    criteria = Criteria,
                    input = Inputstr,
                    aresult = Aresult,
                    eresult = Eresult,
                    pass = Pass
                };
                return model;
            }



            PatientsDoctors patientsDoctors_ = Tcontext.PatientsDoctorss.FirstOrDefault(
                p => p.PatientId == patientid && p.DoctorId == doctorid
            );

            bool linkmade = Tcontext.PatientsDoctorss.Contains(patientsDoctors_);



            if (!linkmade)
            {
                Aresult = "Link is not made";
                Pass = true;
            }

            if (linkmade == true)
            {
                Aresult = "Link is made";
                Pass = false;
            }



            model = new TestViewModel()
            {
                id = Id,
                time = DateTime.Now,
                description = Description,
                steps = Steps,
                criteria = Criteria,
                input = Inputstr,
                aresult = Aresult,
                eresult = Eresult,
                pass = Pass
            };
            return model;
        }
    }

}