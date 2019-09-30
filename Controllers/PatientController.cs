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

        //GET: Patient/CreateAccount
        public IActionResult CreateAccount() => View();

        

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

        public ActionResult SubmitPatientAccount()  
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


    }
    }