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
using Microsoft.AspNetCore.Mvc.Filters;

namespace zorgapp.Controllers{
    public class TestController : Controller{
        private readonly DatabaseContext _context;
        private List<Test> testlist;

        public TestController(DatabaseContext context)
        {
            _context = context;
            testlist = new List<Test>();
            {
                testlist.Add(new SubmitDoctorAccountTest1(this));
                testlist.Add(new SubmitDoctorAccountTest2(this));
                testlist.Add(new SubmitDoctorAccountTest3(this));
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

            List<Tuple<string,string>> tupleList = new List<Tuple<string,string>>();

            foreach (Test T in testlist)
            {
                tupleList.Add(new Tuple<string, string>(T.Id, T.Id));
            }

            TestListViewModel testlistmodel = new TestListViewModel { tuples = tupleList};
            return View(testlistmodel);
        }

        public IActionResult StartTest(string TestId)
        {
            Test testobj = testlist.FirstOrDefault();
            foreach(Test T in testlist)
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
            Inputstr = "All fields filled in";
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
            string eMail = "HarryJ1@email.com";
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
            catch(Exception)
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
                Aresult = "UserName="+doc.UserName+" | FirstName="+doc.FirstName;
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
            Inputstr = "All fields, except username=null";
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
            string eMail = "HarryJ2@email.com";
            string phoneNumber = "06-69696969";
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
    /*internal class GenerateNewAuthorizeCodeTest1 : Test
    {
        public GenerateNewAuthorizeCodeTest1(TestController tc)
        {
            testController = tc;
            Id = "P5.Integration.GNAC1";
            Description = "GenerateNewAuthorizeCode test 1";
            Steps = "";
            Criteria = "";
            Inputstr = "";
            Aresult = "";
            Eresult = "a string with 32 characters";
        }

        public override TestViewModel Run()
        {
            TestViewModel model;

            //arrange
            bool Pass = false;
            PatientController controller = new PatientController(testController.getContext());

            int uses = 3;

            var claims = new List<Claim>
                                {
                                    new Claim(ClaimTypes.Name, "Patient", ClaimValueTypes.String),
                                    new Claim(ClaimTypes.NameIdentifier, "Admin", ClaimValueTypes.String),
                                    new Claim(ClaimTypes.Role, "Patient", ClaimValueTypes.String)
                                };
            var userIdentity = new ClaimsIdentity(claims, "SecureLogin");
            var userPrincipal = new ClaimsPrincipal(userIdentity);

            //act
            try
            {
            controller.GenerateNewCode(uses);
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
            Patient P = Tcontext.Patients.FirstOrDefault(u => u.UserName == "Admin");
            string Code = P.LinkCode;
            if (P == null)
            {
                Aresult = "Null";
            }
            else
            {
                Aresult = "a string with "+P.LinkCode.Length.ToString()+" characters";
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
    }*/
}
