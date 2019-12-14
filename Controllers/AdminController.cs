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

        public ActionResult Link(){
            if (TempData["message"]!= null){
            ViewBag.Message = TempData["message"].ToString();
            TempData.Remove("message");
            }
            return View();
        }
    


      //  links patient with doctor 
 
        public ActionResult SubmitLink(int patientid, int doctorid){
            Doctor doctor = _context.Doctors.FirstOrDefault(m => m.DoctorId == doctorid);
            Patient patient = _context.Patients.FirstOrDefault(y => y.PatientId == patientid);

            PatientsDoctors patientsDoctors_ = _context.PatientsDoctorss.FirstOrDefault(
                p => p.PatientId == patientid && p.DoctorId == doctorid
            );

            bool linkmade = _context.PatientsDoctorss.Contains(patientsDoctors_);

            PatientsDoctors patientsDoctors = new PatientsDoctors(){
                PatientId = patientid,
                DoctorId = doctorid
            };
             if(patient == null)
            {
                if (TempData != null)
                {TempData["message"] = "PatientId does not exist";}
                return RedirectToAction("Link", "Admin");
            }
            if(doctor == null)
            {
                if (TempData != null)
                {TempData["message"] = "Doctorid does not exist";}
                return RedirectToAction("Link", "Admin");
            }

            if(!linkmade){
                _context.PatientsDoctorss.Add(patientsDoctors);
                _context.SaveChanges();
            }
            else if(linkmade){
                if (TempData != null)
                    {TempData["message"] = "Link has already been made";}
                return RedirectToAction("Link", "Admin");
            }
            
            ViewData["Doctor"] = doctor.FirstName;
            ViewData["Patient"] = patient.FirstName;

            return View();
        }

    }
    
}



