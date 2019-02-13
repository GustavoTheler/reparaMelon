using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Configuration;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;

namespace gestionaMelon
{
    public class CursosTController : Controller
    {
        private gestionaMelonBDDEntities db = new gestionaMelonBDDEntities();

        // GET: /CursosT/
        public ActionResult Index()
        {
            return View(db.CursosT.ToList());
        }

        // GET: /CursosT/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CursosT CursosT = db.CursosT.Find(id);
            if (CursosT == null)
            {
                return HttpNotFound();
            }
            return View(CursosT);
        }

        // GET: /CursosT/Create
        [Authorize(Roles = "Admin")]
        public ActionResult Create()
        {
            return View();
        }

        // POST: /CursosT/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult Create([Bind(Include = "id_cursos, cursos_descripcion")] CursosT CursosT)
        {
            if (ModelState.IsValid)
            {
                db.CursosT.Add(CursosT);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(CursosT);
        }

        // GET: /CursosT/Edit/5
        [Authorize(Roles = "Admin")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CursosT CursosT = db.CursosT.Find(id);
            if (CursosT == null)
            {
                return HttpNotFound();
            }
            return View(CursosT);
        }

        // POST: /CursosT/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult Edit([Bind(Include = "id_cursos, cursos_descripcion")] CursosT CursosT)
        {
            if (ModelState.IsValid)
            {
                db.Entry(CursosT).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(CursosT);
        }

        // GET: /CursosT/Delete/5
        [Authorize(Roles = "Admin")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CursosT CursosT = db.CursosT.Find(id);
            if (CursosT == null)
            {
                return HttpNotFound();
            }
            return View(CursosT);
        }

        // POST: /estado/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult DeleteConfirmed(int id)
        {
            CursosT CursosT = db.CursosT.Find(id);
            db.CursosT.Remove(CursosT);
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
