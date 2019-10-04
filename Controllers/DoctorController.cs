using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using zorgapp.Models;

namespace zorgapp.Controllers
{

    public class DoctorController : Controller
    {
        private readonly DatabaseContext _context;

        public DoctorController(DatabaseContext context)
        {
            _context = context;
        }

        //GET: Patient/CreateAccount
        public IActionResult CreateAccount() => View();



        //PatientList Page
        public IActionResult DoctorList()
        {
            var doctors = from p in _context.Doctors select p;

            return View(doctors);
        }



        // public ActionResult SubmitPatientAccount(){
        //     return View();
        // }

        // [HttpPost]
        // public ActionResult SubmitPatientAccount(string firstname, string lastname){

        //         ViewData["FirstName"] = firstname;
        //         ViewData["LastName"] = lastname;


        //     return View();
        // }


        // [HttpPost]
        // public ActionResult SubmitPatientAccount(){
        //     ViewBag.name = "test";
        //     return View();
        // }

        public ActionResult SubmitDoctorAccount()
        {
            // Patient rec = new Patient (1,"y","s",1,"ysnoek","12345678","y@ello.nl"); 
            // ViewBag.Message = rec;  
            return View();
        }


        // [HttpPost]
        // public FileStreamResult test (Patient patient)
        // {
        //     string name = patient.FirstName;
        //     string lastname = patient.LastName;

        // }

        public ActionResult Login(string username, string password)
        {
            string Username = username;
            string Password = password;
            //var UserL = from u in _context.Patients where u.UserName == Username select u;
            Doctor user = _context.Doctors.FirstOrDefault(u => u.UserName == username);
            if (user != null)
            {
                if (user.Password == Password)
                {
                    return RedirectToAction("Profile", "Doctor");
                }
                else
                {
                    ViewBag.emptyfield = "Username or Password is incorrect";
                }
            }
            else if (Username != null)
            {
                ViewBag.emptyfield = "Username or Password is incorrect";
            }
            return View();
        }

        public ActionResult Profile()
        {
            return View();
        }

        public ActionResult Message(string sendto, string message) //Send a message to a doctor
        {
            string Sendto = sendto; //recipient name
            string Message = message;
            Doctor user = _context.Doctors.FirstOrDefault(u => u.UserName == Sendto);
            if (user != null)
            {
                //add the Message to the List<string> of messages
                user.Messages.Add(Message);
                //send the new List<string> into the Database

            }
            return View();
        }
    }
}