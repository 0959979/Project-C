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
                return View("CreateAccount"); //moet de data in de fields nog bewaren?
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
                        Messages = new List<string>(),
                        PatientIds = new List<int>()
                    };
                    _context.Doctors.Add(doctor);
                    _context.SaveChanges();
    
                    ViewData["FirstName"] = doctor.FirstName;
                    ViewData["LastName"] = doctor.LastName;

                    return View("SubmitDoctorAccount");
                
            }          
            return View();
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
            int patientid_ = patientid;
            int doctorid_ = doctorid;
            string docName = doctor.FirstName;
            string patName = patient.FirstName;

            bool DocContains = doctor.PatientIds.Contains(patientid_);
            bool PatContains = patient.DoctorIds.Contains(doctorid_);

            if(!DocContains){
                doctor.PatientIds.Add(patientid_);
                patient.DoctorIds.Add(doctorid_);
                _context.SaveChanges();
            }
            else if(DocContains){
                TempData["message"] = "Link has already been made";
                return RedirectToAction("Link","Admin");
            }

            ViewData["Doctor"] = docName;
            ViewData["Patient"] = patName;

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
                    user.Messages.Add(username);
                    user.Messages.Add(subject);
                    user.Messages.Add(message);
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
            ViewBag.message = user.Messages;
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