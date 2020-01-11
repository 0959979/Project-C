using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using zorgapp.Models;

namespace zorgapp.Controllers
{
    public class HomeController : Controller
    
    {
        private readonly DatabaseContext _context;

        public HomeController(DatabaseContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            Doctor doc = _context.Doctors.FirstOrDefault(u => u.DoctorId == -1);
            if (doc.LocalId == null)
            {
                doc.LocalId = new List<String>();
            }
            Doctor doc2 = _context.Doctors.FirstOrDefault(u => u.DoctorId == -2);
            if (doc2.LocalId == null)
            {
                doc2.LocalId = new List<String>();
            }
            Doctor doc3 = _context.Doctors.FirstOrDefault(u => u.DoctorId == -3);
            if (doc3.LocalId == null)
            {
                doc3.LocalId = new List<String>();
            }

            Patient pat = _context.Patients.FirstOrDefault(u => u.PatientId == -1);
            if (pat.LocalId == null)
            {
                pat.LocalId = new List<String>();
            }

            Patient pat2 = _context.Patients.FirstOrDefault(u => u.PatientId == -2);
            if (pat2.LocalId == null)
            {
                pat2.LocalId = new List<String>();
            }


            Patient pat3 = _context.Patients.FirstOrDefault(u => u.PatientId == -3);
            if (pat3.LocalId == null)
            {
                pat3.LocalId = new List<String>();
            }
            _context.SaveChanges();
            return View();
        }

    }
    
}
