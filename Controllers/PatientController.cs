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
            Patient patient = new Patient()
            {
                FirstName = firstname,
                LastName = lastname,
                Email = email,
                PhoneNumber = phonenumber,
                UserName = username,
                Password = password
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



        



    }
    }