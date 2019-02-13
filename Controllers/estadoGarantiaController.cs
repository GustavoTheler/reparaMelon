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
    public class estadoGarantiaController : Controller
    {
        private gestionaMelonBDDEntities db = new gestionaMelonBDDEntities();

        // GET: /estadoGarantia/
        public ActionResult Index()
        {
            return View(db.estadoGarantia.ToList());
        }

        // GET: /estadoGarantia/Details/5
        [Authorize(Roles = "Admin")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            estadoGarantia estadogarantia = db.estadoGarantia.Find(id);
            if (estadogarantia == null)
            {
                return HttpNotFound();
            }
            return View(estadogarantia);
        }

        // GET: /estadoGarantia/Create
        [Authorize(Roles = "Admin")]
        public ActionResult Create()
        {
            return View();
        }

        // POST: /estadoGarantia/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult Create([Bind(Include="id_estado_garantia,descripcion")] estadoGarantia estadogarantia)
        {
            if (ModelState.IsValid)
            {
                db.estadoGarantia.Add(estadogarantia);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(estadogarantia);
        }

        // GET: /estadoGarantia/Edit/5
        [Authorize(Roles = "Admin")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            estadoGarantia estadogarantia = db.estadoGarantia.Find(id);
            if (estadogarantia == null)
            {
                return HttpNotFound();
            }
            return View(estadogarantia);
        }

        // POST: /estadoGarantia/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult Edit([Bind(Include="id_estado_garantia,descripcion")] estadoGarantia estadogarantia)
        {
            if (ModelState.IsValid)
            {
                db.Entry(estadogarantia).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(estadogarantia);
        }

        // GET: /estadoGarantia/Delete/5
        [Authorize(Roles = "Admin")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            estadoGarantia estadogarantia = db.estadoGarantia.Find(id);
            if (estadogarantia == null)
            {
                return HttpNotFound();
            }
            return View(estadogarantia);
        }

        // POST: /estadoGarantia/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult DeleteConfirmed(int id)
        {
            estadoGarantia estadogarantia = db.estadoGarantia.Find(id);
            db.estadoGarantia.Remove(estadogarantia);
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
