using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using zorgapp.Models;
using zorgapp.ViewModels;

namespace zorgapp.Controllers
{

    public class PatientController : Controller
    {
        private readonly DatabaseContext _context;

        public PatientController(DatabaseContext context)
        {
            _context = context;
        }

        // returns view of CreateAccount
        public IActionResult CreateAccount()
        {
            return View();
        }


        [Route("Patient/SubmitPatientAccount")]
        // creates account voor patient
        public IActionResult SubmitPatientAccount(string firstname, string lastname, string email, string phonenumber, string username, string password)
        {
            // check if all parameters are filled in
            if (firstname != null && lastname != null && email != null && phonenumber != null && username != null && password != null)
            {
                bool valid = true;
                {   // check if email is aready in use
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
                    { // check if username is aready in use
                        ViewBag.emptyfield3 = "this username is already in use";
                        valid = false;
                    }
                }
                {
                    if (password.Count() < 8)
                    { // check if password has enough characters (8 or more)
                        ViewBag.emptyfield3 = "Password should be more than 8 characters long";
                        valid = false;
                    }
                }
                if (!valid)
                {
                    return View("CreateAccount");
                }
                // create the new patient 
                Patient patient = new Patient()
                {
                    FirstName = firstname,
                    LastName = lastname,
                    Email = email,
                    LocalId = new List<string>(),
                    PhoneNumber = phonenumber,
                    UserName = username,
                    Password = Program.Hash256bits(username + password),
                    LinkCode = null,
                    LinkUses = 0,
                    CanSeeMeId = new List<int>(),
                    ICanSeeId = new List<int>()
                };

                // add patient to database
                _context.Patients.Add(patient);
                _context.SaveChanges();

                ViewData["FirstName"] = patient.FirstName;
                ViewData["LastName"] = patient.LastName;

                return View("SubmitPatientAccount");
            }
            return View();
        }

        // Authorizes the page so only users with the role admin can view it
        [Authorize(Roles = "Admin")]
        public IActionResult PatientList()
        {
            // selects all patients in the database
            var patients = from p in _context.Patients select p;

            return View(patients);
        }
        public IActionResult DoctorList()
        {
            // take the username of the patient that is logged in

            string username_ = User.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value;

            // find the patient with that username

            Patient patient = _context.Patients.FirstOrDefault(u => u.UserName == username_);

            // take the id of the patient

            int patid = patient.PatientId;

            // create a new list of doctors

            var Doc = new List<Doctor>();

            // create a list of patientsdoctors (linked) where the patient logged in is linked

            var patientsDoctors = from d in _context.PatientsDoctorss where d.PatientId == patid select d;

            // for every link made with that patient, find the doctor and add it to the list of doctors

            foreach (var item in patientsDoctors)
            {
                if (item.DoctorId != null)
                {
                    var doctor_ = _context.Doctors.FirstOrDefault(p => p.DoctorId == item.DoctorId);
                    Doc.Add(doctor_);
                };

            }
            // return the view with the list of doctors linked to the logged in patient

            return View(Doc);
        }
        public IActionResult DocProfile(IFormCollection form)
        {
            string id = form["doctorid"].ToString();
            int id_ = int.Parse(id);
            Doctor doctor = _context.Doctors.FirstOrDefault(u => u.DoctorId == id_);

            return View(doctor);
        }



        public IActionResult AddLocalId(int patientid, string localid)
        {
            Patient patient = _context.Patients.FirstOrDefault(u => u.PatientId == patientid);
            patient.LocalId.Add(localid);
            _context.SaveChanges();

            return RedirectToAction("PatientList", "Doctor");
        }


        [Authorize(Roles = "Patient")]
        public ActionResult Agenda(string Previous, int dayoffset, string Next, int starthour, int endhour, string Apply)
        {
            {
                if (!string.IsNullOrEmpty(Next)) //if the doctor pressed the next button
                {
                    dayoffset += 7;
                    //ViewBag.Recieved = "Next, dayoffset = "+dayoffset.ToString();
                }
                else if (!string.IsNullOrEmpty(Previous)) //if the doctor pressed the previous button
                {
                    dayoffset -= 7;
                    //ViewBag.Recieved = "Previous, dayoffset = " + dayoffset.ToString();
                }
                else if (!string.IsNullOrEmpty(Apply)) //if the doctor pressed the apply button
                {
                    dayoffset += 0;
                    //ViewBag.Recieved = "Previous, dayoffset = " + dayoffset.ToString();
                }
                else //if he did not press any of the 3 above buttons
                {
                    dayoffset = 0;
                    //ViewBag.Recieved = "None";
                }
                if (endhour <= starthour)
                {
                    starthour = 6;
                    endhour = 20;
                }
                System.Diagnostics.Debug.WriteLine("starthour: " + starthour.ToString());
                System.Diagnostics.Debug.WriteLine("endhour: " + endhour.ToString());
                bool AmPm = true;
                List<string> Day = new List<string>();
                List<string> Date = new List<string>();
                List<string> Hour = new List<string>();
                List<int> Houri = new List<int>();
                List<int> Minute = new List<int>();
                List<Case> Case = new List<Case>();
                List<Appointment> Appointment = new List<Appointment>();
                //List<List<int>> AppointmentDatesHours = new List<List<int>>();
                //List<string> AppointmentStrings = new List<string>();
                DateTime Today = DateTime.Now;//new DateTime(2020, 4, 13);
                Today = Today.AddDays(dayoffset);
                DateTime FWeekday;
                int offset;

                //the days and dates
                {
                    DayOfWeek WeekDay = Today.DayOfWeek;
                    offset = 0;
                    switch (WeekDay.ToString())
                    {
                        case "Monday":
                            offset = 0;
                            break;
                        case "Tuesday":
                            offset = 1;
                            break;
                        case "Wednesday":
                            offset = 2;
                            break;
                        case "Thursday":
                            offset = 3;
                            break;
                        case "Friday":
                            offset = 4;
                            break;
                        case "Saturday":
                            offset = 5;
                            break;
                        case "Sunday":
                            offset = 6;
                            break;
                        default:
                            offset = 0;
                            break;
                    }
                    //offset ensures that the agenda starts at monday. To get it to start at sunday uncomment:
                    //offset += 1; //Higher offset means the first point on the agenda starts earlier
                    int d;
                    FWeekday = new DateTime(Today.Year, Today.Month, Today.Day);
                    FWeekday = FWeekday.AddDays(-offset);//makes FWeekday equal to the first day of the week
                    for (d = 0; d < 7; d++)
                    {
                        DateTime day = Today;//DateTime.Now;
                        day = day.AddDays(d - offset);
                        Day.Add(day.DayOfWeek.ToString());
                        Date.Add(day.Date.ToShortDateString());
                        //System.Diagnostics.Debug.WriteLine("RonanDayList: " + day.ToString());
                    }
                }
                //The hours and minutes that will be shown
                {
                    for (int h = starthour; h <= endhour; h++)
                    {
                        Houri.Add(h);
                        if (AmPm)
                        {
                            if (h > 12)
                            {
                                int u;
                                u = h - 12;
                                Hour.Add(u.ToString() + " pm");
                            }
                            else
                            {
                                Hour.Add(h.ToString() + " am");
                            }
                        }
                        else
                        {
                            Hour.Add(h.ToString());
                        }
                    }
                    for (int m = 0; m < 60; m += 5)
                    {
                        Minute.Add(m);
                    }
                }
                //Gets the appointments of that week
                { //query door Pelle geschreven
                    Patient user = _context.Patients.FirstOrDefault(u => u.UserName == User.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value);
                    int patientid = user.PatientId;

                    var cases = from c in _context.Cases where c.PatientId == patientid select c;
                    var Tempappointments = new List<Appointment>();
                    //var appointments = from a in _context.Appointments where a.select a;

                    foreach (var item in cases)
                    {
                        Case.Add(item);
                    }
                    foreach (var item in cases) //gets all apointments 
                    {
                        var appointment = from c in _context.Appointments where c.CaseId == item.CaseId select c;
                        appointment = from c in appointment where c.DoctorId == item.DoctorId select c;
                        foreach (var app in appointment)
                        {
                            Tempappointments.Add(app);
                        }
                        Tempappointments = Program.FilterWeek(Tempappointments, Today, 7);
                    }
                    foreach (var item in Tempappointments)
                    {
                        string infosub;
                        infosub = item.Info.Trim();
                        if (infosub.Length > 18)
                        {
                            infosub = infosub.Substring(0, 16) + "...";
                        }
                        /*DateTime AppTime = new DateTime();
                        AppTime.AddHours(item.Date.Hour);
                        AppTime.AddMinutes(item.Date.Minute);*/
                        Appointment.Add(new Appointment()
                        {
                            AppointmentId = item.AppointmentId,
                            Date = item.Date,
                            Info = infosub,
                            CaseId = item.CaseId,
                        }
                        );
                    }
                }
                bool sWeek;
                DateTime c_date;
                c_date = DateTime.Now;
                sWeek = Program.SameWeek(Today, c_date);
                AgendaViewModel agendamodel = new AgendaViewModel
                {
                    Days = Day,
                    Dates = Date,
                    Hours = Hour,
                    Hoursi = Houri,
                    Minutes = Minute,
                    CurrentDate = offset,
                    dayOffset = dayoffset,
                    sameWeek = sWeek,
                    Cases = Case,
                    Appointments = Appointment
                };

                return View(agendamodel);
            }

        }

        public ActionResult Login(string username, string password, string type)
        {
            // checks if the "standard" admin account is already created in the database

            var adminexists = _context.Admins.Any(x => x.UserName == "admin");
            if (!adminexists)
            {
                // if admin does not already exist in database, create new admin account
                Admin admin = new Admin()
                {
                    UserName = "admin",
                    Password = Program.Hash256bits("adminpassword")
                };
                // add admin to the database

                _context.Admins.Add(admin);
                _context.SaveChanges();
            }


            if (username != null && password != null)
            {
                string pwhash = Program.Hash256bits(username + password);
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

                                return RedirectToAction("Inbox", "Patient");
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

                                return RedirectToAction("Inbox", "Doctor");
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

                                return RedirectToAction("CreateAccount", "Doctor");
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
        public ActionResult Message(string reciever, string subject, string text)
        {
            Doctor doctor = _context.Doctors.FirstOrDefault(u => u.UserName == reciever);
            if (doctor != null)
            {
                if (text != null && text != "")
                {
                    var username = User.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value;
                    Patient patient = _context.Patients.FirstOrDefault(u => u.UserName == username);
                    Message message = new Message()
                    {
                        Sender = username,
                        Receiver = reciever,
                        Subject = subject,
                        Text = text,
                        Date = DateTime.Now,
                        DoctorToPatient = false
                    };

                    _context.Messages.Add(message);
                    _context.SaveChanges();
                    return RedirectToAction("MessageSend", "Patient");
                }
                else
                {
                    ViewBag.emptyfield = "You need to type in a message to send it.";
                }
            }
            else if (reciever != null)
            {
                ViewBag.emptyfield = "User not found";
            }
            return View();
        }

        [Authorize(Roles = "Patient")]
        public ActionResult Inbox()
        {
            Patient user = _context.Patients.FirstOrDefault(u => u.UserName == User.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value);
            var message = from m in _context.Messages where m.Text == "" select m;
            var check = from m in _context.Messages where m.DoctorToPatient == true && m.Receiver == User.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value select m;
            if (message != check)
            {
                message = from m in _context.Messages where m.DoctorToPatient == true && m.Receiver == User.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value orderby m.Date descending select m;
            }
            return View(message);
        }


        [Authorize(Roles = "Patient")]
        public ActionResult SentMessages()
        {
            Patient user = _context.Patients.FirstOrDefault(u => u.UserName == User.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value);
            var message = from m in _context.Messages where m.Text == "" select m;
            var check = from m in _context.Messages where m.DoctorToPatient == false && m.Sender == User.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value select m;
            if (message != check)
            {
                message = from m in _context.Messages where m.DoctorToPatient == false && m.Sender == User.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value orderby m.Date descending select m;
            }
            return View(message);
        }

        public ActionResult Reply(IFormCollection form)
        {
            string reply = form["reply"].ToString();
            ViewBag.reply = reply;
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
        public IActionResult UpdateAccount(string firstname, string lastname, string email, string phonenumber)
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

        [Authorize(Roles = "Patient")]
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
            DbAddCode(code, Uses); //adds the code to the logged in user
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
                ViewBag.UsesMessage = Uses.ToString() + " attempts can be made to enter the correct code";
            }

            return View();
        }
        [Authorize(Roles = "Patient")]
        public IActionResult AuthorizationForm()
        {
            return View();
        }
        [Authorize(Roles = "Patient")]
        public IActionResult ConfirmAuthorization(string Username, string Code)
        {
            if (Username == null || Code == null) //If one of these is not filled in, a note will show in ConfirmAuthorization.cshtml
            {
                ViewBag.MessageRed = "You need to enter a Username and Code to proceed";
                return View();
                //RedirectToAction("Patient", "AuthorizationForm");
            }
            else //username and code are both filled in
            {
                Username = Username.ToLower();
                {
                    Patient targetP = _context.Patients.FirstOrDefault(u => u.UserName.ToLower() == Username);
                    if (targetP == null) //the user you are trying to get authorization for does not exist
                    {
                        ViewBag.MessageRed = "You entered an incorrect code or username.";
                        ViewBag.Message1 = "One attempt has been removed from the users authorization code.\nIf the code runs out of attempts, the user will have to generate a new code.";
                        return View();
                    }
                    _context.SaveChanges();
                }
                {
                    //_context.SaveChanges();
                    if (CheckAuthorizeCode(Code, Username)) //checks if the code is correct
                    {
                        //get the id of the logged in user
                        string authUsername = User.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value.ToLower();
                        Patient Authorizer = _context.Patients.FirstOrDefault(u => u.UserName.ToLower() == authUsername);
                        Patient targetNew = _context.Patients.FirstOrDefault(u => u.UserName.ToLower() == Username);
                        if (Authorizer == null || targetNew == null)
                        {
                            throw new Exception("There is no user with name " + authUsername + " in the database");
                        }
                        else
                        {
                            if (targetNew.CanSeeMeId == null) 
                            {
                                targetNew.CanSeeMeId = new List<int>();
                            }
                            if (!targetNew.CanSeeMeId.Contains(Authorizer.PatientId)) //make sure the user id is not already in his list
                            {
                                targetNew.CanSeeMeId.Add(Authorizer.PatientId);
                                if (Authorizer.ICanSeeId == null) //ensure the list exists to avoid null errors
                                {
                                    Authorizer.ICanSeeId = new List<int>();
                                }
                                Authorizer.ICanSeeId.Add(targetNew.PatientId);

                                _context.Update(targetNew);
                                _context.Update(Authorizer);
                                _context.SaveChanges();
                            }
                            ViewBag.Message1 = "You have succesfully been authorized to view " + targetNew.UserName + "'s profile.\nTheir authorization code has been cleared.";
                            return View();
                        }
                    }
                    else //if the code is incorrect
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
                    throw new Exception("uses has to be at least 1, but you used: " + uses.ToString());
                }
                patient.LinkCode = code;
                patient.LinkUses = uses;
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
                if (uses <= 0)
                {
                    uses = 1;
                }
                if (uses > 3)
                {
                    uses = 3;
                }
                patient.LinkCode = code;
                patient.LinkUses = uses;
                _context.SaveChanges();
            }
            else
            {
                throw new Exception("Attempted to find patient with non-existing username: " + username);
            }
        }
        public void DbAddCode(string code, int uses, string Username)
        {
            var patient = _context.Patients.FirstOrDefault(u => u.UserName == Username); //get the patient according to the username
            if (patient != null)
            {
                patient.LinkCode = code;
                patient.LinkUses = uses;
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
                if (patient.LinkCode == null)
                {
                    return false;
                }
                else
                {
                    if (patient.LinkUses <= 0)
                    {
                        patient.LinkCode = null;
                        _context.SaveChanges();
                        return false;
                    }
                    else
                    {
                        patient.LinkUses -= 1;
                        if (patient.LinkCode == code)
                        {
                            if (patient.LinkUses <= 0)
                            {
                                patient.LinkCode = null;
                            }
                            _context.SaveChanges();
                            return true;
                        }
                        else
                        {
                            if (patient.LinkUses <= 0)
                            {
                                patient.LinkCode = null;
                            }
                            _context.SaveChanges();
                            return false;
                        }
                    }
                }
            }
        }
        public bool CheckAuthorizeCode(string code, string username) //checks if the code is correct for the given username
        {
            Patient patient = _context.Patients.FirstOrDefault(u => u.UserName.ToLower() == username.ToLower());
            if (patient == null) //if the patient does not exist
            {
                throw new Exception("Trying to check code for non-existent patient using username: " + username);
                //return false;
            }
            else
            {
                if (patient.LinkCode == null) //if the patient has no code currently
                {
                    return false;
                }
                else
                {
                    if (patient.LinkUses <= 0) //if the patient has a code but no uses left, remove the code
                    {
                        patient.LinkCode = null;
                        _context.SaveChanges();
                        return false;
                    }
                    else //the patient has a code and has uses
                    {
                        patient.LinkUses -= 1; //remove one use of the code, regardless if it is correct or wrong
                        if (patient.LinkCode == code) //check if the code is correct
                        {
                            patient.LinkCode = null; //remove the code
                            patient.LinkUses = 0; //remove the uses
                            _context.SaveChanges();
                            return true;
                        }
                        else //if the code is wrong
                        {
                            if (patient.LinkUses <= 0)
                            {
                                patient.LinkCode = null;
                            }
                            _context.SaveChanges();
                            return false;
                        }
                    }
                }
            }
        }

        public ActionResult AuthorizationList()
        {
            string username = User.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value;
            Patient user = _context.Patients.FirstOrDefault(u => u.UserName == username);
            int id = user.PatientId;

            //ensure the lists exist to avoid null errors
            if (user.ICanSeeId == null)
            {
                user.ICanSeeId = new List<int>();
            }
            if (user.CanSeeMeId == null)
            {
                user.CanSeeMeId = new List<int>();
            }

            List<int> carerids = user.CanSeeMeId;
            List<Patient> carers = new List<Patient>();

            List<int> careeids = user.ICanSeeId;
            List<Patient> carees = new List<Patient>();

            if (carerids != null)
            {
                foreach (var item in carerids)
                {
                    Patient carer = _context.Patients.FirstOrDefault(u => u.PatientId == item);
                    carers.Add(carer);
                }
            }
            else
            {
                Patient empty = _context.Patients.FirstOrDefault(u => u.UserName == "");
                carers.Add(empty);
            }


            if (careeids != null)
            {
                foreach (var item in careeids)
                {
                    Patient caree = _context.Patients.FirstOrDefault(u => u.PatientId == item);
                    carees.Add(caree);
                }
            }
            else
            {
                Patient empty = _context.Patients.FirstOrDefault(u => u.UserName == "");
                carees.Add(empty);
            }

            CareViewModel carelist = new CareViewModel
            {
                Carers = carers,
                Carees = carees
            };

            return View(carelist);
        }

        public ActionResult ProfileVisit(IFormCollection form)
        {
            string idstring = form["reply"].ToString();
            int profileid = int.Parse(idstring);
            Patient patient = _context.Patients.FirstOrDefault(p => p.PatientId == profileid);

            var medicines_ = from m in _context.Medicines where m.PatientId == profileid select m;
            var emptymedicine = _context.Medicines.FirstOrDefault(m => m.MedicineId == 0);
            List<Medicine> medicines = new List<Medicine>();
            if (medicines_ == null)
            {
                medicines.Add(emptymedicine);
            }
            else
            {
                foreach (var item in medicines_)
                {
                    medicines.Add(item);
                }
            }

            var cases = from c in _context.Cases where c.PatientId == profileid select c;
            List<string> caseids = new List<string>();

            var emptyappointment = _context.Appointments.FirstOrDefault(m => m.AppointmentId == 0);
            List<Appointment> appointments = new List<Appointment>();
            if (cases == null)
            {
                appointments.Add(emptyappointment);
            }
            else
            {
                foreach (var item in cases)
                {
                    caseids.Add(item.CaseId);
                }

                foreach (var item in caseids)
                {

                    var appointments_ = from a in _context.Appointments where a.CaseId == item select a;
                    foreach (var item_ in appointments_)
                    {
                        appointments.Add(item_);
                    }
                }
            }

            ProfileViewModel profiledata = new ProfileViewModel
            {
                UserInfo = patient,
                Appointments = appointments,
                Medicines = medicines
            };

            return View(profiledata);
        }

        public ActionResult AuthorizationRevoke(int id)
        {
            string username = User.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value;
            Patient user = _context.Patients.FirstOrDefault(u => u.UserName == username);
            int userid = user.PatientId;
            List<int> newlist = new List<int>();
            List<int> newlist2 = new List<int>();

            //ensure the lists exist to avoid null errors
            if (user.ICanSeeId == null)
            {
                user.ICanSeeId = new List<int>();
            }
            if (user.CanSeeMeId == null)
            {
                user.CanSeeMeId = new List<int>();
            }

            foreach (var item in user.CanSeeMeId)
            {
                if (item != id)
                {
                    newlist.Add(item);
                }
            }
            user.CanSeeMeId = newlist;
            _context.SaveChanges();

            Patient user2 = _context.Patients.FirstOrDefault(u => u.PatientId == id);
            if (user2 == null)
            {
                return RedirectToAction("AuthorizationList", "Patient");
            }

            //ensure the lists exist to avoid null errors
            if (user2.ICanSeeId == null)
            {
                user2.ICanSeeId = new List<int>();
            }
            if (user2.CanSeeMeId == null)
            {
                user2.CanSeeMeId = new List<int>();
            }

            foreach (var item in user.ICanSeeId)
            {
                if (item != userid)
                {
                    newlist2.Add(item);
                }
            }
            user2.ICanSeeId = newlist2;
            _context.SaveChanges();

            return RedirectToAction("AuthorizationList", "Patient");
        }

        //TESTING
        public DatabaseContext getContext()
        {
            return _context;
        }
        public IActionResult noAccess()
        {
            return View();
        }

        public IActionResult TestPage()
        {
            //Login("admin", "password", "patient");
            List<Tuple<string, string>> tupleList = new List<Tuple<string, string>>();

            List<PatientTest> testlist = new List<PatientTest>();
            {
                testlist.Add(new GenerateNewAuthorizeCodeTest1(this));
                testlist.Add(new GenerateNewAuthorizeCodeTest2(this));
                testlist.Add(new GenerateNewAuthorizeCodeTest3(this));
                testlist.Add(new ConfirmAuthorizeCodeTest1(this));
                testlist.Add(new ConfirmAuthorizeCodeTest2(this));
                testlist.Add(new ConfirmAuthorizeCodeTest3(this));
                testlist.Add(new AuthorizationRevokeTest1(this));
                testlist.Add(new AuthorizationRevokeTest2(this));
                testlist.Add(new AuthorizationRevokeTest3(this));
            }
            foreach (PatientTest T in testlist)
            {
                tupleList.Add(new Tuple<string, string>(T.Id, T.Id));
            }
            Login("admin", "password", "patient");
            TestListViewModel testlistmodel = new TestListViewModel { tuples = tupleList };
            return View(testlistmodel);
        }

        public IActionResult StartTest(string TestId)
        {
            List<PatientTest> testlist = new List<PatientTest>();
            {
                testlist.Add(new GenerateNewAuthorizeCodeTest1(this));
                testlist.Add(new GenerateNewAuthorizeCodeTest2(this));
                testlist.Add(new GenerateNewAuthorizeCodeTest3(this));
                testlist.Add(new ConfirmAuthorizeCodeTest1(this));
                testlist.Add(new ConfirmAuthorizeCodeTest2(this));
                testlist.Add(new ConfirmAuthorizeCodeTest3(this));
                testlist.Add(new AuthorizationRevokeTest1(this));
                testlist.Add(new AuthorizationRevokeTest2(this));
                testlist.Add(new AuthorizationRevokeTest3(this));
            }
            PatientTest testobj = testlist.FirstOrDefault();
            foreach (PatientTest T in testlist)
            {
                if (T.Id == TestId)
                {
                    testobj = T;
                }
            }
            TestViewModel Model = testobj.Run();
            return View(Model);
        }
    }
    internal abstract class PatientTest
    {
        public abstract TestViewModel Run();
        public PatientController testController;
        public string Id;
        public string Description;
        public string Steps;
        public string Criteria;
        public string Inputstr;
        public string Aresult;
        public string Eresult;
        public bool Pass;
    }
    internal class GenerateNewAuthorizeCodeTest1 : PatientTest
    {
        public GenerateNewAuthorizeCodeTest1(PatientController tc)
        {
            testController = tc;
            Id = "P5.Integration.GNAC1";
            Description = "GenerateNewAuthorizeCode test 1";
            Steps = "Generate a new authorize code whilst logged in.";
            Criteria = "The code must be added to the patient in the database, and be a 32 character string";
            Inputstr = "Uses = 3";
            Aresult = "";
            Eresult = "a string with 32 characters";
        }

        public override TestViewModel Run()
        {
            TestViewModel model;

            //arrange
            bool Pass = false;
            PatientController controller = testController;
            //controller.Login("admin", "password", "Patient");

            int uses = 3;

            //act
            try
            {
                controller.GenerateNewCode(uses);
            }
            catch (Exception e)
            {
                Pass = false;
                Aresult = e.ToString();
                model = new TestViewModel()
                {
                    id = Id,
                    time = DateTime.Now,
                    description = Description,
                    steps = Steps,
                    criteria = Criteria,
                    input = Inputstr,
                    aresult = Aresult,
                    eresult = Eresult,
                    pass = Pass
                };
                return model;
            }

            //assert
            DatabaseContext Tcontext = testController.getContext();
            Patient P = Tcontext.Patients.FirstOrDefault(u => u.UserName.ToLower() == "Admin".ToLower());
            string Code = P.LinkCode;
            if (P == null)
            {
                Aresult = "Null";
            }
            else
            {
                Aresult = "a string with " + P.LinkCode.Length.ToString() + " characters";
            }

            if (Aresult == Eresult)
            {
                Pass = true;
            }
            else
            {
                Pass = false;
            }

            model = new TestViewModel()
            {
                id = Id,
                time = DateTime.Now,
                description = Description,
                steps = Steps,
                criteria = Criteria,
                input = Inputstr,
                aresult = Aresult,
                eresult = Eresult,
                pass = Pass
            };
            return model;
        }
    }
    internal class GenerateNewAuthorizeCodeTest2 : PatientTest
    {
        public GenerateNewAuthorizeCodeTest2(PatientController tc)
        {
            testController = tc;
            Id = "P5.Integration.GNAC2";
            Description = "GenerateNewAuthorizeCode test 2";
            Steps = "Generate a new authorize code whilst logged in.";
            Criteria = "The code must be added to the patient in the database, and be a 32 character string, and have 1 use";
            Inputstr = "Uses = -2";
            Aresult = "";
            Eresult = "a string with 32 characters and 1 use(s)";
        }

        public override TestViewModel Run()
        {
            TestViewModel model;

            //arrange
            bool Pass = false;
            PatientController controller = testController;
            //controller.Login("admin", "password", "Patient");

            int uses = -2;

            //act
            try
            {
                controller.GenerateNewCode(uses);
            }
            catch (Exception e)
            {
                Pass = false;
                Aresult = e.ToString();
                model = new TestViewModel()
                {
                    id = Id,
                    time = DateTime.Now,
                    description = Description,
                    steps = Steps,
                    criteria = Criteria,
                    input = Inputstr,
                    aresult = Aresult,
                    eresult = Eresult,
                    pass = Pass
                };
                controller.Logout();
                return model;
            }

            //assert
            DatabaseContext Tcontext = testController.getContext();
            Patient P = Tcontext.Patients.FirstOrDefault(u => u.UserName.ToLower() == "Admin".ToLower());
            string Code = P.LinkCode;
            if (P == null)
            {
                Aresult = "Null";
            }
            else
            {
                Aresult = "a string with " + P.LinkCode.Length.ToString() + " characters and "+ P.LinkUses.ToString() + " use(s)";
            }

            if (Aresult == Eresult)
            {
                Pass = true;
            }
            else
            {
                Pass = false;
            }

            model = new TestViewModel()
            {
                id = Id,
                time = DateTime.Now,
                description = Description,
                steps = Steps,
                criteria = Criteria,
                input = Inputstr,
                aresult = Aresult,
                eresult = Eresult,
                pass = Pass
            };
            controller.Logout();
            return model;
        }
    }
    internal class GenerateNewAuthorizeCodeTest3 : PatientTest
    {
        public GenerateNewAuthorizeCodeTest3(PatientController tc)
        {
            testController = tc;
            Id = "P5.Integration.GNAC3";
            Description = "GenerateNewAuthorizeCode test 3";
            Steps = "Generate a new authorize code whilst logged in.";
            Criteria = "The code must be added to the patient in the database, and be a 32 character string, and have 3 uses";
            Inputstr = "Uses = 20";
            Aresult = "";
            Eresult = "a string with 32 characters and 3 use(s)";
        }

        public override TestViewModel Run()
        {
            TestViewModel model;

            //arrange
            bool Pass = false;
            PatientController controller = testController;
            //controller.Login("admin", "password", "patient");

            int uses = 20;

            //act
            try
            {
                controller.GenerateNewCode(uses);
            }
            catch (Exception e)
            {
                Pass = false;
                Aresult = e.ToString();
                model = new TestViewModel()
                {
                    id = Id,
                    time = DateTime.Now,
                    description = Description,
                    steps = Steps,
                    criteria = Criteria,
                    input = Inputstr,
                    aresult = Aresult,
                    eresult = Eresult,
                    pass = Pass
                };
                controller.Logout();
                return model;
            }

            //assert
            DatabaseContext Tcontext = testController.getContext();
            Patient P = Tcontext.Patients.FirstOrDefault(u => u.UserName.ToLower() == "Admin".ToLower());
            string Code = P.LinkCode;
            if (P == null)
            {
                Aresult = "Null";
            }
            else
            {
                Aresult = "a string with " + P.LinkCode.Length.ToString() + " characters and " + P.LinkUses.ToString() + " use(s)";
            }

            if (Aresult == Eresult)
            {
                Pass = true;
            }
            else
            {
                Pass = false;
            }

            model = new TestViewModel()
            {
                id = Id,
                time = DateTime.Now,
                description = Description,
                steps = Steps,
                criteria = Criteria,
                input = Inputstr,
                aresult = Aresult,
                eresult = Eresult,
                pass = Pass
            };
            controller.Logout();
            return model;
        }
    }
    internal class ConfirmAuthorizeCodeTest1 : PatientTest
    {
        public ConfirmAuthorizeCodeTest1(PatientController tc)
        {
            testController = tc;
            Id = "P5.Integration.CAC1";
            Description = "ConfirmAuthorizeCode test 1";
            Steps = "Make sure the link is not already made. Confirm authorize code whilst logged in.";
            Criteria = "The link must not already be made, and after the Act stage must be made.";
            Inputstr = "Correct link code";
            Aresult = "";
            Eresult = "Link correctly made";
        }

        public override TestViewModel Run()
        {
            TestViewModel model;

            //arrange
            bool Pass = false;
            PatientController controller = testController;
            DatabaseContext Tcontext = testController.getContext();
            //controller.Login("admin", "password", "patient");

            string code = Program.GenerateLinkCode();
            Patient patient = Tcontext.Patients.FirstOrDefault(u => u.UserName.ToLower() == "Admin2".ToLower());
            Patient loggedPatient = Tcontext.Patients.FirstOrDefault(u => u.UserName.ToLower() == "Admin".ToLower());
            if (loggedPatient == null)
            {
                Pass = false;
                Aresult = "User Admin does not exist";
                model = new TestViewModel()
                {
                    id = Id,
                    time = DateTime.Now,
                    description = Description,
                    steps = Steps,
                    criteria = Criteria,
                    input = Inputstr,
                    aresult = Aresult,
                    eresult = Eresult,
                    pass = Pass
                };
                controller.Logout();
                return model;
            }
            if (patient == null)
            {
                patient = new Patient()
                {
                    FirstName = "Admin2",
                    LastName = "Testaccount",
                    Email = "Admin2testaccmail@fakeemail.fa",
                    LocalId = new List<string>(),
                    PhoneNumber = "06-00000000",
                    UserName = "Admin2",
                    Password = Program.Hash256bits("Admin2" + "password"),
                    LinkCode = null,
                    LinkUses = 0,
                    CanSeeMeId = new List<int>(),
                    ICanSeeId = new List<int>()
                };
                Tcontext.Patients.Add(patient);
                Tcontext.SaveChanges();
            }
            else
            {
                //ensure the lists exist to avoid null errors
                if (patient.ICanSeeId == null)
                {
                    patient.ICanSeeId = new List<int>();
                }
                if (patient.CanSeeMeId == null)
                {
                    patient.CanSeeMeId = new List<int>();
                }
                if (loggedPatient.ICanSeeId == null)
                {
                    loggedPatient.ICanSeeId = new List<int>();
                }
                if (loggedPatient.CanSeeMeId == null)
                {
                    loggedPatient.CanSeeMeId = new List<int>();
                }

                if (patient.CanSeeMeId.Contains(loggedPatient.PatientId))
                {
                    patient.CanSeeMeId.Remove(loggedPatient.PatientId);
                    loggedPatient.ICanSeeId.Remove(patient.PatientId);
                }
            }
            controller.DbAddCode(code, 1,"Admin2");
            Tcontext.SaveChanges();


            //act
            try
            {
                controller.ConfirmAuthorization("admin2",code);
            }
            catch (Exception e)
            {
                Pass = false;
                Aresult = e.ToString();
                model = new TestViewModel()
                {
                    id = Id,
                    time = DateTime.Now,
                    description = Description,
                    steps = Steps,
                    criteria = Criteria,
                    input = Inputstr,
                    aresult = Aresult,
                    eresult = Eresult,
                    pass = Pass
                };
                controller.Logout();
                return model;
            }

            //assert
            Patient P = Tcontext.Patients.FirstOrDefault(u => u.UserName.ToLower() == "Admin".ToLower());
            if (P.ICanSeeId == null)
            {
                P.ICanSeeId = new List<int>();
            }
            List<int> idlist = P.ICanSeeId;
            if (P == null)
            {
                Aresult = "Null";
            }
            else
            {
                if (idlist.Contains(patient.PatientId))
                {
                    Aresult = "Link correctly made";
                }
                else
                {
                    Aresult = "Link between " + P.UserName + " and " + patient.UserName + " not made";
                }
            }

            if (Aresult == Eresult)
            {
                Pass = true;
            }
            else
            {
                Pass = false;
            }

            model = new TestViewModel()
            {
                id = Id,
                time = DateTime.Now,
                description = Description,
                steps = Steps,
                criteria = Criteria,
                input = Inputstr,
                aresult = Aresult,
                eresult = Eresult,
                pass = Pass
            };
            controller.Logout();
            return model;
        }
    }
    internal class ConfirmAuthorizeCodeTest2 : PatientTest
    {
        public ConfirmAuthorizeCodeTest2(PatientController tc)
        {
            testController = tc;
            Id = "P5.Integration.CAC2";
            Description = "ConfirmAuthorizeCode test 2";
            Steps = "Make sure the link is not already made. Confirm authorize code whilst logged in.";
            Criteria = "The link must not already be made, and after the Act stage must still not be made.";
            Inputstr = "Wrong link code";
            Aresult = "";
            Eresult = "Link not made";
        }

        public override TestViewModel Run()
        {
            TestViewModel model;

            //arrange
            bool Pass = false;
            PatientController controller = testController;
            DatabaseContext Tcontext = testController.getContext();
            //controller.Login("admin", "password", "patient");

            string code = Program.GenerateLinkCode();
            Patient patient = Tcontext.Patients.FirstOrDefault(u => u.UserName.ToLower() == "Admin2".ToLower());
            Patient loggedPatient = Tcontext.Patients.FirstOrDefault(u => u.UserName.ToLower() == "Admin".ToLower());
            if (loggedPatient == null)
            {
                Pass = false;
                Aresult = "User Admin does not exist";
                model = new TestViewModel()
                {
                    id = Id,
                    time = DateTime.Now,
                    description = Description,
                    steps = Steps,
                    criteria = Criteria,
                    input = Inputstr,
                    aresult = Aresult,
                    eresult = Eresult,
                    pass = Pass
                };
                controller.Logout();
                return model;
            }
            if (patient == null)
            {
                patient = new Patient()
                {
                    FirstName = "Admin2",
                    LastName = "Testaccount",
                    Email = "Admin2testaccmail@fakeemail.fa",
                    LocalId = new List<string>(),
                    PhoneNumber = "06-00000000",
                    UserName = "Admin2",
                    Password = Program.Hash256bits("Admin2" + "password"),
                    LinkCode = null,
                    LinkUses = 0,
                    CanSeeMeId = new List<int>(),
                    ICanSeeId = new List<int>()
                };
                Tcontext.Patients.Add(patient);
                Tcontext.SaveChanges();
            }
            else
            {
                //ensure the lists exist to avoid null errors
                if (patient.ICanSeeId == null)
                {
                    patient.ICanSeeId = new List<int>();
                }
                if (patient.CanSeeMeId == null)
                {
                    patient.CanSeeMeId = new List<int>();
                }
                if (loggedPatient.ICanSeeId == null)
                {
                    loggedPatient.ICanSeeId = new List<int>();
                }
                if (loggedPatient.CanSeeMeId == null)
                {
                    loggedPatient.CanSeeMeId = new List<int>();
                }

                if (patient.CanSeeMeId.Contains(loggedPatient.PatientId))
                {
                    patient.CanSeeMeId.Remove(loggedPatient.PatientId);
                    loggedPatient.ICanSeeId.Remove(patient.PatientId);
                }
            }
            controller.DbAddCode(code, 1, "Admin2");
            Tcontext.SaveChanges();


            //act
            try
            {
                controller.ConfirmAuthorization("admin2", "wrongcodestring");
            }
            catch (Exception e)
            {
                Pass = false;
                Aresult = e.ToString();
                model = new TestViewModel()
                {
                    id = Id,
                    time = DateTime.Now,
                    description = Description,
                    steps = Steps,
                    criteria = Criteria,
                    input = Inputstr,
                    aresult = Aresult,
                    eresult = Eresult,
                    pass = Pass
                };
                controller.Logout();
                return model;
            }

            //assert
            Patient P = Tcontext.Patients.FirstOrDefault(u => u.UserName.ToLower() == "Admin".ToLower());

            //ensure the lists exist to avoid null errors
            if (P.ICanSeeId == null)
            {
                P.ICanSeeId = new List<int>();
            }

            List<int> idlist = P.ICanSeeId;
            if (P == null)
            {
                Aresult = "Null";
            }
            else
            {
                if (idlist.Contains(patient.PatientId))
                {
                    Aresult = "Link correctly made";
                }
                else
                {
                    Aresult = "Link not made";
                }
            }

            if (Aresult == Eresult)
            {
                Pass = true;
            }
            else
            {
                Pass = false;
            }

            model = new TestViewModel()
            {
                id = Id,
                time = DateTime.Now,
                description = Description,
                steps = Steps,
                criteria = Criteria,
                input = Inputstr,
                aresult = Aresult,
                eresult = Eresult,
                pass = Pass
            };
            controller.Logout();
            return model;
        }
    }
    internal class ConfirmAuthorizeCodeTest3 : PatientTest
    {
        public ConfirmAuthorizeCodeTest3(PatientController tc)
        {
            testController = tc;
            Id = "P5.Integration.CAC3";
            Description = "ConfirmAuthorizeCode test 3";
            Steps = "Make sure there is no patient with username null. Confirm authorize code whilst logged in.";
            Criteria = "The link must not already be made, and after the Act stage must still not be made. No error thrown";
            Inputstr = "username = null";
            Aresult = "";
            Eresult = "No patient with name == null";
        }

        public override TestViewModel Run()
        {
            TestViewModel model;

            //arrange
            bool Pass = false;
            PatientController controller = testController;
            DatabaseContext Tcontext = testController.getContext();
            //controller.Login("admin", "password", "patient");

            string code = Program.GenerateLinkCode();
            Patient patient = Tcontext.Patients.FirstOrDefault(u => u.UserName.ToLower() == null);
            Patient loggedPatient = Tcontext.Patients.FirstOrDefault(u => u.UserName.ToLower() == "Admin".ToLower());
            if (loggedPatient == null)
            {
                Pass = false;
                Aresult = "User Admin does not exist";
                model = new TestViewModel()
                {
                    id = Id,
                    time = DateTime.Now,
                    description = Description,
                    steps = Steps,
                    criteria = Criteria,
                    input = Inputstr,
                    aresult = Aresult,
                    eresult = Eresult,
                    pass = Pass
                };
                controller.Logout();
                return model;
            }
            if (patient != null)
            {
                Tcontext.Patients.Remove(patient);
                Tcontext.SaveChanges();
            }
            /*else
            {
                if (patient.CanSeeMeId.Contains(loggedPatient.PatientId))
                {
                    patient.CanSeeMeId.Remove(loggedPatient.PatientId);
                    loggedPatient.ICanSeeId.Remove(patient.PatientId);
                }
            }*/
            //controller.DbAddCode(code, 1, "Admin2");
            Tcontext.SaveChanges();


            //act
            try
            {
                controller.ConfirmAuthorization(null, code);
            }
            catch (Exception e)
            {
                Pass = false;
                Aresult = e.ToString();
                model = new TestViewModel()
                {
                    id = Id,
                    time = DateTime.Now,
                    description = Description,
                    steps = Steps,
                    criteria = Criteria,
                    input = Inputstr,
                    aresult = Aresult,
                    eresult = Eresult,
                    pass = Pass
                };
                controller.Logout();
                return model;
            }

            //assert
            Patient P = Tcontext.Patients.FirstOrDefault(u => u.UserName.ToLower() == null);
            List<int> idlist;
            if (P == null)
            {
                Aresult = "No patient with name == null";
            }
            else
            {
                //ensure the lists exist to avoid null errors
                if (P.ICanSeeId == null)
                {
                    P.ICanSeeId = new List<int>();
                }
                if (P.CanSeeMeId == null)
                {
                    P.CanSeeMeId = new List<int>();
                }
                idlist = P.ICanSeeId;
                try
                {
                    if (idlist.Contains(patient.PatientId))
                    {
                        Aresult = "Link correctly made";
                    }
                    else
                    {
                        Aresult = "Link not made";
                    }
                }
                catch (Exception e)
                {
                    Pass = false;
                    Aresult = e.ToString();
                    model = new TestViewModel()
                    {
                        id = Id,
                        time = DateTime.Now,
                        description = Description,
                        steps = Steps,
                        criteria = Criteria,
                        input = Inputstr,
                        aresult = Aresult,
                        eresult = Eresult,
                        pass = Pass
                    };
                    controller.Logout();
                    return model;
                }
            }

            if (Aresult == Eresult)
            {
                Pass = true;
            }
            else
            {
                Pass = false;
            }

            model = new TestViewModel()
            {
                id = Id,
                time = DateTime.Now,
                description = Description,
                steps = Steps,
                criteria = Criteria,
                input = Inputstr,
                aresult = Aresult,
                eresult = Eresult,
                pass = Pass
            };
            controller.Logout();
            return model;
        }
    }
    internal class AuthorizationRevokeTest1 : PatientTest
    {
        public AuthorizationRevokeTest1(PatientController tc)
        {
            testController = tc;
            Id = "P5.Integration.AR1";
            Description = "AuthorizationRevoke Test 1";
            Steps = "Check if there is authorization between admin and admin2, then revoke it.";
            Criteria = "Authorization must be present, and after the act stage, be removed.";
            Inputstr = "id = admin2's Id.";
            Aresult = "";
            Eresult = "Link not made";
        }

        public override TestViewModel Run()
        {
            TestViewModel model;

            //arrange
            bool Pass = false;
            PatientController controller = testController;
            DatabaseContext Tcontext = testController.getContext();
            //controller.Login("admin", "password", "patient");

            string code = Program.GenerateLinkCode();
            Patient patient = Tcontext.Patients.FirstOrDefault(u => u.UserName.ToLower() == "Admin2".ToLower());
            Patient loggedPatient = Tcontext.Patients.FirstOrDefault(u => u.UserName.ToLower() == "Admin".ToLower());
            if (loggedPatient == null)
            {
                Pass = false;
                Aresult = "User Admin does not exist";
                model = new TestViewModel()
                {
                    id = Id,
                    time = DateTime.Now,
                    description = Description,
                    steps = Steps,
                    criteria = Criteria,
                    input = Inputstr,
                    aresult = Aresult,
                    eresult = Eresult,
                    pass = Pass
                };
                controller.Logout();
                return model;
            }
            if (patient == null)
            {
                patient = new Patient()
                {
                    FirstName = "Admin2",
                    LastName = "Testaccount",
                    Email = "Admin2testaccmail@fakeemail.fa",
                    LocalId = new List<string>(),
                    PhoneNumber = "06-00000000",
                    UserName = "Admin2",
                    Password = Program.Hash256bits("Admin2" + "password"),
                    LinkCode = null,
                    LinkUses = 0,
                    CanSeeMeId = new List<int>(),
                    ICanSeeId = new List<int>()
                };
                Tcontext.Patients.Add(patient);
                Tcontext.SaveChanges();
            }
            if (patient.ICanSeeId == null)
            {
                patient.ICanSeeId = new List<int>();
            }
            if (!(patient.ICanSeeId.Contains(loggedPatient.PatientId)))
            {
                patient.ICanSeeId.Add(loggedPatient.PatientId);
            }
            if (loggedPatient.CanSeeMeId == null)
            {
                loggedPatient.CanSeeMeId = new List<int>();
            }
            if (!(loggedPatient.CanSeeMeId.Contains(patient.PatientId)))
            {
                loggedPatient.CanSeeMeId.Add(patient.PatientId);
            }
            Tcontext.SaveChanges();

            //act
            try
            {
                controller.AuthorizationRevoke(patient.PatientId);
            }
            catch (Exception e)
            {
                Pass = false;
                Aresult = e.ToString();
                model = new TestViewModel()
                {
                    id = Id,
                    time = DateTime.Now,
                    description = Description,
                    steps = Steps,
                    criteria = Criteria,
                    input = Inputstr,
                    aresult = Aresult,
                    eresult = Eresult,
                    pass = Pass
                };
                controller.Logout();
                return model;
            }

            //assert
            Patient P = Tcontext.Patients.FirstOrDefault(u => u.PatientId == patient.PatientId);
            List<int> idlist = P.ICanSeeId;
            if (P == null)
            {
                Aresult = "Null";
            }
            else
            {
                if (idlist.Contains(loggedPatient.PatientId))
                {
                    Aresult = "Link made";
                }
                else
                {
                    Aresult = "Link not made";
                }
            }

            if (Aresult == Eresult)
            {
                Pass = true;
            }
            else
            {
                Pass = false;
            }

            model = new TestViewModel()
            {
                id = Id,
                time = DateTime.Now,
                description = Description,
                steps = Steps,
                criteria = Criteria,
                input = Inputstr,
                aresult = Aresult,
                eresult = Eresult,
                pass = Pass
            };
            controller.Logout();
            return model;
        }
    }
    internal class AuthorizationRevokeTest2 : PatientTest
    {
        public AuthorizationRevokeTest2(PatientController tc)
        {
            testController = tc;
            Id = "P5.Integration.AR2";
            Description = "AuthorizationRevoke Test 2";
            Steps = "Make sure there is no authorization between admin and admin2, then try to revoke it.";
            Criteria = "Authorization must not be present, and after the act stage, still not be present.";
            Inputstr = "id = admin2's Id.";
            Aresult = "";
            Eresult = "Link not made";
        }

        public override TestViewModel Run()
        {
            TestViewModel model;

            //arrange
            bool Pass = false;
            PatientController controller = testController;
            DatabaseContext Tcontext = testController.getContext();
            //controller.Login("admin", "password", "patient");

            string code = Program.GenerateLinkCode();
            Patient patient = Tcontext.Patients.FirstOrDefault(u => u.UserName.ToLower() == "Admin2".ToLower());
            Patient loggedPatient = Tcontext.Patients.FirstOrDefault(u => u.UserName.ToLower() == "Admin".ToLower());
            if (loggedPatient == null)
            {
                Pass = false;
                Aresult = "User Admin does not exist";
                model = new TestViewModel()
                {
                    id = Id,
                    time = DateTime.Now,
                    description = Description,
                    steps = Steps,
                    criteria = Criteria,
                    input = Inputstr,
                    aresult = Aresult,
                    eresult = Eresult,
                    pass = Pass
                };
                controller.Logout();
                return model;
            }
            if (patient == null)
            {
                patient = new Patient()
                {
                    FirstName = "Admin2",
                    LastName = "Testaccount",
                    Email = "Admin2testaccmail@fakeemail.fa",
                    LocalId = new List<string>(),
                    PhoneNumber = "06-00000000",
                    UserName = "Admin2",
                    Password = Program.Hash256bits("Admin2" + "password"),
                    LinkCode = null,
                    LinkUses = 0,
                    CanSeeMeId = new List<int>(),
                    ICanSeeId = new List<int>()
                };
                Tcontext.Patients.Add(patient);
                Tcontext.SaveChanges();
            }
            if (patient.ICanSeeId == null)
            {
                patient.ICanSeeId = new List<int>();
            }
            else if (patient.ICanSeeId.Contains(loggedPatient.PatientId))
            {
                patient.ICanSeeId.Remove(loggedPatient.PatientId);
            }
            if (loggedPatient.CanSeeMeId == null)
            {
                loggedPatient.CanSeeMeId = new List<int>();
            }
            else if (loggedPatient.CanSeeMeId.Contains(patient.PatientId))
            {
                loggedPatient.CanSeeMeId.Remove(patient.PatientId);
            }
            Tcontext.SaveChanges();

            //act
            try
            {
                controller.AuthorizationRevoke(patient.PatientId);
            }
            catch (Exception e)
            {
                Pass = false;
                Aresult = e.ToString();
                model = new TestViewModel()
                {
                    id = Id,
                    time = DateTime.Now,
                    description = Description,
                    steps = Steps,
                    criteria = Criteria,
                    input = Inputstr,
                    aresult = Aresult,
                    eresult = Eresult,
                    pass = Pass
                };
                controller.Logout();
                return model;
            }

            //assert
            Patient P = Tcontext.Patients.FirstOrDefault(u => u.PatientId == patient.PatientId);
            List<int> idlist = P.ICanSeeId;
            if (P == null)
            {
                Aresult = "Null";
            }
            else
            {
                if (idlist.Contains(loggedPatient.PatientId))
                {
                    Aresult = "Link made";
                }
                else
                {
                    Aresult = "Link not made";
                }
            }

            if (Aresult == Eresult)
            {
                Pass = true;
            }
            else
            {
                Pass = false;
            }

            model = new TestViewModel()
            {
                id = Id,
                time = DateTime.Now,
                description = Description,
                steps = Steps,
                criteria = Criteria,
                input = Inputstr,
                aresult = Aresult,
                eresult = Eresult,
                pass = Pass
            };
            controller.Logout();
            return model;
        }
    }
    internal class AuthorizationRevokeTest3 : PatientTest
    {
        public AuthorizationRevokeTest3(PatientController tc)
        {
            testController = tc;
            Id = "P5.Integration.AR3";
            Description = "AuthorizationRevoke Test 3";
            Steps = "Try to revoke authorization for a patient that does not exist.";
            Criteria = "Authorization must not be present after the act stage, and no new patient object must be created in db with the nonexistend Id.";
            Inputstr = "id = -10";
            Aresult = "";
            Eresult = "patient with id -10 is null";
        }

        public override TestViewModel Run()
        {
            TestViewModel model;

            //arrange
            bool Pass = false;
            PatientController controller = testController;
            DatabaseContext Tcontext = testController.getContext();
            //controller.Login("admin", "password", "patient");

            int revokeId = -10;

            string code = Program.GenerateLinkCode();
            Patient loggedPatient = Tcontext.Patients.FirstOrDefault(u => u.UserName.ToLower() == "Admin".ToLower());
            if (loggedPatient == null)
            {
                Pass = false;
                Aresult = "User Admin does not exist";
                model = new TestViewModel()
                {
                    id = Id,
                    time = DateTime.Now,
                    description = Description,
                    steps = Steps,
                    criteria = Criteria,
                    input = Inputstr,
                    aresult = Aresult,
                    eresult = Eresult,
                    pass = Pass
                };
                return model;
            }
            if (loggedPatient.ICanSeeId.Contains(revokeId))
            {
                loggedPatient.ICanSeeId.Remove(revokeId);
            }
            Tcontext.SaveChanges();

            //act
            try
            {
                controller.AuthorizationRevoke(revokeId);
            }
            catch (Exception e)
            {
                Pass = false;
                Aresult = e.ToString();
                model = new TestViewModel()
                {
                    id = Id,
                    time = DateTime.Now,
                    description = Description,
                    steps = Steps,
                    criteria = Criteria,
                    input = Inputstr,
                    aresult = Aresult,
                    eresult = Eresult,
                    pass = Pass
                };
                controller.Logout();
                return model;
            }

            //assert
            Patient P = Tcontext.Patients.FirstOrDefault(u => u.PatientId == revokeId);
            if (P == null)
            {
                Aresult = "patient with id -10 is null";
            }
            else
            {
                Aresult = "UserName=" + P.UserName + " | FirstName=" + P.FirstName;
            }

            if (Aresult == Eresult)
            {
                Pass = true;
            }
            else
            {
                Pass = false;
            }

            model = new TestViewModel()
            {
                id = Id,
                time = DateTime.Now,
                description = Description,
                steps = Steps,
                criteria = Criteria,
                input = Inputstr,
                aresult = Aresult,
                eresult = Eresult,
                pass = Pass
            };
            controller.Logout();
            return model;
        }
    }
}
