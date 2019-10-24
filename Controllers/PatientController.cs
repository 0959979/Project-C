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
            var USERNAME = _context.Patients.FirstOrDefault(u => u.UserName == username);
            var EMAIL = _context.Patients.FirstOrDefault(u => u.Email == email);
            if (USERNAME != null)
            {
                ViewBag.username = "Username is already used";
                RedirectToAction("CreateAccount", "Patient");                    
            }
            else if (EMAIL != null)
            {
                ViewBag.email = "Email is already in use";
                RedirectToAction("CreateAccount", "Patient");
            }
            else
            {
                Patient patient = new Patient()
                {
                    FirstName = firstname,
                    LastName = lastname,
                    Email = email,
                    PhoneNumber = phonenumber,
                    UserName = username,
                    Password = password,
                    Messages = new List<string>()
                };
                _context.Patients.Add(patient);
                _context.SaveChanges();

                ViewData["FirstName"] = patient.FirstName;
                ViewData["LastName"] = patient.LastName;
            }
            return View("SubmitPatientAccount");
        }

        

        //PatientList Page
        //Authorizes the page so only users with the role Patient can view it
        [Authorize(Roles = "Patient")]
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
            if (username != null && password != null)
            {
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
                            if (user.Password == password)
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
                            if (user.Password == password)
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
                        ViewBag.emptyfield = "Sorry admin is not in this version yet";
                        //Admin user = _context.Admins.FirstOrDefault(u => u.UserName == username);
                        //if (user != null)
                        //{
                        //    if (user.Password == password)
                        //    {
                        //        //Creates a new Identity of the user
                        //        var claims = new List<Claim>
                        //        {
                        //            new Claim(ClaimTypes.Name, "Admin", ClaimValueTypes.String),
                        //            new Claim(ClaimTypes.NameIdentifier, user.UserName.ToString(), ClaimValueTypes.String),
                        //            new Claim(ClaimTypes.Role, "Admin", ClaimValueTypes.String)
                        //        };
                        //        var userIdentity = new ClaimsIdentity(claims, "SecureLogin");
                        //        var userPrincipal = new ClaimsPrincipal(userIdentity);

                        //        HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                        //            userPrincipal,
                        //            new AuthenticationProperties
                        //            {
                        //                ExpiresUtc = DateTime.UtcNow.AddMinutes(30),
                        //                IsPersistent = true,
                        //                AllowRefresh = false
                        //            });

                        //        return RedirectToAction("CreateAccount", "Doctor");
                        //    }
                        //    else
                        //    {
                        //        ViewBag.emptyfield = "Username or Password is incorrect";
                        //    }
                        //}
                        //else
                        //{
                        //    ViewBag.emptyfield = "Username or Password is incorrect";
                        //}
                    }                    
                }               
            }
            return View();
        }


        public ActionResult Message(string sendto, string message) //Send a message to a patient
        {
            //string Sendto = sendto; //recipient name
            //string Message = message;
            Patient user = _context.Patients.FirstOrDefault(u => u.UserName == sendto);
            if (user != null)
            {
                if (message != null && message != "")
                {
                    //_context.Patients.Update(user); niet nodig
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
                ViewBag.emptyfield = "User not found";
            }
            return View();
        }
        public ActionResult Profile()
        {
            //Gets the username of the logged in user and sends it to the view
            var username = User.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value;
            ViewBag.username = username;

            return View();
        }

        public ActionResult Logout()
        {
            HttpContext.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

    }
    }