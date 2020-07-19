using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Web;
using System.Web.Mvc;
using WantToLearn.Models;

namespace WantToLearn.Controllers
{
    //[Authorize(Roles = "Admin")]
    public class CourseController : Controller
    {
        private MostaLearningEntities db = new MostaLearningEntities();

        // GET: Course
        [Authorize(Roles = "Lecturer, Admin")]
        public ActionResult Index(string searchName, int currentPage = 1)
        {
            int pageSize = 2; //number of items in one page
            var courses = db.Course_tbl.Include(c => c.Lecturer_tbl);

            if (User.IsInRole("Lecturer"))
            {
                var lec = db.Lecturer_tbl.First(l => l.Email == User.Identity.Name);
                courses = db.Course_tbl.Where(c => c.Lecturer == lec.Code_ID);
                //return View(courses.ToList());
            }
            //search
            if (!string.IsNullOrEmpty(searchName))
            {
                courses = courses.Where(c => c.Name.Contains(searchName));
            }

            //Paging
            
            Session["PagesCount"] = (courses.Count() / pageSize) + (courses.Count() % pageSize);
            Session["CurrentP"] = currentPage;
            int skip = (currentPage - 1) * pageSize;

            return View(courses.OrderByDescending(c => c.ID).Skip(skip).Take(pageSize).ToList());
        }

        // GET: Course/Details/5
        [Authorize(Roles = "Student,Lecturer,Admin")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Course_tbl course_tbl = db.Course_tbl.Find(id);
            if (course_tbl == null)
            {
                return HttpNotFound();
            }
            return View(course_tbl);
        }

        
        // GET: Course/Create
        public ActionResult Create()
        {
            ViewBag.Lecturer = new SelectList(db.Lecturer_tbl, "Code_ID", "Name");
            return View();
        }

        // POST: Course/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,Name,Lecturer,PDFs_Links,Level")] Course_tbl course_tbl)
        {
            if (ModelState.IsValid)
            {
                db.Course_tbl.Add(course_tbl);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.Lecturer = new SelectList(db.Lecturer_tbl, "Code_ID", "Name", course_tbl.Lecturer);
            return View(course_tbl);
        }

        // GET: Course/Edit/5
        [Authorize(Roles = "Admin, Lecturer")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Course_tbl course_tbl = db.Course_tbl.Find(id);
            if (course_tbl == null)
            {
                return HttpNotFound();
            }
            ViewBag.Lecturer = new SelectList(db.Lecturer_tbl, "Code_ID", "Name", course_tbl.Lecturer);
            return View(course_tbl);
        }

        // POST: Course/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,Name,Lecturer,PDFs_Links,Level")] Course_tbl course_tbl)
        {
            if (ModelState.IsValid)
            {
                db.Entry(course_tbl).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.Lecturer = new SelectList(db.Lecturer_tbl, "Code_ID", "Name", course_tbl.Lecturer);
            return View(course_tbl);
        }

        // GET: Course/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Course_tbl course_tbl = db.Course_tbl.Find(id);
            if (course_tbl == null)
            {
                return HttpNotFound();
            }
            return View(course_tbl);
        }

        // POST: Course/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Course_tbl course_tbl = db.Course_tbl.Find(id);
            db.Course_tbl.Remove(course_tbl);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Lecturer")]
        public ActionResult StudentsByCourses(int? id)
        {
            Course_tbl course = db.Course_tbl.Find(id);
            var students = db.Student_tbl.Where(c => c.Level == course.Level);

            return View(students.ToList());
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
