using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace gestionaMelon
{
    public class problemaController : Controller
    {
        private gestionaMelonBDDEntities db = new gestionaMelonBDDEntities();

        // GET: /problema/
        public ActionResult Index()
        {
            return View(db.problema.ToList());
        }

        // GET: /problema/Details/5
        [Authorize(Roles = "Admin")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            problema problema = db.problema.Find(id);
            if (problema == null)
            {
                return HttpNotFound();
            }
            return View(problema);
        }

        // GET: /problema/Create
        [Authorize(Roles = "Admin")]
        public ActionResult Create()
        {
            return View();
        }

        // POST: /problema/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult Create([Bind(Include="id_problema,descripcion")] problema problema)
        {
            if (ModelState.IsValid)
            {
                db.problema.Add(problema);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(problema);
        }

        // GET: /problema/Edit/5
        [Authorize(Roles = "Admin")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            problema problema = db.problema.Find(id);
            if (problema == null)
            {
                return HttpNotFound();
            }
            return View(problema);
        }

        // POST: /problema/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult Edit([Bind(Include="id_problema,descripcion")] problema problema)
        {
            if (ModelState.IsValid)
            {
                db.Entry(problema).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(problema);
        }

        // GET: /problema/Delete/5
        [Authorize(Roles = "Admin")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            problema problema = db.problema.Find(id);
            if (problema == null)
            {
                return HttpNotFound();
            }
            return View(problema);
        }

        // POST: /problema/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult DeleteConfirmed(int id)
        {
            problema problema = db.problema.Find(id);
            db.problema.Remove(problema);
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
