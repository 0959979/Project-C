using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
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
       
    //     public IActionResult CreateAccount(string firstname, string lastname, string email,int phonenumber,string specialism, string username, string password)
    //     {
    //         Doctor doctor = new Doctor()
    //         {
    //             FirstName = firstname,
    //             LastName = lastname,
    //             Email = email,
    //             PhoneNumber = phonenumber,
    //             Specialism = specialism,
    //             Username = username,
    //             Password = password
    //         };
    //         _context.Doctors.Add(doctor);
    //         _context.SaveChanges();

    //         ViewData["FirstName"] = doctor.FirstName;
    //         ViewData["LastName"] = doctor.LastName;


    //         return View();

    //     }

                



    }
    }