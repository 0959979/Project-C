using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace zorgapp.Controllers
{
    public class ProfilePageController : Controller
    {
        public string Index()
        {

            return "hoi";
        }
        public IActionResult Profile()
        {

            return View();
        }
    }
}