using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using WantToLearn.Models;

namespace WantToLearn.Controllers
{
    public class StudentController : Controller
    {
        private MostaLearningEntities db = new MostaLearningEntities();

        //// GET: Student
        //public ActionResult Index()
        //{
        //    return View(db.Student_tbl.ToList());
        //}

        // GET: Student/Details
        [Authorize(Roles = "Student")]
        public ActionResult Details()
        {
            Student_tbl student_tbl = db.Student_tbl.First(st => st.Email == User.Identity.Name);
            if (student_tbl == null)
            {
                return HttpNotFound();
            }
            return View(student_tbl);
        }

        //Hashing methods ---------------------------------------------
        public static byte[] GenerateSalt()
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                var randomNumber = new byte[32];

                rng.GetBytes(randomNumber);

                return randomNumber;

            }
        }
        public static byte[] ComputeHMAC_SHA256(byte[] data, byte[] salt)
        {
            using (var hmac = new HMACSHA256(salt))
            {
                return hmac.ComputeHash(data);
            }
        }

        // GET: Student/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Student/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Student_tbl student_tbl)
        {
            var salt = GenerateSalt();
            if (ModelState.IsValid)
            {
                student_tbl.Password = Convert.ToBase64String(ComputeHMAC_SHA256(Encoding.UTF8.GetBytes(student_tbl.Password), salt));
                student_tbl.salt = salt;
                db.Student_tbl.Add(student_tbl);
                db.SaveChanges();
                return RedirectToAction("Index", "Home");
            }

            return View(student_tbl);
        }

        // GET: Student/Edit
        [Authorize(Roles = "Student")]
        public ActionResult Edit()
        {
            Student_tbl student_tbl = db.Student_tbl.First(st => st.Email == User.Identity.Name);
            if (student_tbl == null)
            {
                return HttpNotFound();
            }
            return View(student_tbl);
        }

        // POST: Student/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Student_tbl student_tbl)
        {
            if (ModelState.IsValid)
            {
                db.Entry(student_tbl).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Details");
            }
            return View(student_tbl);
        }

        [Authorize(Roles = "Student")]
        public ActionResult EditPassword()
        {
            return View();
        }

        [HttpPost]
        public ActionResult EditPassword(PasswordViewModel passwordVM)
        {
            Student_tbl std = db.Student_tbl.First(st => st.Email == User.Identity.Name);
            var pass = Convert.ToBase64String(ComputeHMAC_SHA256(Encoding.UTF8.GetBytes(passwordVM.OldPassword), std.salt));
            bool IsValidStudent = false;
            if(pass == std.Password)
            {
                IsValidStudent = true;
            }
            if (IsValidStudent)
            {
                std.Password = Convert.ToBase64String(ComputeHMAC_SHA256(Encoding.UTF8.GetBytes(passwordVM.NewPassword), std.salt));
                db.Entry(std).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Details");
            }
            return View(passwordVM);
        }

        // GET: Student/Delete/5
        [Authorize(Roles = "Admin")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Student_tbl student_tbl = db.Student_tbl.Find(id);
            if (student_tbl == null)
            {
                return HttpNotFound();
            }
            return View(student_tbl);
        }

        // POST: Student/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Student_tbl student_tbl = db.Student_tbl.Find(id);
            db.Student_tbl.Remove(student_tbl);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Student")]
        public ActionResult CoursesAvailable(string searchName, int currentPage = 1)
        {
            int pageSize = 2; //number of items in one page
            Student_tbl student = db.Student_tbl.First(st => st.Email == User.Identity.Name);
            var coursesAval = db.Course_tbl.Where(c => c.Level == student.Level);
            if (!string.IsNullOrEmpty(searchName))
            {
                coursesAval = coursesAval.Where(c => c.Name.Contains(searchName));
            }

            //Paging

            Session["PagesCount"] = (coursesAval.Count() / pageSize) + (coursesAval.Count() % pageSize);
            Session["CurrentP"] = currentPage;
            int skip = (currentPage-1) * pageSize;

            return View(coursesAval.OrderBy(c => c.ID).Skip(skip).Take(pageSize).ToList());
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
