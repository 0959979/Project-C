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
using zorgapp.Controllers;

namespace zorgapp.Controllers
{

    public class TestController : Controller
    {
        private readonly DatabaseContext _context;
        private List<Test> testlist;

        public TestController(DatabaseContext context)
        {
            _context = context;
            testlist = new List<Test>();
            {
                testlist.Add(new SubmitDoctorAccountTest1(this));
                testlist.Add(new SubmitDoctorAccountTest2(this));
                testlist.Add(new SubmitDoctorAccountTest3(this));
                testlist.Add(new LinkAdminLinkAlreadyMadeTest1(this));
                testlist.Add(new LinkAdminLinkNotMadeTest1(this));
                testlist.Add(new LinkAdminPatNullTest1(this));
                testlist.Add(new LinkAdminDocNullTest1(this));
            }
        }

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
            List<Tuple<string, string>> tupleList = new List<Tuple<string, string>>();

            foreach (Test T in testlist)
            {
                tupleList.Add(new Tuple<string, string>(T.Id, T.Id));
            }

            TestListViewModel testlistmodel = new TestListViewModel { tuples = tupleList };
            return View(testlistmodel);
        }

        public IActionResult StartTest(string TestId)
        {
            Test testobj = testlist.FirstOrDefault();
            foreach (Test T in testlist)
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

    internal abstract class Test
    {
        public abstract TestViewModel Run();
        public TestController testController;
        public string Id;
        public string Description;
        public string Steps;
        public string Criteria;
        public string Inputstr;
        public string Aresult;
        public string Eresult;
        public bool Pass;
    }
    internal class SubmitDoctorAccountTest1 : Test
    {
        public SubmitDoctorAccountTest1(TestController tc)
        {
            testController = tc;
            Id = "A1.Integration.SDA1";
            Description = "SubmitDoctorAccount test 1";
            Steps = "";
            Criteria = "";
            Inputstr = "";
            Aresult = "";
            Eresult = "UserName=UserAT1 | FirstName=Harry";
        }

        public override TestViewModel Run()
        {
            TestViewModel model;

            //arrange
            bool Pass = false;
            DoctorController controller = new DoctorController(testController.getContext());

            string firstName = "Harry";
            string lastName = "Jones";
            string eMail = "HarryJ@email.com";
            string phoneNumber = "06-24685344";
            string specialism = "Bone fractures";
            string localId = "2515T1";
            string userName = "UserAT1";
            string password = "password12342";

            //act
            try
            {
                controller.SubmitDoctorAccount(firstName, lastName, eMail, phoneNumber, specialism, localId, userName, password);
            }
            catch (Exception)
            {
                Pass = false;
            }

            //assert
            DatabaseContext Tcontext = testController.getContext();
            Doctor doc = Tcontext.Doctors.FirstOrDefault(u => u.UserName == userName);
            if (doc == null)
            {
                Pass = false;
            }
            else
            {
                Aresult = "UserName=" + doc.UserName + " | FirstName=" + doc.FirstName;
            }
            if (Aresult == Eresult)
            {
                Pass = true;
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

    internal class SubmitDoctorAccountTest2 : Test
    {
        public SubmitDoctorAccountTest2(TestController tc)
        {
            testController = tc;
            Id = "A1.Integration.SDA2";
            Description = "SubmitDoctorAccount test 2";
            Steps = "";
            Criteria = "";
            Inputstr = "";
            Aresult = "";
            Eresult = "Null";
        }

        public override TestViewModel Run()
        {
            TestViewModel model;

            //arrange
            bool Pass = false;
            DoctorController controller = new DoctorController(testController.getContext());

            string firstName = "Harry";
            string lastName = "Jones";
            string eMail = "HarryJ@email.com";
            string phoneNumber = "06-24685344";
            string specialism = "Bone fractures";
            string localId = "2515T2";
            string userName = null;
            string password = "password12342";

            //act
            try
            {
                controller.SubmitDoctorAccount(firstName, lastName, eMail, phoneNumber, specialism, localId, userName, password);
            }
            catch (Exception)
            {
                Pass = false;
                Aresult = "Exception Error.";
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
            Doctor doc = Tcontext.Doctors.FirstOrDefault(u => u.UserName == userName);
            if (doc == null)
            {
                Aresult = "Null";
            }
            else
            {
                Aresult = "UserName=" + doc.UserName + " | FirstName=" + doc.FirstName;
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
    internal class SubmitDoctorAccountTest3 : Test
    {
        public SubmitDoctorAccountTest3(TestController tc)
        {
            testController = tc;
            Id = "A1.Integration.SDA3";
            Description = "SubmitDoctorAccount test 3";
            Steps = "";
            Criteria = "";
            Inputstr = "";
            Aresult = "";
            Eresult = "Null";
        }

        public override TestViewModel Run()
        {
            TestViewModel model;

            //arrange
            bool Pass = false;
            DoctorController controller = new DoctorController(testController.getContext());

            string firstName = null;
            string lastName = "Tester";
            string eMail = "TestmanT3@email.com";
            string phoneNumber = "06-24685344";
            string specialism = "Bone fractures";
            string localId = "2515T3";
            string userName = "UserAT3";
            string password = "password12342";

            //act
            try
            {
                controller.SubmitDoctorAccount(firstName, lastName, eMail, phoneNumber, specialism, localId, userName, password);
            }
            catch (Exception)
            {
                Pass = false;
                Aresult = "Exception Error.";
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
            Doctor doc = Tcontext.Doctors.FirstOrDefault(u => u.UserName == userName);
            if (doc == null)
            {
                Aresult = "Null";
            }
            else
            {
                Aresult = "UserName=" + doc.UserName + " | FirstName=" + doc.FirstName;
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
    internal class LinkAdminLinkAlreadyMadeTest1 : Test
    {
        public LinkAdminLinkAlreadyMadeTest1(TestController tc)
        {
            testController = tc;
            Id = "A2.Integration.LAT1";
            Description = "Link Admin Link Already Made test 1";
            Steps = "Check if link is made, no changes to database";
            Criteria = "Pass: no changes to database | Fail: double link";
            Inputstr = "PatientID and doctorID where link is already made";
            Aresult = "";
            Eresult = "Link not doubled";
        }

        public override TestViewModel Run()
        {
            TestViewModel model;

            //arrange
            bool Pass = false;
            AdminController controller = new AdminController(testController.getContext());

            int PatientID = 1;
            int DocterID = 1;

            // act
            try
            {
                controller.SubmitLink(PatientID, DocterID);
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
            var patientsDoctors = from d in Tcontext.PatientsDoctorss where d.DoctorId == DocterID select d;
            Patient patient = Tcontext.Patients.FirstOrDefault(x => x.PatientId == PatientID);
            List<int> list = new List<int>();

            foreach (var item in patientsDoctors)
            {
                if (item.PatientId == patient.PatientId)
                {
                    list.Add(1);

                };
            }
            //check if the list of patientsdoctors contains the link twice
            if (list.Count() > 1)
            {
                Aresult = "Link doubled";
            }
            else
            {
                Aresult = "Link not doubled";
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
    internal class LinkAdminLinkNotMadeTest1 : Test
    {
        public LinkAdminLinkNotMadeTest1(TestController tc)
        {
            testController = tc;
            Id = "A2.Integration.LAT2";
            Description = "Link Admin Link not made";
            Steps = "Check if link is made, if not, add link";
            Criteria = "Pass: link added in database | Fail: no link made";
            Inputstr = "PatientID and doctorID where link is not already made";
            Aresult = "";
            Eresult = "Link made";
        }

        public override TestViewModel Run()
        {
            TestViewModel model;

            //arrange
            bool Pass = false;
            DatabaseContext Tcontext = testController.getContext();
            AdminController controller = new AdminController(testController.getContext());
            int PatientID = 1;
            int DoctorID = 1;
            Doctor doctor = Tcontext.Doctors.FirstOrDefault(x => x.DoctorId == DoctorID);

            Patient patient = Tcontext.Patients.FirstOrDefault(p => p.PatientId == PatientID);
            var patients = from d in Tcontext.Patients select d;
            bool patientExists = patients.Contains(patient);

            PatientsDoctors patientsDoctors_ = Tcontext.PatientsDoctorss.FirstOrDefault(
            p => p.PatientId == PatientID && p.DoctorId == doctor.DoctorId);
            bool linkmade = Tcontext.PatientsDoctorss.Contains(patientsDoctors_);

            //loops untill patient is found where doc is not already linked
            for (int i = 1; linkmade; i++)
            {
                PatientID = i;
                patientsDoctors_ = Tcontext.PatientsDoctorss.FirstOrDefault(
                p => p.PatientId == PatientID && p.DoctorId == doctor.DoctorId);
                linkmade = Tcontext.PatientsDoctorss.Contains(patientsDoctors_);

                patient = Tcontext.Patients.FirstOrDefault(p => p.PatientId == PatientID);
            // if all patients are linked, the loop is stopped
                if (patient == null)
                {
                    break;
                }
            }
            //if all patients are linked, the results below are viewed
            if (patient == null)
            {
                Pass = false;
                Aresult = "All patients are linked to the doctor";
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
            //if there is a patient that is not already linked, run submitlink method
            try
            {
                controller.SubmitLink(PatientID, DoctorID);
            }
            //if for some reason we get an error, the error will be viewed 
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
            var patientsDoctors = from d in Tcontext.PatientsDoctorss where d.DoctorId == DoctorID select d;
            Patient patient_ = Tcontext.Patients.FirstOrDefault(x => x.PatientId == PatientID);

            //check if link is made in the database
            foreach (var item in patientsDoctors)
            {
                if (item.PatientId == patient_.PatientId)
                {
                    Aresult = "Link made for Patientid:" + patient_.PatientId + " and Doctorid:" + DoctorID;
                    Pass = true;
                }
                else
                {
                    Aresult = "Link not made";
                    Pass = false;
                }
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
     internal class LinkAdminPatNullTest1 : Test
    {
        public LinkAdminPatNullTest1(TestController tc)
        {
            testController = tc;
            Id = "A2.Integration.LAT3";
            Description = "Link Admin Patient is null";
            Steps = "Check if patient does not exist, try to create link";
            Criteria = "Pass: link is not made, warning to admin that patient does not exist| Fail: exeption error";
            Inputstr = "PatientID of a non existent patient, DoctorID of existing doctor  ";
            Aresult = "";
            Eresult = "Patient does not exist";
        }

        public override TestViewModel Run()
        {
            TestViewModel model;

            //arrange
            bool Pass = false;
            DatabaseContext Tcontext = testController.getContext();
            AdminController controller = new AdminController(testController.getContext());
            int PatientID = 1;
            int DoctorID = 1;
            Doctor doctor = Tcontext.Doctors.FirstOrDefault(x => x.DoctorId == DoctorID);

            Patient patient = Tcontext.Patients.FirstOrDefault(p => p.PatientId == PatientID);
            var patients = from d in Tcontext.Patients select d;
            bool patientExists = true;
            if (patient == null){
                patientExists = false;
            }

            // PatientsDoctors patientsDoctors_ = Tcontext.PatientsDoctorss.FirstOrDefault(
            // p => p.PatientId == PatientID && p.DoctorId == doctor.DoctorId);
            // bool linkmade = Tcontext.PatientsDoctorss.Contains(patientsDoctors_);

            //loops untill patient is found that doesnt exist 
            for (int i = 1; patientExists; i++)
            {
                PatientID = i;
                patient = Tcontext.Patients.FirstOrDefault(p => p.PatientId == PatientID);
                if (patient == null)
                {
                patientExists = false;
                }
            }

            //try to run the submitlink method with a non existing patient 
            try
            {
                controller.SubmitLink(PatientID, DoctorID);
            }
            //if for some reason we get an error, the error will be viewed 
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
            var patientsDoctors = from d in Tcontext.PatientsDoctorss where d.DoctorId == DoctorID select d;
            Patient patient_ = Tcontext.Patients.FirstOrDefault(x => x.PatientId == PatientID);
            
            //check if link is made in the database
            foreach (var item in patientsDoctors)
            {   
                if (patientExists == true)
            {
                if (item.PatientId == patient_.PatientId)
                {
                    Aresult = "Link made for Patientid:" + patient_.PatientId + " and Doctorid:" + DoctorID;
                    Pass = false;
                }
            }
                else
                {
                    Aresult = "Link not made";
                    Pass = true;
                }
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
         internal class LinkAdminDocNullTest1 : Test
    {
        public LinkAdminDocNullTest1(TestController tc)
        {
            testController = tc;
            Id = "A2.Integration.LAT4";
            Description = "Link Admin Doctor is null";
            Steps = "Check if doctor does not exist, try to create link";
            Criteria = "Pass: link is not made, warning to admin that doctor does not exist| Fail: exeption error";
            Inputstr = "DoctorID of a non existent patient, PatientID of existing doctor  ";
            Aresult = "";
            Eresult = "Doctor does not exist";
        }

        public override TestViewModel Run()
        {
            TestViewModel model;

            //arrange
            bool Pass = false;
            DatabaseContext Tcontext = testController.getContext();
            AdminController controller = new AdminController(testController.getContext());
            int PatientID = 1;
            int DoctorID = 1;
            Doctor doctor = Tcontext.Doctors.FirstOrDefault(x => x.DoctorId == DoctorID);
            Patient patient = Tcontext.Patients.FirstOrDefault(p => p.PatientId == PatientID);

            var doctors = from d in Tcontext.Doctors select d;
            bool doctorExists = true;
            if (doctor == null){
                doctorExists = false;
            }


            //loops untill doctor is found that doesnt exist 
            for (int i = 1; doctorExists; i++)
            {
                DoctorID = i;
                doctor = Tcontext.Doctors.FirstOrDefault(p => p.DoctorId == DoctorID);
                if (doctor == null)
                {
                doctorExists = false;
                }
            }

            //try to run the submitlink method with a non existing doctor 
            try
            {
                controller.SubmitLink(PatientID, DoctorID);
            }
            //if for some reason we get an error, the error will be viewed 
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
            var patientsDoctors = from d in Tcontext.PatientsDoctorss where d.PatientId == PatientID select d;
            Doctor doctor_ = Tcontext.Doctors.FirstOrDefault(x => x.DoctorId == DoctorID);
            
            //check if link is made in the database
            foreach (var item in patientsDoctors)
            {   
                if (doctorExists == true)
            {
                if (item.DoctorId == doctor_.DoctorId)
                {
                    Aresult = "Link made for Patientid:" + PatientID + " and Doctorid:" + doctor_.DoctorId;
                    Pass = false;
                }
            }
                else
                {
                    Aresult = "Link not made";
                    Pass = true;
                }
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

}
