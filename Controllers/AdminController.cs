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

            //take the link in the db where doctor and patient are linked
            PatientsDoctors patientsDoctors_ = _context.PatientsDoctorss.FirstOrDefault(
                p => p.PatientId == patientid && p.DoctorId == doctorid
            );

            //check if the link is already made
            bool linkmade = _context.PatientsDoctorss.Contains(patientsDoctors_);

            PatientsDoctors patientsDoctors = new PatientsDoctors(){
                PatientId = patientid,
                DoctorId = doctorid
            };
            //check if patient exists
             if(patient == null)
            {
                if (TempData != null)
                {TempData["message"] = "PatientId does not exist";}
                return RedirectToAction("Link", "Admin");
            }
            //check if doctor exists
            if(doctor == null)
            {
                if (TempData != null)
                {TempData["message"] = "Doctorid does not exist";}
                return RedirectToAction("Link", "Admin");
            }

            //when both doctor and patient exist and link is not made, add link to db
            if(!linkmade){
                _context.PatientsDoctorss.Add(patientsDoctors);
                _context.SaveChanges();
            }
            
            //if link is already made, redirect to same page with text to warn that the link is already made
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



