using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using WantToLearn.Models;

namespace WantToLearn.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
        
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("LogIn");
        }
        
        [HttpGet]
        public ActionResult LogIn()
        {
            return View("LogIn");
        }

        //Hashing methods ---------------------------------------------
        public static byte[] ComputeHMAC_SHA256(byte[] data, byte[] salt)
        {
            using (var hmac = new HMACSHA256(salt))
            {
                return hmac.ComputeHash(data);
            }
        }

        [HttpPost]
        public ActionResult LogIn(UserModel model)
        {
            using (MostaLearningEntities db = new MostaLearningEntities())
            {
                Student_tbl std = db.Student_tbl.First(st => st.Email == model.Email);


                var pass = Convert.ToBase64String(ComputeHMAC_SHA256(Encoding.UTF8.GetBytes(model.Password), std.salt));
                bool IsValidStudent = db.Student_tbl.Any(user => user.Email.ToLower() == model.Email.ToLower() && user.Password == pass);
                bool IsValidLecturer = db.Lecturer_tbl.Any(user => user.Email.ToLower() == model.Email.ToLower() && user.Password == model.Password);
                bool IsValidAdmin = db.Admin_tbl.Any(user => user.Email.ToLower() == model.Email.ToLower() && user.Password == model.Password);

                if (IsValidStudent)
                {
                    FormsAuthentication.SetAuthCookie(model.Email, false);
                    //Go to profile page
                    return RedirectToAction("Index", "Home");
                }

                if (IsValidLecturer)
                {
                    FormsAuthentication.SetAuthCookie(model.Email, false);
                    //Go to profile page
                    return RedirectToAction("Index", "Home");
                }

                if (IsValidAdmin)
                {
                    FormsAuthentication.SetAuthCookie(model.Email, false);
                    //Go to profile page
                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError("", "invalid Username or Password");
                return View("LogIn");
            }
        }
    }
}