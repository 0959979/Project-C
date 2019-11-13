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
                        FirstName = firstname,
                        LastName = lastname,
                        Email = email,
                        PhoneNumber = phonenumber,
                        Specialism = specialism,
                        UserName = username,
                        Password = Program.Hash256bits(password),
                        PatientIds = new List<int>()
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

       
        public ActionResult CreateCase(string caseid, string casename, int patientid)
        {
            if (caseid != null)
            {
                Doctor user = _context.Doctors.FirstOrDefault(u => u.UserName == User.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value);
                int doctorid = user.DoctorId;
                Case newcase = new Case()
                {
                    Id = caseid,
                    Name = casename,
                    DoctorId = doctorid,
                    PatientId = patientid
                };
                _context.Cases.Add(newcase);
                _context.SaveChanges();

                return RedirectToAction("Profile", "Doctor");
            }

            return View();
        }

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
        public ActionResult Message(string reciever, string subject, string text)
        {
            Patient patient = _context.Patients.FirstOrDefault(u => u.UserName == reciever);
            if (patient != null)
            {
                if (text != null && text != "")
                {
                    var username = User.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value;
                    Doctor doctor = _context.Doctors.FirstOrDefault(u => u.UserName == username);
                    Message message = new Message()
                    {
                        Sender = username,
                        Reciever = reciever,
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
            var check = from m in _context.Messages where m.DoctorToPatient == false && m.Reciever == User.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value select m;
            if (message != check)
            {
                message = from m in _context.Messages where m.DoctorToPatient == false && m.Reciever == User.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value orderby m.Date descending select m;
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
				USER.PhoneNumber = phonenumber;
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