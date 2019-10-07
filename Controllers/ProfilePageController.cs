using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using zorgapp.Models;

namespace zorgapp.Controllers
{
    public class ProfilePageController : Controller
    {
        private readonly DatabaseContext _context;

        public ProfilePageController(DatabaseContext context)
        {
            _context = context;
        }
        public IActionResult DoctorProfile()
        {
            var doctors = from p in _context.Doctors select p;

            return View(doctors);
        }
    }
}