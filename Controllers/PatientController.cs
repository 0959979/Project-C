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

    public class PatientController : Controller{
        private readonly DatabaseContext _context;

        public PatientController(DatabaseContext context)
        {
            _context = context;
        }


        public IActionResult CreateAccount()
        {           
            return View();
        }


        [Route("Patient/SubmitPatientAccount")]
        public IActionResult SubmitPatientAccount(string firstname, string lastname, string email,int phonenumber, string username, string password)
        {          
            bool valid = true;
            {
                Patient user = _context.Patients.FirstOrDefault(u => u.Email == email);
                if (user != null)
                {
                    ViewBag.emptyfield1 = "this E-mail is already in use";
                    valid = false;
                }
            }
            {
                Patient user = _context.Patients.FirstOrDefault(u => u.UserName == username);
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
            
                Patient patient = new Patient()
                {
                    FirstName = firstname,
                    LocalId = "LocalPat",
                    LastName = lastname,
                    Email = email,
                    PhoneNumber = phonenumber.ToString(),//DIT NIET MERGEN, IS TIJDELIJK
                    UserName = username.ToLower(),
                    Password = Program.Hash256bits(password),
                    //Messages = new List<string>(),//DIT NIET MERGEN, IS TIJDELIJK
                    //DoctorIds = new List<int>()//DIT NIET MERGEN, IS TIJDELIJK
                };
                _context.Patients.Add(patient);
                _context.SaveChanges();

                ViewData["FirstName"] = patient.FirstName;
                ViewData["LastName"] = patient.LastName;
            
            return View("SubmitPatientAccount");
        }

        
        //PatientList Page
        //Authorizes the page so only users with the role Patient can view it
        [Authorize(Roles = "Admin")]
        public IActionResult PatientList() 
        {
            var patients = from p in _context.Patients select p;

            return View(patients);
        }


        public ActionResult Login(string username, string password, string type)
        {
            //string Username = username;
            //string Password = password;
            //var UserL = from u in _context.Patients where u.UserName == Username select u;   
            
            var adminexists = _context.Admins.Any(x => x.UserName == "admin");
            if(!adminexists){
            Admin admin = new Admin(){
                UserName = "admin",
                Password = Program.Hash256bits("password")
            };

            _context.Admins.Add(admin);
            _context.SaveChanges();
            }

                     
            if (username != null && password != null)
            {    string pwhash = Program.Hash256bits(password);
                if (type == null)
                {
                    ViewBag.emptyfield = "Please select an account type";
                }
                else
                {
                    if (type == "patient")
                    {

                        Patient user = _context.Patients.FirstOrDefault(u => u.UserName == username);
                        if (user != null)
                        {
                                  if (user.Password == pwhash) 
                            {
                                //Creates a new Identity of the user
                                var claims = new List<Claim>
                                {
                                    new Claim(ClaimTypes.Name, "Patient", ClaimValueTypes.String),
                                    new Claim(ClaimTypes.NameIdentifier, user.UserName.ToString(), ClaimValueTypes.String),
                                    new Claim(ClaimTypes.Role, "Patient", ClaimValueTypes.String)
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

                                return RedirectToAction("Profile", "Patient");
                            }
                            else
                            {
                                ViewBag.emptyfield = "Username or Password is incorrect";
                            }
                        }
                        else
                        {
                            ViewBag.emptyfield = "Username or Password is incorrect";
                        }                        
                    }
                    if (type == "doctor")
                    {
                        Doctor user = _context.Doctors.FirstOrDefault(u => u.UserName == username);
                        if (user != null)
                        {
                            if (user.Password == pwhash) 
                            {
                                //Creates a new Identity of the user
                                var claims = new List<Claim>
                                {
                                    new Claim(ClaimTypes.Name, "Doctor", ClaimValueTypes.String),
                                    new Claim(ClaimTypes.NameIdentifier, user.UserName.ToString(), ClaimValueTypes.String),
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
                        else
                        {
                            ViewBag.emptyfield = "Username or Password is incorrect";
                        }
                    }
                    if (type == "admin")
                    {
                       
                        Admin user = _context.Admins.FirstOrDefault(u => u.UserName == username);
                        if (user != null)
                        {
                           if (user.Password == pwhash) 
                           {
                               //Creates a new Identity of the user
                               var claims = new List<Claim>
                               {
                                   new Claim(ClaimTypes.Name, "Admin", ClaimValueTypes.String),
                                   new Claim(ClaimTypes.NameIdentifier, user.UserName.ToString(), ClaimValueTypes.String),
                                   new Claim(ClaimTypes.Role, "Admin", ClaimValueTypes.String)
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

                               return RedirectToAction("Link", "Admin");
                           }
                           else
                           {
                               ViewBag.emptyfield = "Username or Password is incorrect";
                           }
                        }
                        else
                        {
                           ViewBag.emptyfield = "Username or Password is incorrect";
                        }
                    }                    
                }               
            }
            return View();
        }

        [Authorize(Roles = "Patient")]
        public ActionResult Message(string sendto, string subject, string message) //Send a message to a patient
        {
            //string Sendto = sendto; //recipient name
            //string Message = message;


            Doctor user = _context.Doctors.FirstOrDefault(u => u.UserName == sendto);
            if (user != null)
            {
                if (message != null && message != "")
                {
                    var username = User.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value;
                    //_context.Patients.Update(user); niet nodig
                    //add the Message to the List<string> of messages
                    //user.Messages.Add(username)//DIT NIET MERGEN, IS TIJDELIJK;
                    //user.Messages.Add(subject);
                    //user.Messages.Add(message);
                    //send the new List<string> into the Database
                    _context.SaveChanges();
                    return RedirectToAction("MessageSend", "Patient");
                }
                else
                {
                    ViewBag.emptyfield = "You need to type in a message to send it.";
                }
            }
            else if (sendto != null)
            {
                ViewBag.emptyfield = "User not found";
            }
            return View();
        }

        [Authorize(Roles = "Patient")]
        public ActionResult Inbox()
        {

            Patient user = _context.Patients.FirstOrDefault(u => u.UserName == User.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value);
            ViewBag.message = "";//user.Messages;
            return View();
        }

        [Authorize(Roles = "Patient")]
        public ActionResult MessageDisplay(int index)
        {

            return View();
        }

        [Authorize(Roles = "Patient")]
        public ActionResult MessageSend()
        {
            return View();
        }


        public ActionResult Logout()
        {
            HttpContext.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [Authorize(Roles = "Patient")]
        public ActionResult Profile()
        {
            //Gets the username of the logged in user and sends it to the view
            var username = User.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value;
            ViewBag.username = username;
            var user = _context.Patients.FirstOrDefault(u => u.UserName == username);
			string email = user.Email.ToString();
			ViewBag.email = email;
			var phonenumber = user.PhoneNumber.ToString();
			ViewBag.phonenumber = phonenumber;
			var firstname = user.FirstName.ToString();
			ViewBag.firstname = firstname;
			var lastname = user.LastName.ToString();
			ViewBag.lastname = lastname;
			return View();
        }

        [Authorize(Roles = "Patient")]
        public IActionResult UpdateAccount(string firstname, string lastname, string email, int phonenumber)
		{
			if (firstname != null)
			{
				var USERNAME = User.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value;
				var USER = _context.Patients.FirstOrDefault(u => u.UserName == USERNAME);
				USER.FirstName = firstname;
				USER.LastName = lastname;
				USER.Email = email;
				USER.PhoneNumber = phonenumber.ToString();//DIT NIET MERGEN, IS TIJDELIJK
                _context.SaveChanges();
				return RedirectToAction("Profile", "Patient");
			}
			return View();
		}
    }
}
