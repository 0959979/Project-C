using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using zorgapp.Models;

namespace zorgapp.Controllers{
    public class AdminController : Controller{
        private readonly DatabaseContext _context;

        public AdminController(DatabaseContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "Admin")]
        public ActionResult Profile(){
            return View();
        }

        [Authorize(Roles = "Admin")]
        public ActionResult Link(){
            if (TempData["message"]!= null){
            ViewBag.Message = TempData["message"].ToString();
            TempData.Remove("message");
            }
            return View();
        }

        [Authorize(Roles = "Admin")]
        //links patient with doctor but does not check if already linked yet
        public ActionResult SubmitLink(int patientid, int doctorid){
            Doctor doctor = _context.Doctors.FirstOrDefault(m => m.DoctorId == doctorid);
            Patient patient = _context.Patients.FirstOrDefault(y => y.PatientId == patientid);
            int patientid_ = patientid;
            int doctorid_ = doctorid;
            string docName = doctor.FirstName;
            string patName = patient.FirstName;

            bool DocContains = doctor.PatientIds.Contains(patientid_);
            bool PatContains = patient.DoctorIds.Contains(doctorid_);

            if(!DocContains){
                doctor.PatientIds.Add(patientid_);
                patient.DoctorIds.Add(doctorid_);
                _context.SaveChanges();
            }
            else if(DocContains){
                TempData["message"] = "Link has already been made";
                return RedirectToAction("Link","Admin");
            }

            ViewData["Doctor"] = docName;
            ViewData["Patient"] = patName;

            return View();
        }

    //    public ActionResult Login(string username, string password)
    //     {
      //  var adminexists = _context.Admins.Any(x => x.UserName == "admin");
    //         if(!adminexists){
    //         Admin admin = new Admin(){
    //             UserName = "admin",
    //             Password = "password"
    //         };

    //         _context.Admins.Add(admin);
    //         _context.SaveChanges();
    //         }


    //         Admin user = _context.Admins.FirstOrDefault(u => u.UserName == username);
    //         if (user != null)
    //         {
    //             if (user.Password == password)
    //             {
    //                 return RedirectToAction("Profile", "Admin");
    //             }
    //             else
    //             {
    //                 ViewBag.emptyfield = "Username or Password is incorrect";
    //             }
    //         }
    //         else if (username != null)
    //         {
    //             ViewBag.emptyfield = "Username or Password is incorrect";
    //         }
    //         return View();
    //     }

    // }
}
}