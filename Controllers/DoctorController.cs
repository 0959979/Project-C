using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using zorgapp.Models;
using zorgapp.ViewModels;

namespace zorgapp.Controllers{

    public class DoctorController : Controller{
        private readonly DatabaseContext _context;

        public DoctorController(DatabaseContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "Admin")]
        public IActionResult CreateAccount() => View();

        [Authorize(Roles = "Admin")]
        public IActionResult SubmitDoctorAccount(string firstname, string lastname, string email, int phonenumber, string specialism, string username, string password)
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
                        LocalId = "local",//DIT NIET MERGEN, IS TIJDELIJK
                        FirstName = firstname,
                        LastName = lastname,
                        Email = email,
                        PhoneNumber = phonenumber.ToString(),//DIT NIET MERGEN, IS TIJDELIJK
                        Specialism = specialism,
                        UserName = username,
                        Password = Program.Hash256bits(password),
                    };
                    _context.Doctors.Add(doctor);
                    _context.SaveChanges();
    
                    ViewData["FirstName"] = doctor.FirstName;
                    ViewData["LastName"] = doctor.LastName;

                    return View();
                
            }          
            return View();
        }
       
        // public IActionResult SubmitDoctorAccount()
        // {
        //     string firstname = TempData["MyTempData"].ToString();
        //     ViewData["FirstName"] = firstname;
        //     //ViewData["LastName"] = lastname;

        //     return View("SubmitDoctorAccount");

        // }

        //Doctorlist Page
        //Authorizes the page so only users with the role Doctor can view it
        [Authorize(Roles = "Admin")]
        public IActionResult DoctorList()
        {
            var doctors = from d in _context.Doctors select d;

            return View(doctors);
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

                return RedirectToAction("Profile", "Doctor");
            }

            return View();
        }
        [Authorize(Roles="Doctor")]
        public ActionResult Agenda (string Previous, int dayoffset, string Next)
        {
            {
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
                else
                {
                    //ViewBag.Recieved = "None";
                }
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
                    for(int h = 6;h<=24;h++)
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
                { //al door Pelle geschreven
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

        private List<Appointment> FilterWeek(List<Appointment> List, DateTime dateTime, int Days)
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

        [Authorize(Roles = "Doctor")]
        public ActionResult CreateAppointment(string caseid, string info)
        {
            if (caseid != null)
            {               
                Appointment appointment = new Appointment()
                {
                    CaseId = caseid,
                    Date = DateTime.Now,
                    Info = info
                };
                _context.Appointments.Add(appointment);
                _context.SaveChanges();

                return RedirectToAction("Profile", "Doctor");
            }
            return View();
        }

        [Authorize(Roles = "Doctor")]
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

        [Authorize(Roles = "Doctor")]
        public ActionResult Message(string sendto, string subject, string message) //Send a message to a doctor
        {
            //string Sendto = sendto; //recipient name
            //string Message = message;
            Patient user = _context.Patients.FirstOrDefault(u => u.UserName == sendto);
            if (user != null)
            {
                if (message != null && message != "")
                {
                    //mark for updating, is dit nodig? idk. blijkbaar niet
                    //add the Message to the List<string> of messages
                    var username = User.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value;
                    //_context.Patients.Update(user); niet nodig
                    //add the Message to the List<string> of messages
                    //user.Messages.Add(username);
                    //user.Messages.Add(subject);
                    //user.Messages.Add(message);
                    //send the new List<string> into the Database
                    _context.SaveChanges();
                    return RedirectToAction("MessageSend", "Doctor");
                }
                else
                {
                    ViewBag.emptyfield = "You need to type in a message to send it.";
                }
            }
            else if (sendto != null)
            {
                ViewBag.emptyfield = "User not found.";
            }
            return View();
        }

        [Authorize(Roles = "Doctor")]
        public ActionResult Inbox()
        {

            Doctor user = _context.Doctors.FirstOrDefault(u => u.UserName == User.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value);
            ViewBag.message = "";//user.Messages;
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
        public IActionResult UpdateAccount(string firstname, string lastname, string email, int phonenumber, string specialism)
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
    }
}