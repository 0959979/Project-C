using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using zorgapp.Models;

namespace zorgapp.Controllers{

    public class PatientController : Controller{

        //GET: Patient/CreateAccount
        public IActionResult CreateAccount() => View();

        // [HttpPost]
        // public FileStreamResult test (Patient patient)
        // {
        //     string name = patient.FirstName;
        //     string lastname = patient.LastName;

        // }

        // public ActionResult CreatePatientAccount(Patient patientView)
        // {
        //     Patient patient = new Patient(1,patientView.FirstName,patientView.LastName, patientView.PhoneNumber,patientView.UserName,patientView.Password,patientView.Email);
        //     return View();

        // }





    }
    }


