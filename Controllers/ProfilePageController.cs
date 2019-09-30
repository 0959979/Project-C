using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace zorgapp.Controllers
{
    public class ProfilePageController : Controller
    {
        public IActionResult Index()
        {

            return View();
        }
        public IActionResult Profile()
        {
            ViewData["User"] = "Doctor";

            return View();
        }
    }
}