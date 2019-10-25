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

        public IActionResult SubmitDoctorAccount(string firstname, string lastname, string email, int phonenumber, string specialism, string username, string password)
        {
            if (username != null && password != null)
            {
    bool valid = true;
            {
                Doctor user = _context.Doctors.FirstOrDefault(u => u.Email == email);
                if (user != null)
                {
                    ViewBag.emptyfield1 = "this E-mail is already in use";
                    valid = false;
                }
            }

            {
                Doctor user = _context.Doctors.FirstOrDefault(u => u.UserName == username);
                if (user != null)
                {
                    ViewBag.emptyfield3 = "this username is already in use";
                    valid = false;
                }
            }
            if (!valid)
            {
                return View("CreateAccount"); //moet de data in de fields nog bewaren?
            }
                    Doctor doctor = new Doctor()
                    {
                        FirstName = firstname,
                        LastName = lastname,
                        Email = email,
                        PhoneNumber = phonenumber,
                        Specialism = specialism,
                        UserName = username,
                        Password = Program.Hash256bits(password),
                        Messages = new List<string>(),
                        PatientIds = new List<int>()
                    };
                    _context.Doctors.Add(doctor);
                    _context.SaveChanges();
    
                    ViewData["FirstName"] = doctor.FirstName;
                    ViewData["LastName"] = doctor.LastName;

                    return RedirectToAction("SubmitDoctorAccount", "Doctor");
                
            }
            
            return View();
        }
       
        // public IActionResult SubmitDoctorAccount()
        // {
        //     string firstname = TempData["MyTempData"].ToString();
        //     ViewData["FirstName"] = firstname;
        //     //ViewData["LastName"] = lastname;

        //     return View("SubmitDoctorAccount");

        // }

        //Doctorlist Page
        //Authorizes the page so only users with the role Doctor can view it
        [Authorize(Roles = "Admin")]
        public IActionResult DoctorList()
        {
            var doctors = from d in _context.Doctors select d;

            return View(doctors);
        }


        // public ActionResult Login(string username, string password, bool staylogged)
        // {
        //     //string Username = username;
        //     string Password = password;
        //     var UserL = from u in _context.Patients where u.UserName == username select u;
        //     if (username != null)
        //     {
        //         username = username.ToLower();
        //         Doctor user = _context.Doctors.FirstOrDefault(u => u.UserName.ToLower() == username);
        //         if (user != null)
        //         {
        //             string pwhash = Program.Hash256bits(password);
        //             if (user.Password == pwhash)                  
        //              {
        //                 var claims = new List<Claim>
        //             {
        //                 new Claim(ClaimTypes.Name, "Doctor", ClaimValueTypes.String),
        //                 new Claim(ClaimTypes.NameIdentifier, user.UserName.ToString(), ClaimValueTypes.String),
        //                 new Claim(ClaimTypes.Role, "Doctor", ClaimValueTypes.String)
        //             };
        //                 var userIdentity = new ClaimsIdentity(claims, "SecureLogin");
        //                 var userPrincipal = new ClaimsPrincipal(userIdentity);
        //              }
        //         }
        //         }    
                     
            
        //     public ActionResult Login(string username, string password, bool staylogged)
        // {
        //     //string Username = username;
        //     //string Password = password;
        //     //var UserL = from u in _context.Patients where u.UserName == Username select u;
        //     if (username != null)
        //     {
        //         username = username.ToLower();
        //         Doctor user = _context.Doctors.FirstOrDefault(u => u.UserName.ToLower() == username);
        //         if (user != null)

        //         {
        //             string pwhash = Program.Hash256bits(password);
        //             if (user.Password == pwhash)                  
        //              {
        //                 var claims = new List<Claim>
        //             {
        //                 new Claim(ClaimTypes.Name, "Doctor", ClaimValueTypes.String),
        //                 new Claim(ClaimTypes.NameIdentifier, user.UserName.ToString(), ClaimValueTypes.String),
        //                 new Claim(ClaimTypes.Role, "Doctor", ClaimValueTypes.String)
        //             };
        //                 var userIdentity = new ClaimsIdentity(claims, "SecureLogin");
        //                 var userPrincipal = new ClaimsPrincipal(userIdentity);

        //                 HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
        //                     userPrincipal,
        //                     new AuthenticationProperties
        //                     {
        //                         ExpiresUtc = DateTime.UtcNow.AddMinutes(30),
        //                         IsPersistent = true,
        //                         AllowRefresh = false
        //                     });
        //                 HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
        //                     userPrincipal,
        //                     new AuthenticationProperties
        //                     {
        //                         ExpiresUtc = DateTime.UtcNow.AddMinutes(30),
        //                         IsPersistent = true,
        //                         AllowRefresh = false
        //                     });

        //                 return RedirectToAction("Profile", "Doctor");
        //             }
        //             else
        //             {
        //                 ViewBag.emptyfield = "Username or Password is incorrect";
        //             }
        //         }
        //         else
        //         {
        //             ViewBag.emptyfield = "Username or Password is incorrect";
        //         }
        //     }
        //     return View();
        // }
        //                 return RedirectToAction("Profile", "Doctor");
        //             }
        //             else
        //             {
        //                 ViewBag.emptyfield = "Username or Password is incorrect";
        //             }
        //         }
        //         else
        //         {
        //             ViewBag.emptyfield = "Username or Password is incorrect";
        //         }
        //     }
        //     return View();
        // }


              public ActionResult Message(string sendto, string message) //Send a message to a doctor
        {
            //string Sendto = sendto; //recipient name
            //string Message = message;
            Doctor user = _context.Doctors.FirstOrDefault(u => u.UserName == sendto);
            if (user != null)
            {
                if (message != null && message != "")
                {

                    //add the Message to the List<string> of messages
                    if (user.Messages == null)
                    {
                        user.Messages = new List<string> { };
                    }
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
			var username = User.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value;
			ViewBag.username = username;
			var user = _context.Doctors.FirstOrDefault(u => u.UserName == username);
			string email = user.Email.ToString();
			ViewBag.email = email;
			var specialism = user.Specialism.ToString();
			ViewBag.specialism = specialism;
			var phonenumber = user.PhoneNumber;
			ViewBag.phonenumber = phonenumber;
			var firstname = user.FirstName.ToString();
			ViewBag.firstname = firstname;
			var lastname = user.LastName.ToString();
			ViewBag.lastname = lastname;
			return View();
        }
		public IActionResult UpdateAccount(string firstname, string lastname, string email, int phonenumber, string specialism)
		{
			if (firstname != null)
			{
				var USERNAME = User.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value;
				var USER = _context.Doctors.FirstOrDefault(u => u.UserName == USERNAME);
				USER.FirstName = firstname;
				USER.LastName = lastname;
				USER.Email = email;
				USER.PhoneNumber = phonenumber;
				USER.Specialism = specialism;
				_context.SaveChanges();
				return RedirectToAction("Profile", "Doctor");
			}
			return View();
		}
		public IActionResult UpdateDoctorAccount()
		{
			string firstname = TempData["MyTempData"].ToString();
			ViewData["FirstName"] = firstname;
			return View();
		}
}
}