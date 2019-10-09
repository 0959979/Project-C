using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using zorgapp.Models;

namespace zorgapp.Controllers{

    public class DoctorController : Controller{
        private readonly DatabaseContext _context;

        public DoctorController(DatabaseContext context)
        {
            _context = context;
        }

      public IActionResult CreateAccount() => View();
       
        public IActionResult SubmitDoctorAccount(string firstname, string lastname, string email,int phonenumber,string specialism, string username, string password)
        {
            Doctor doctor = new Doctor()
            {
                FirstName = firstname,
                LastName = lastname,
                Email = email,
                PhoneNumber = phonenumber,
                Specialism = specialism,
                UserName = username,
                Password = password
            };
            _context.Doctors.Add(doctor);
            _context.SaveChanges();

            ViewData["FirstName"] = doctor.FirstName;
            ViewData["LastName"] = doctor.LastName;


            return View("SubmitDoctorAccount");

        }

        //Doctorlist Page
        //Authorizes the page so only users with the role Doctor can view it
        [Authorize(Roles = "Doctor")]
        public IActionResult DoctorList()
        {
            var doctors = from d in _context.Doctors select d;

            return View(doctors);
        }


        public ActionResult Login(string username, string password)
        {
            //string Username = username;
            //string Password = password;
            //var UserL = from u in _context.Patients where u.UserName == Username select u;
            Doctor user = _context.Doctors.FirstOrDefault(u => u.UserName == username);
            if (user != null)
            {
                if (user.Password == password)
                {
                    //Creates a new Identity of the user
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, "Doctor", ClaimValueTypes.String),
                        new Claim(ClaimTypes.NameIdentifier, user.DoctorId.ToString(), ClaimValueTypes.String),
                        new Claim(ClaimTypes.Role, "Doctor", ClaimValueTypes.String)
                    };
                    var userIdentity = new ClaimsIdentity(claims, "SecureLogin");
                    var userPrincipal = new ClaimsPrincipal(userIdentity);

                    HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                        userPrincipal,
                        new AuthenticationProperties
                        {
                            ExpiresUtc = DateTime.UtcNow.AddMinutes(30),
                            IsPersistent = true,
                            AllowRefresh = false
                        });

                    return RedirectToAction("Profile", "Doctor");
                }
                else
                {
                    ViewBag.emptyfield = "Username or Password is incorrect";
                }
            }
            else if (username != null)
            {
                ViewBag.emptyfield = "Username or Password is incorrect";
            }
            return View();
        }


        public ActionResult Message(string sendto, string message) //Send a message to a doctor
        {
            //string Sendto = sendto; //recipient name
            //string Message = message;
            Doctor user = _context.Doctors.FirstOrDefault(u => u.UserName == sendto);
            if (user != null)
            {
                if (message != null && message != "")
                {
                    //mark for updating, is dit nodig? idk. blijkbaar niet
                    //add the Message to the List<string> of messages
                    user.Messages.Add(message);
                    //send the new List<string> into the Database
                    _context.SaveChanges();
                }
                else
                {
                    ViewBag.emptyfield = "You need to type in a message to send it.";
                }
            }
            else if (sendto != null)
            {
                ViewBag.emptyfield = "User not found.";
            }
            return View();
        }
        public ActionResult Profile()
        {
            //Gets the username of the logged in user and sends it to the view
            ViewBag.username = User.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value;

            return View();
        }
    }
}
