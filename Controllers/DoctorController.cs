using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using zorgapp.Models;

namespace zorgapp.Controllers{

    public class DoctorController : Controller{
        private readonly DatabaseContext _context;

        public DoctorController(DatabaseContext context)
        {
            _context = context;
        }

      public IActionResult CreateAccount() => View();
       
        public IActionResult SubmitDoctorAccount(string firstname, string lastname, string email,int phonenumber,string specialism, string username, string password)
        {
            Doctor doctor = new Doctor()
            {
                FirstName = firstname,
                LastName = lastname,
                Email = email,
                PhoneNumber = phonenumber,
                Specialism = specialism,
                Username = username,
                Password = Program.Hash256bits(password)
            };
            _context.Doctors.Add(doctor);
            _context.SaveChanges();

            ViewData["FirstName"] = doctor.FirstName;
            ViewData["LastName"] = doctor.LastName;


            return View("SubmitDoctorAccount");

        }

        //Doctorlist Page
        public IActionResult DoctorList()
        {
            var doctors = from p in _context.Doctors select p;

            return View(doctors);
        }


        public ActionResult Login(string username, string password)
        {
            //string Username = username;
            //string Password = password;
            //var UserL = from u in _context.Patients where u.UserName == Username select u;
            Doctor user = _context.Doctors.FirstOrDefault(u => u.Username == username);
            if (user != null)
            {
                string pwhash = Program.Hash256bits(password);
                if (user.Password == pwhash) //password is hashed in the db, so no need to hash again.
                {
                    return RedirectToAction("Profile", "Doctor");
                }
                else
                {
                    ViewBag.emptyfield = "Username or Password is incorrect";
                }
            }
            else if (username != null)
            {
                ViewBag.emptyfield = "Username or Password is incorrect";
            }
            return View();
        }


        public ActionResult Message(string sendto, string message) //Send a message to a doctor
        {
            //string Sendto = sendto; //recipient name
            //string Message = message;
            Doctor user = _context.Doctors.FirstOrDefault(u => u.Username == sendto);
            if (user != null)
            {
                if (message != null && message != "")
                {
                    //mark for updating, is dit nodig? idk. blijkbaar niet
                    //add the Message to the List<string> of messages
                    user.Messages.Add(message);
                    //send the new List<string> into the Database
                    _context.SaveChanges();
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
        public ActionResult Profile()
        {
            return View();
        }
    }
}
