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
        public IActionResult SubmitPatientAccount(string firstname, string lastname, string email,string phonenumber, string username, string password)
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
                    LocalId = new List<string>(),
                    PhoneNumber = phonenumber,
                    UserName = username,
                    Password = Program.Hash256bits(username+password),
                    LinkCode = null,
                    LinkUses = 0,
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
     //   Authorizes the page so only users with the role Patient can view it
        [Authorize(Roles = "Admin")]
        public IActionResult PatientList() 
        {
            var patients = from p in _context.Patients select p;

            return View(patients);
        }
      public IActionResult DoctorList()
        {
            string username_ = User.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value;
            Patient patient = _context.Patients.FirstOrDefault(u => u.UserName == username_);
            int patid = patient.PatientId;
            var Doc = new List<Doctor>();
            var patientsDoctors = from d in _context.PatientsDoctorss where d.PatientId == patid select d;

            foreach (var item in patientsDoctors)
            {   
                if (item.DoctorId != null){
                var doctor_ = _context.Doctors.FirstOrDefault(p => p.DoctorId == item.DoctorId);
                Doc.Add(doctor_);
                };

            }
            return View(Doc);
        }
        public IActionResult DocProfile (IFormCollection form)
        {
            string id = form["doctorid"].ToString();
            int id_ = int.Parse(id);
            Doctor doctor = _context.Doctors.FirstOrDefault(u => u.DoctorId == id_);
            
            return View(doctor);
        }



        public IActionResult AddLocalId (int patientid, string localid){
            Patient patient = _context.Patients.FirstOrDefault(u => u.PatientId == patientid);
            patient.LocalId.Add(localid);
            _context.SaveChanges();
            
            return RedirectToAction("PatientList","Doctor");
        }


        [Authorize(Roles = "Patient")]
        public ActionResult Agenda(string Previous, int dayoffset, string Next, int starthour, int endhour, string Apply)
        {
            {
                System.Diagnostics.Debug.WriteLine("starthour: " + starthour.ToString());
                System.Diagnostics.Debug.WriteLine("endhour: " + endhour.ToString());

                if (!string.IsNullOrEmpty(Next))
                {
                    dayoffset += 7;
                    //ViewBag.Recieved = "Next, dayoffset = "+dayoffset.ToString();
                }
                else if (!string.IsNullOrEmpty(Previous))
                {
                    dayoffset -= 7;
                    //ViewBag.Recieved = "Previous, dayoffset = " + dayoffset.ToString();
                }
                else if (!string.IsNullOrEmpty(Apply))
                {
                    dayoffset += 0;
                    //ViewBag.Recieved = "Previous, dayoffset = " + dayoffset.ToString();
                }
                else
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
                    FWeekday = FWeekday.AddDays(-offset);
                    for (d = 0; d < 7; d++)
                    {
                        DateTime day = Today;//DateTime.Now;
                        day = day.AddDays(d - offset);
                        Day.Add(day.DayOfWeek.ToString());
                        Date.Add(day.Date.ToShortDateString());
                        //System.Diagnostics.Debug.WriteLine("RonanDayList: " + day.ToString());
                    }
                }
                //The hours and minutes
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
                //The appointments of that week
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
                    foreach (var item in cases)
                    {
                        var appointment = from c in _context.Appointments where c.CaseId == item.CaseId select c;
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
            //string Username = username;
            //string Password = password;
            //var UserL = from u in _context.Patients where u.UserName == Username select u;   
            
            var adminexists = _context.Admins.Any(x => x.UserName == "admin");
            if(!adminexists){
            Admin admin = new Admin(){
                UserName = "admin",
                Password = Program.Hash256bits("adminpassword")
            };

            _context.Admins.Add(admin);
            _context.SaveChanges();
            }

                     
            if (username != null && password != null)
            {   string pwhash = Program.Hash256bits(username+password);
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
            DbAddCode(code, Uses);
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
                            ViewBag.Message1 = "You have succesfully been authorized to view " + targetNew.UserName + "'s profile.\nTheir authorization code has been cleared.";
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
                patient.LinkCode = code;
                patient.LinkUses = uses;
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
        public bool CheckAuthorizeCode(string code, string username)
        {
            Patient patient = _context.Patients.FirstOrDefault(u => u.UserName == username);
            if (patient == null)
            {
                throw new Exception("Trying to check code for non-existent patient using username: " + username);
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
                            patient.LinkCode = null;
                            patient.LinkUses = 0;
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

        public ActionResult AuthorizationList()
        {
            string username = User.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value;
            Patient user = _context.Patients.FirstOrDefault(u => u.UserName == username);
            int id = user.PatientId;

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
    }
}
