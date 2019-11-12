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

    public class PatientController : Controller {
        private readonly DatabaseContext _context;

        public PatientController(DatabaseContext context)
        {
            _context = context;
        }

        public IActionResult CreateAccount()
        {
            return View();
        }

        [Route("Patient/SubmitPatientAccount")]
        public IActionResult SubmitPatientAccount(string firstname, string lastname, string email, int phonenumber, string username, string password)
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
                LastName = lastname,
                Email = email,
                PhoneNumber = phonenumber,
                UserName = username,
                Password = Program.Hash256bits(password),
                Messages = new List<string>(),
                DoctorIds = new List<int>(),
                linkcode = null,
                linkuses = 0,
                CanSeeMeId = new List<int>(),
                ICanSeeId = new List<int>()
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
        public IActionResult PatientList()
        {
            var patients = from p in _context.Patients select p;

            return View(patients);
        }


        public ActionResult Login(string username, string password, string type)
        {
            //string Username = username;
            //string Password = password;
            //var UserL = from u in _context.Patients where u.UserName == Username select u;   

            var adminexists = _context.Admins.Any(x => x.UserName == "admin");
            if (!adminexists) {
                Admin admin = new Admin() {
                    UserName = "admin",
                    Password = Program.Hash256bits("password")
                };

                _context.Admins.Add(admin);
                _context.SaveChanges();
            }


            if (username != null && password != null)
            {
                string pwhash = Program.Hash256bits(password);
                username = username.ToLower();
                if (type == null)
                {
                    ViewBag.emptyfield = "Please select an account type";
                }
                else
                {
                    if (type == "patient")
                    {
                        Patient user = _context.Patients.FirstOrDefault(u => u.UserName.ToLower() == username);
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
                        Doctor user = _context.Doctors.FirstOrDefault(u => u.UserName.ToLower() == username);
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

                        Admin user = _context.Admins.FirstOrDefault(u => u.UserName.ToLower() == username);
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
                ViewBag.emptyfield = "User not found";
            }
            return View();
        }


        public ActionResult Logout()
        {
            HttpContext.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

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
        public IActionResult UpdateAccount(string firstname, string lastname, string email, int phonenumber)
        {
            if (firstname != null)
            {
                var USERNAME = User.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value;
                var USER = _context.Patients.FirstOrDefault(u => u.UserName == USERNAME);
                USER.FirstName = firstname;
                USER.LastName = lastname;
                USER.Email = email;
                USER.PhoneNumber = phonenumber;
                _context.SaveChanges();
                return RedirectToAction("Profile", "Patient");
            }
            return View();
        }
        [Authorize(Roles="Patient")]
        public IActionResult GenerateAuthorizeCode()
        {
            return View();
        }
        /*public IActionResult CodeGenerated(int uses)
        {
            ViewData["Uses"] = uses;
            return View();
        }*/
        [Authorize(Roles = "Patient")]
        public IActionResult GenerateNewCode(int Uses)
        {
            string code;
            string username;

            code = Program.GenerateLinkCode(); //generates a new random code
            username = User.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value;
            DbAddCode(code,Uses);
            /*switch(Uses)
            {
                case "1 use":
                    usesInt = 1;
                    break;
                case "2 uses":
                    usesInt = 2;
                    break;
                case "3 uses":
                    usesInt = 3;
                    break;
            }*/
            ViewBag.CodeGenerated = code;
            if (Uses == 1)
            {
                ViewBag.UsesMessage = "1 attempt can be made to enter the correct code";
            }
            else
            {
                ViewBag.UsesMessage = Uses.ToString()+" attempts can be made to enter the correct code";
            }

            return View();
        }
        [Authorize(Roles ="Patient")]
        public IActionResult AuthorizationForm()
        {
            return View();
        }
        [Authorize(Roles = "Patient")]
        public IActionResult ConfirmAuthorization(string Username, string Code)
        {
            if (Username == null || Code == null) //als een van deze niet is ingevuld wordt er een melding gegeven in ConfirmAuthorization.cshtml
            {
                ViewBag.MessageRed = "You need to enter a Username and Code to proceed";
                return View();
                //RedirectToAction("Patient", "AuthorizationForm");
            }
            else //username and code are both filled in
            {
                {
                    Patient targetP = _context.Patients.FirstOrDefault(u => u.UserName == Username);
                    if (targetP == null)
                    {
                        ViewBag.MessageRed = "You entered an incorrect code or username.";
                        ViewBag.Message1 = "One attempt has been removed from the users authorization code.\nIf the code runs out of attempts, the user will have to generate a new code.";
                        return View();
                    }
                    _context.SaveChanges();
                }
                {
                    //_context.SaveChanges();
                    if (CheckAuthorizeCode(Code, Username))
                    {
                        //get the id of the logged in user
                        string authUsername = User.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value;
                        Patient Authorizer = _context.Patients.FirstOrDefault(u => u.UserName == authUsername);
                        Patient targetNew = _context.Patients.FirstOrDefault(u => u.UserName == Username);
                        if (Authorizer == null || targetNew == null)
                        {
                            throw new Exception("There is not user with name " + authUsername + " in the database");
                        }
                        else
                        {
                            if (targetNew.CanSeeMeId == null)
                            {
                                targetNew.CanSeeMeId = new List<int>();
                            }
                            if (!targetNew.CanSeeMeId.Contains(Authorizer.PatientId))
                            {
                                targetNew.CanSeeMeId.Add(Authorizer.PatientId);
                                if (Authorizer.ICanSeeId == null)
                                {
                                    Authorizer.ICanSeeId = new List<int>();
                                }
                                Authorizer.ICanSeeId.Add(targetNew.PatientId);

                                _context.Update(targetNew);
                                _context.Update(Authorizer);
                                _context.SaveChanges();
                            }
                            ViewBag.Message1 = "You have succesfully been authorized to view "+targetNew.UserName+"'s profile.\nTheir authorization code has been cleared.";
                            return View();
                        }
                    }
                    else
                    {
                        ViewBag.MessageRed = "You entered an incorrect code or username.";
                        ViewBag.Message1 = "One attempt has been removed from the users authorization code.\nIf the code runs out of attempts, the user will have to generate a new code.";
                        return View();
                    }
                }
                
            }
        }
        private void DbAddCode(string code, int uses, int patientId)
        {
            var patient = _context.Patients.FirstOrDefault(u => u.PatientId == patientId); //get the patient according to the patientId
            if (patient != null)
            {
                if (uses < 1)
                {
                    throw new Exception("uses has to be at least 1, but you used: "+uses.ToString());
                }
                patient.linkcode = code;
                patient.linkuses = uses;
                _context.SaveChanges();
            }
            else
            {
                throw new Exception("Attempted to find patient with non-existing Id: " + patientId.ToString());
            }
        }
        private void DbAddCode(string code, int uses)
        {
            var username = User.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value; //get the username
            var patient = _context.Patients.FirstOrDefault(u => u.UserName == username); //get the patient according to the username
            if (patient != null)
            {
                patient.linkcode = code;
                patient.linkuses = uses;
                _context.SaveChanges();
            }
            else
            {
                throw new Exception("Attempted to find patient with non-existing username: " + username);
            }
        }
        private void DbAddCode(string code, int uses, string Username)
        {
            var patient = _context.Patients.FirstOrDefault(u => u.UserName == Username); //get the patient according to the username
            if (patient != null)
            {
                patient.linkcode = code;
                patient.linkuses = uses;
                _context.SaveChanges();
            }
            else
            {
                throw new Exception("Attempted to find patient with non-existing Username: " + Username);
            }
        }
        public bool CheckAuthorizeCode(string code, int patientId)
        {
            Patient patient = _context.Patients.FirstOrDefault(u => u.PatientId == patientId);
            if (patient == null)
            {
                throw new Exception("Trying to check code for non-existent patient using patientId: " + patientId.ToString());
                //return false;
            }
            else
            {
                if (patient.linkcode == null)
                {
                    return false;
                }
                else
                {
                    if (patient.linkuses <= 0)
                    {
                        patient.linkcode = null;
                        _context.SaveChanges();
                        return false;
                    }
                    else
                    {
                        patient.linkuses -= 1;
                        if (patient.linkcode == code)
                        {
                            if (patient.linkuses <= 0)
                            {
                                patient.linkcode = null;
                            }
                            _context.SaveChanges();
                            return true;
                        }
                        else
                        {
                            if (patient.linkuses <= 0)
                            {
                                patient.linkcode = null;
                            }
                            _context.SaveChanges();
                            return false;
                        }
                    }
                }
            }
        }
        public bool CheckAuthorizeCode(string code, string username)
        {
            Patient patient = _context.Patients.FirstOrDefault(u => u.UserName == username);
            if (patient == null)
            {
                throw new Exception("Trying to check code for non-existent patient using username: "+ username);
                //return false;
            }
            else
            {
                if (patient.linkcode == null)
                {
                    return false;
                }
                else
                {
                    if (patient.linkuses <= 0)
                    {
                        patient.linkcode = null;
                        _context.SaveChanges();
                        return false;
                    }
                    else
                    {
                        patient.linkuses -= 1;
                        if (patient.linkcode == code)
                        {
                            patient.linkcode = null;
                            patient.linkuses = 0;
                            _context.SaveChanges();
                            return true;
                        }
                        else
                        {
                            if (patient.linkuses <= 0)
                            {
                                patient.linkcode = null;
                            }
                            _context.SaveChanges();
                            return false;
                        }
                    }
                }
            }
        }
    }
}
