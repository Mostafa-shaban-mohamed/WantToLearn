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
    public class LecturerController : Controller
    {
        private MostaLearningEntities db = new MostaLearningEntities();

        // GET: Lecturer
        [Authorize(Roles = "Admin")]
        public ActionResult Index()
        {
            return View(db.Lecturer_tbl.ToList());
        }

        // GET: Lecturer/Details/5
        [Authorize(Roles = "Lecturer")]
        public ActionResult Details(int? id)
        {
            
            Lecturer_tbl lecturer_tbl = db.Lecturer_tbl.First(l => l.Email == User.Identity.Name);
            if (lecturer_tbl == null)
            {
                return HttpNotFound();
            }
            return View(lecturer_tbl);
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

        // GET: Lecturer/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Lecturer/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Code_ID,Name,Email,Phone,Password")] Lecturer_tbl lecturer_tbl)
        {
            var salt = GenerateSalt();
            if (ModelState.IsValid)
            {
                lecturer_tbl.Password = Convert.ToBase64String(ComputeHMAC_SHA256(Encoding.UTF8.GetBytes(lecturer_tbl.Password), salt));
                lecturer_tbl.salt = salt;
                db.Lecturer_tbl.Add(lecturer_tbl);
                db.SaveChanges();
                return RedirectToAction("Index", "Home");
            }

            return View(lecturer_tbl);
        }

        // GET: Lecturer/Edit/5
        [Authorize(Roles = "Lecturer")]
        public ActionResult Edit(int? id)
        {
            Lecturer_tbl lecturer_tbl = db.Lecturer_tbl.First(l => l.Email == User.Identity.Name);
            if (lecturer_tbl == null)
            {
                return HttpNotFound();
            }
            return View(lecturer_tbl);
        }

        // POST: Lecturer/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Code_ID,Name,Email,Phone,Password")] Lecturer_tbl lecturer_tbl)
        {
            if (ModelState.IsValid)
            {
                db.Entry(lecturer_tbl).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Details");
            }
            return View(lecturer_tbl);
        }

        [Authorize(Roles = "Lecturer")]
        public ActionResult EditPassword()
        {
            return View();
        }

        [HttpPost]
        public ActionResult EditPassword(PasswordViewModel passwordVM)
        {
            Lecturer_tbl lec = db.Lecturer_tbl.First(st => st.Email == User.Identity.Name);
            var pass = Convert.ToBase64String(ComputeHMAC_SHA256(Encoding.UTF8.GetBytes(passwordVM.OldPassword), lec.salt));
            bool IsValidStudent = false;
            if (pass == lec.Password)
            {
                IsValidStudent = true;
            }
            if (IsValidStudent)
            {
                lec.Password = Convert.ToBase64String(ComputeHMAC_SHA256(Encoding.UTF8.GetBytes(passwordVM.NewPassword), lec.salt));
                db.Entry(lec).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Details");
            }
            return View(passwordVM);
        }


        // GET: Lecturer/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Lecturer_tbl lecturer_tbl = db.Lecturer_tbl.Find(id);
            if (lecturer_tbl == null)
            {
                return HttpNotFound();
            }
            return View(lecturer_tbl);
        }

        // POST: Lecturer/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Lecturer_tbl lecturer_tbl = db.Lecturer_tbl.Find(id);
            db.Lecturer_tbl.Remove(lecturer_tbl);
            db.SaveChanges();
            return RedirectToAction("Index");
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
