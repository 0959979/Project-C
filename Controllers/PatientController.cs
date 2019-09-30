using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using zorgapp.Models;

namespace zorgapp.Controllers{

    

    public class PatientController : Controller{

        private readonly DatabaseContext _context;

        public PatientController(DatabaseContext context)
        {
            _context = context;
        }

        //GET: Patient/CreateAccount
        public IActionResult CreateAccount() => View();

        //PatientList Page
        public IActionResult PatientList()
        {
            var patients = from p in _context.Patients select p;

            return View(patients);
        }
    }
}