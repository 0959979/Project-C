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

        //GET: Patient/CreateAccountgit
    //    public IActionResult CreateAccount() => View();

        //GET: Patient/PatientList
        // public IActionResult PatientList() => View();
        public IActionResult CreateAccount(string firstname, string lastname, string email,int phonenumber, string username, string password)
        {
            Patient patient = new Patient()
            {
                FirstName = firstname,
                LastName = lastname,
                Email = email,
                PhoneNumber = phonenumber,
                UserName = username,
                Password = password
                //PatientId = 6
            };
            _context.Patients.Add(patient);
            _context.SaveChanges();

            return View(patient);

        }

        

        //PatientList Page
        public IActionResult PatientList() 
        {
            var patients = from p in _context.Patients select p;

            return View(patients);
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

      //  public ActionResult SubmitPatientAccount()  
      //  {  
            // Patient rec = new Patient (1,"y","s",1,"ysnoek","12345678","y@ello.nl"); 
            // ViewBag.Message = rec;  
       //     return View();  
       // }  
  

        // [HttpPost]
        // public FileStreamResult test (Patient patient)
        // {
        //     string name = patient.FirstName;
        //     string lastname = patient.LastName;

        // }


    }
    }