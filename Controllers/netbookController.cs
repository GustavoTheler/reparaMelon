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
    public class netbookController : Controller
    {
        private gestionaMelonBDDEntities db = new gestionaMelonBDDEntities();

        public ActionResult listo() {
            return PartialView();
        }

        // GET: /netbook/
        public ActionResult Index()
        {
            return View(db.netbook.ToList());
        }

        // GET: /netbook/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            netbook netbook = db.netbook.Find(id);
            if (netbook == null)
            {
                return HttpNotFound();
            }
            return View(netbook);
        }
        // get
      
        public ActionResult PartialCreate()
        {
            return PartialView();
        }

        // post
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult PartialCreate([Bind(Include = "id_netbook,ndeserie,marca,modelo")] netbook netbook)
        {
            if (ModelState.IsValid)
            {
                db.netbook.Add(netbook);
                db.SaveChanges();
                return RedirectToAction("listo");
            }

            return View(netbook);
        }


        // GET: /netbook/Create
        [Authorize(Roles = "Admin")]
        public ActionResult Create()
        {
            return View();
        }

        // POST: /netbook/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult Create([Bind(Include = "id_netbook,ndeserie,marca,modelo")] netbook netbook)
        {
            if (ModelState.IsValid)
            {
                db.netbook.Add(netbook);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(netbook);
        }

        // GET: /netbook/Edit/5
        [Authorize(Roles = "Admin")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            netbook netbook = db.netbook.Find(id);
            if (netbook == null)
            {
                return HttpNotFound();
            }
            return View(netbook);
        }

        // POST: /netbook/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult Edit([Bind(Include = "id_netbook,ndeserie,marca,modelo")] netbook netbook)
        {
            if (ModelState.IsValid)
            {
                db.Entry(netbook).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(netbook);
        }

        // GET: /netbook/Delete/5
        [Authorize(Roles = "Admin")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            netbook netbook = db.netbook.Find(id);
            if (netbook == null)
            {
                return HttpNotFound();
            }
            return View(netbook);
        }

        // POST: /netbook/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult DeleteConfirmed(int id)
        {
            netbook netbook = db.netbook.Find(id);
            db.netbook.Remove(netbook);
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
