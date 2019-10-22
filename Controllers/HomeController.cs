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
        private DatabaseContext _context;
        public IActionResult Index()
        {
            return View();
        }

    }
    
}
