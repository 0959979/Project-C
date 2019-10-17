using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using zorgapp.Models;

namespace zorgapp.Controllers{

    public class PatientController : Controller{
        private readonly DatabaseContext _context;

        public PatientController(DatabaseContext context)
        {
            _context = context;
        }
        public IActionResult CreateAccount() => View();

        [Route("Patient/SubmitPatientAccount")]
        public IActionResult SubmitPatientAccount(string firstname, string lastname, string email,int phonenumber, string username, string password)
        {
            bool valid = true;
            {
                Patient user = _context.Patients.FirstOrDefault(u => u.Email == email);
                if (user != null)
                {
                    ViewBag.emptyfield1 = "this E-mail is already in use";
                    valid = false;
                }
            }
            /*{
                Patient user = _context.Patients.FirstOrDefault(u => u.PhoneNumber == phonenumber);
                if (user != null)
                {
                    ViewBag.emptyfield2 = "this phone number is already in use";
                    valid = false;
                }
            }*/
            {
                Patient user = _context.Patients.FirstOrDefault(u => u.UserName == username);
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
            Patient patient = new Patient()
            {
                FirstName = firstname,
                LastName = lastname,
                Email = email,
                PhoneNumber = phonenumber,
                UserName = username,
                Password = Program.Hash256bits(password),
                Messages = new List<string> { }
            };
                _context.Patients.Add(patient);
                _context.SaveChanges();


                ViewData["FirstName"] = patient.FirstName;
                ViewData["LastName"] = patient.LastName;


                return View("SubmitPatientAccount");

        }

        

        //PatientList Page
        public IActionResult PatientList() 
        {
            var patients = from p in _context.Patients select p;

            return View(patients);
        }



        


        public ActionResult Login(string username, string password)
        {
            //string Username = username;
            //string Password = password;
            //var UserL = from u in _context.Patients where u.UserName == Username select u;
            Patient user = _context.Patients.FirstOrDefault(u => u.UserName == username);
            if (user != null)
            {
                string pwhash = Program.Hash256bits(password);
                if (user.Password == pwhash) //password is hashed in the db, so no need to hash again.
                {
                    return RedirectToAction("Profile", "Patient");
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


        public ActionResult Message(string sendto, string message) //Send a message to a patient
        {
            //string Sendto = sendto; //recipient name
            //string Message = message;
            Patient user = _context.Patients.FirstOrDefault(u => u.UserName == sendto);
            if (user != null)
            {
                if (message != null && message != "")
                {
                    //_context.Patients.Update(user); niet nodig
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
                ViewBag.emptyfield = "User not found";
            }
            return View();
        }
        public ActionResult Profile()
        {
            return View();
        }

    }
    }