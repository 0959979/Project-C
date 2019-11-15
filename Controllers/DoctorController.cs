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
                    Email = email,
                    PhoneNumber = phonenumber,
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
            if (!valid)
            {
                return View("CreateAccount"); //moet de data in de fields nog bewaren?
            }
                    Doctor doctor = new Doctor()
                    {
                        FirstName = firstname,
                        LastName = lastname,
                        Email = email,
                        PhoneNumber = phonenumber,
                        Specialism = specialism,
                        LocalId = new List<string>(),
                        UserName = username,
                        Password = Program.Hash256bits(password),
                    };
                    doctor.LocalId.Add(localid);
                    _context.Doctors.Add(doctor);
                    _context.SaveChanges();
    
                    ViewData["FirstName"] = doctor.FirstName;
                    ViewData["LastName"] = doctor.LastName;

                    return View("SubmitDoctorAccount");
                
            }          
            return View();
        }
        
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



        // public IActionResult AddLocalId (int patientid ){
        //   //  int id = form["patientid"];
        // //    int id_ = int.Parse(id);
        //     Patient patient = _context.Patients.FirstOrDefault(u => u.PatientId == patientid);
        //   //  string localid = form["localid"];
        //     string localid = "hey";
        //     patient.LocalId.Add(localid);
        //     _context.SaveChanges();
            
        //     return View("PatProfile");

        // }


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