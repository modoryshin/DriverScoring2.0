using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text.RegularExpressions;
using System.Configuration;

namespace DriverScoring.Controllers
{
    public class HomeController : Controller
    {
        static ConnectionStringSettings c = ConfigurationManager.ConnectionStrings["mainEntitiesDB"];
        static string fixedConnectionString = c.ConnectionString.Replace("{AppDir}", AppDomain.CurrentDomain.BaseDirectory);

        public static DBModels.mainEntitiesDB db = new DBModels.mainEntitiesDB(fixedConnectionString);
        static DBModels.Пользователи currentuser;
        static string usertype = "";
        public static long RequestIdent;
        public ActionResult Index()
        {
            /*
             * 
             * не получилось нормальный unit-test для контекста БД сделать пока. Поэтому через контроллер запускал. Эта штука работает и добавляет тестового пользователя
            
            DriverScoring.DBModels.mainEntitiesDB contextDB = new DriverScoring.DBModels.mainEntitiesDB();
            

            DriverScoring.DBModels.Пользователи person = new DriverScoring.DBModels.Пользователи();
            person.Login = "LOGIN_TEST22";
            person.Password = "PASSWORD_TEST22";

            // Act
            contextDB.Пользователи.Add(person);
            contextDB.SaveChanges();

            person.Login = "LOGIN_TEST";
            person.Password = "PASSWORD_TEST";

            // Act
            contextDB.Пользователи.Add(person);
            contextDB.SaveChanges();
            */

            return RedirectToAction("LogIn");
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";
            ViewBag.logged = currentuser != null;
            ViewBag.type = usertype;
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            ViewBag.logged = currentuser != null;
            ViewBag.type = usertype;
            return View();
        }

        public ActionResult LogIn()
        {
            ViewBag.logged = currentuser != null;
            ViewBag.type = usertype;
            return View();
        }
        [HttpPost]
        public ActionResult LogIn(string SignInLogin, string SignInPassword)
        {
            ViewData.Clear();
            db.Database.Connection.Open();
            List<DBModels.Пользователи> codes = (from e in db.Пользователи where (e.Login == SignInLogin && e.Password == SignInPassword) select e).ToList();
            if (codes.Count != 0)
            {
                long num = codes[0].ПользовательID;
                List<DBModels.Водители> obj = (from e in db.Водители where (e.ПользовательID == num) select e).ToList();
                if (obj.Count() != 0)
                {
                    currentuser = codes[0];
                    db.Database.Connection.Close();
                    return RedirectToAction("DriverPanel");
                }
                else
                {
                    num = codes[0].ПользовательID;
                    List<long> id1 = (from e in db.Аналитики where (e.ПользовательID == num) select e.АналитикID).ToList();
                    currentuser = codes[0];

                    db.Database.Connection.Close();
                    return RedirectToAction("AdministratorPanel");
                }
            }
            else
            {
                ViewData["Login"] = SignInLogin;
                TempData["alertMessage"] = "Такой аккаунт не существует";

                db.Database.Connection.Close();
                ViewBag.logged = currentuser != null;
                return View();
            }
        }

        public ActionResult Register()
        {
            ViewBag.logged = currentuser != null;
            ViewBag.type = usertype;
            return View();
        }
        [HttpPost]
        public ActionResult Register(string RegisterLogin, string RegisterPassword, string RegisterPhone, string RegisterName, string BirthDate, string Series, string ID)
        {
            db.Database.Connection.Open();
            List<long> codes = (from e in db.Пользователи where (e.Login == RegisterLogin && e.Password == RegisterPassword) select e.ПользовательID).ToList();
            if (codes.Count == 0)
            {
                ViewData.Clear();
                DBModels.Пользователи obj = new DBModels.Пользователи();
                long id=0;
                try
                {
                    id = db.Пользователи.Max(e => e.ПользовательID) + 1;
                }
                catch
                {

                }
                obj.Login = RegisterLogin; obj.Password = RegisterPassword;obj.ПользовательID = id;
                db.Пользователи.Add(obj);
                db.SaveChanges();
                DBModels.Водители obj1 = new DBModels.Водители();
                obj1.ПользовательID = id;obj1.ПаспортныеДанные = Series + "|" + ID;obj1.ДеньРождения = BirthDate;obj1.ДатаРегистрации = DateTime.Today.Date.ToShortDateString();
                id = 0;
                try
                {
                    id = db.Водители.Max(e => e.ВодительID) + 1;
                }
                catch
                {

                }
                obj1.ВодительID = id;
                db.Водители.Add(obj1);
                db.SaveChanges(); db.Database.Connection.Close();
                return RedirectToAction("LogIn");
            }
            else
            {
                ViewData["Login"] = RegisterLogin;
                ViewData["Password"] = RegisterPassword;
                ViewData["Phone"] = RegisterPhone;
                ViewData["Name"] = RegisterName;
                ViewData["BirthDate"] = BirthDate;
                ViewData["Series"] = Series;
                ViewData["ID"] = ID;
                TempData["alertMessage"] = "Аккаунт с указанными данными уже существует";
                db.Database.Connection.Close();
                ViewBag.logged = currentuser != null;
                ViewBag.type = usertype;
                return View();
            }

        }

        public ActionResult DriverPanel()
        {
            if (currentuser != null)
            {
                ViewData.Clear();
            ViewData["User"] = currentuser.Login;
                ViewBag.logged = currentuser != null;
                usertype = "driver";
                ViewBag.type = usertype;
                return View();
            }
            else
            {
                return RedirectToAction("LogIn");
            }
        }

        public ActionResult DriverApplication()
        {
            if (currentuser != null)
            {
                ViewData.Clear();
                ViewData["User"] = currentuser.Login;
                db.Database.Connection.Open();
                List<DBModels.Запросы> list = (from e in db.Запросы where (e.ВодительID == currentuser.ВодительID) select e).ToList();
                ViewData["Requests"] = list;
                db.Database.Connection.Close();
                ViewBag.logged = currentuser != null;
                ViewBag.type = usertype;
                return View();
            }
            else
            {
                return RedirectToAction("LogIn");
            }
        }
        [HttpPost]
        public ActionResult DriverApplication(long ReqIdVal)
        {
            if (currentuser != null)
            {
                db.Database.Connection.Open();
                List<DBModels.Запросы> obj = (from e in db.Запросы where (e.ЗапросID == ReqIdVal) select e).ToList();
                db.Запросы.Remove(obj[0]);
                db.SaveChanges();
                TempData["alertMessage"] = "Заявка была успешно удалена";
                db.Database.Connection.Close();
                return RedirectToAction("DriverApplication");
            }
            else
            {
                return RedirectToAction("LogIn");
            }
        }

        public ActionResult NewDriverApplication()
        {
            if (currentuser != null)
            {
                ViewData["Name"] = currentuser.Login;
                ViewBag.logged = currentuser != null;
                ViewBag.type = usertype;
                return View();
            }
            else
            {
                return RedirectToAction("LogIn");
            }
        }

        [HttpPost]
        public ActionResult NewDriverApplication(string CarId)
        {
            DBModels.Запросы obj = new DBModels.Запросы();
            obj.ВодительID = currentuser.ВодительID;
            obj.Водители = db.Водители.Where(x => x.ВодительID == currentuser.ВодительID).First();
            obj.ЗапросРассмотрен = 1;
            obj.ДатаЗапроса = DateTime.Today.Date.ToShortDateString();
            db.Database.Connection.Open();
            long id = 0;
            try
            {
                id = (long)db.Запросы.Max(e => e.ЗапросID) + 1;
            }
            catch
            {

            }
            obj.ЗапросID = id;
            obj.МашинаID = (long)(Convert.ToInt32(CarId));
            obj.Машины = db.Машины.Where(x => x.МашинаID == obj.МашинаID).First();
            obj.ВремяВыдачиМашины = "";
            obj.ВремяПолученияМашины = "";
            db.Запросы.Add(obj);
            db.SaveChanges();
            db.Database.Connection.Close();
            return RedirectToAction("DriverApplication");
        }

        public ActionResult Survey()
        {
            if (currentuser != null)
            {
                db.Database.Connection.Open();
                List<DBModels.Анкета> list = (from e in db.Анкета where (e.ВодительID == currentuser.ВодительID) select e).ToList();
                db.Database.Connection.Close();
                if (list.Count == 0)
                {
                    ViewData["Name"] = currentuser.Login;
                    ViewBag.logged = currentuser != null;
                    ViewBag.type = usertype;
                    return View();
                }
                else
                {
                    return RedirectToAction("DriverPanel");
                }
            }
            else
            {
                return RedirectToAction("LogIn");
            }
        }

        [HttpPost]
        public ActionResult Survey(string Age, string Children, string Family, string Loan, string Salary, string Job, string HaveCar, string Crashes, string JobYears, string DriveYears)
        {
            db.Database.Connection.Open();
            DBModels.Анкета obj = new DBModels.Анкета();
            obj.АварииЗаПятьЛет = (long)Convert.ToInt32(Crashes);
            obj.ВодительID = currentuser.ВодительID;
            obj.Возраст = (long)Convert.ToInt32(Age);
            obj.Дети = (long)Convert.ToInt32(Children);
            obj.Доход = (long)Convert.ToInt32(Salary);
            if (Loan == "Имеется кредит")
                obj.ЕстьКредит = 0;
            else
            if (Family == "В браке")
                obj.СемейноеПоложение = 0;
            else
                obj.СемейноеПоложение = 1;
            if (HaveCar == "Имеется автомобиль")
                obj.НаличиеМашины = 0;
            else
                obj.НаличиеМашины = 1;
            obj.СтажВождения = (long)Convert.ToInt32(DriveYears);
            obj.СтажРаботы = (long)Convert.ToInt32(JobYears);
            if (Job == "Трудоустроен(а)")
                obj.СтажРаботы = 0;
            else
                obj.Трудоустройство = 1;
            long id = 0;
            try
            {
                id = (long)db.Анкета.Max(e => e.АнкетаID) + 1;
            }
            catch
            {

            }
            obj.АнкетаID = id;
            db.Анкета.Add(obj);
            db.SaveChanges();
            db.Database.Connection.Close();
            return RedirectToAction("DriverPanel");
        }

        public ActionResult AdministratorPanel()
        {
            if (currentuser != null)
            {
                ViewData.Clear();
                db.Database.Connection.Open();
                ViewData["User"] = currentuser.Login;
                List<DBModels.Запросы> list = (from e in db.Запросы where e.ЗапросРассмотрен == 1 select e).ToList();
                db.Database.Connection.Close();
                ViewData["RequestList"] = list;
                ViewBag.logged = currentuser != null;
                usertype = "admin";
                ViewBag.type = usertype;
                return View();
            }
            else
            {
                return RedirectToAction("LogIn");
            }
        }

        [HttpPost]
        public ActionResult AdministratorPanel(string ReqId)
        {
            return RedirectToAction("ApplicationInfo", new { Id = ReqId });
        }

        public ActionResult ApplicationInfo(string Id)
        {
            if (currentuser != null)
            {
                ViewData.Clear();
                ViewData["Id"] = Id;
                ViewData["Name"] = currentuser.Login;
                ViewBag.logged = currentuser != null;
                ViewBag.type = usertype;
                return View();
            }
            else
            {
                return RedirectToAction("LogIn");
            }
        }

        public ActionResult ApplicationDecision(string dec,long requestid)
        {
            if (currentuser != null)
            {
                if (dec == "Принять")
            {
                db.Database.Connection.Open();
                List<DBModels.Запросы> obj = (from e in db.Запросы where(e.ЗапросID==requestid) select e).ToList();
                obj[0].ЗапросРассмотрен = 0;
                db.SaveChanges();
                DBModels.РезультатЗапроса obj1 = new DBModels.РезультатЗапроса();
                obj1.АналитикID = currentuser.АналитикID;
                obj1.МашинаВыдана = 0;
                obj1.ПричинаОтказа = "-";
                obj1.ЗапросID = requestid;
                long id = 0;
                try
                {
                    id = (long)db.РезультатЗапроса.Max(e => e.РезультатID) + 1;
                }
                catch
                {

                }
                obj1.РезультатID = id;
                db.РезультатЗапроса.Add(obj1);
                db.SaveChanges();
                db.Database.Connection.Close();
                return RedirectToAction("AdministratorPanel");
            }
            else
            {
                RequestIdent = requestid;
                return RedirectToAction("EnterReason",new {reqid=requestid });
            }
            }
            else
            {
                return RedirectToAction("LogIn");
            }

        }
        
        public ActionResult EnterReason(long reqid)
        {
            if (currentuser != null)
            {
                ViewData.Clear();
            ViewData["Name"] = currentuser.Login;
                ViewBag.logged = currentuser != null;
                ViewBag.type = usertype;
                return View();
            }
            else
            {
                return RedirectToAction("LogIn");
            }
        }

        [HttpPost]
        public ActionResult EnterReason(string ReasonField)
        {
            db.Database.Connection.Open();
            List<DBModels.Запросы> obj = (from e in db.Запросы where (e.ЗапросID == RequestIdent) select e).ToList();
            obj[0].ЗапросРассмотрен = 0;
            db.SaveChanges();
            DBModels.РезультатЗапроса obj1 = new DBModels.РезультатЗапроса();
            obj1.АналитикID = currentuser.АналитикID;
            obj1.МашинаВыдана = 1;
            obj1.ПричинаОтказа = ReasonField;
            obj1.ЗапросID = RequestIdent;
            long id = 0;
            try
            {
                id = (long)db.РезультатЗапроса.Max(e => e.РезультатID) + 1;
            }
            catch
            {

            }
            obj1.РезультатID = id;
            db.РезультатЗапроса.Add(obj1);
            db.SaveChanges();
            db.Database.Connection.Close();
            return RedirectToAction("AdministratorPanel");
        }

        public ActionResult LogOut()
        {
            currentuser = null;
            usertype = "";
            return RedirectToAction("LogIn");
        }
    }
}