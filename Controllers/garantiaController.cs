using gestionaMelon.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using PagedList;
using System.Data.SqlClient;

namespace gestionaMelon
{
    public class garantiaController : Controller
    {
        private gestionaMelonBDDEntities db = new gestionaMelonBDDEntities();


        [Authorize(Roles = "Admin")]
        public ActionResult Vuelta(int id)
        {
            /// SE MARCA COMO QUE VOLVIO
            /// falgVuelta = si y fechaVuelta de hoy
            garantia garantia = db.garantia.Find(id);
                garantia.flagVuelta = true;
                garantia.fechaVuelta = DateTime.Today;
            db.Entry(garantia).State = EntityState.Modified;

            db.SaveChanges();
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin")]
        public ActionResult Retirada(int id)
        {
            /// SE MARCA COMO RETIRADA flagRetiradatrue y fechaRetirada de hoy
            garantia garantia = db.garantia.Find(id);
                garantia.flagRetirada = true;
                garantia.fechaRetirada = DateTime.Today;
            db.Entry(garantia).State = EntityState.Modified;

            db.SaveChanges();
            return RedirectToAction("Index");
        }
        
        // GET: /garantia/
        [Authorize(Roles = "Admin")]
        public ActionResult Index(string busquedaString, string DDlRetirada, string DDlVuelta, string DDlEstadoGarantia, string DDlProblema, string Sorting_Order, int? page)
        {
            /// si no es Admin volver a home index
            roleController ctrlRole = new roleController();
            if (!ctrlRole.isAdminUser()) {
                    return RedirectToAction("Index", "Home");
                }

            // crea lista de 3 estados (retirado, no retirada, todos) //CODIGO DUPLICADO
            // VER BIEN QUE ONDA, QUE SE PUEDE HACER
            var YesOrNoState = new List<SelectListItem> {
                 new SelectListItem { Text = "Si", Value = true.ToString() },
                 new SelectListItem { Text = "No", Value = false.ToString() }
                    };
            /// VIEWBAGS
            ViewBag.DDlRetirada = YesOrNoState;
            ViewBag.DDlVuelta = YesOrNoState;
            ViewBag.DDlEstadoGarantia = new SelectList(db.estadoGarantia, "id_estado_garantia", "descripcion");
            ViewBag.DDlProblema = new SelectList(db.problema, "id_problema", "descripcion");
            ViewBag.busquedaString = busquedaString;

            // lista de garantias que no hayan sido marcadas como eliminadas
            var garantia = from gar in db.garantia select gar;
            garantia = garantia.Where(i => i.eliminado != true);

            /// selecciona del listado dependiendo la busqueda y los filtros
            if (!String.IsNullOrEmpty(busquedaString))
            {
                garantia = garantia.Where(s => s.vista.alumno1.nombre.Contains(busquedaString) ||
                                               s.vista.alumno1.apellido.Contains(busquedaString) ||
                                               s.vista.alumno1.netbook.ndeserie.Contains(busquedaString) ||
                                               s.vista.alumno1.dni.Contains(busquedaString));
            }
            if (!String.IsNullOrEmpty(DDlRetirada))
            {
                bool retiradaBool = Convert.ToBoolean(DDlRetirada); // se hace la conversion afuera de la sentencia LINQ
                garantia = garantia.Where(s => s.flagRetirada == retiradaBool);
                ViewBag.retiradaBool = retiradaBool; // se pasa a la vista para hacer el filtro en los displayname de la taba
            }
            if (!String.IsNullOrEmpty(DDlVuelta))
            {
                bool vueltaBool = Convert.ToBoolean(DDlVuelta); // se hace la conversion afuera de la sentencia LINQ
                garantia = garantia.Where(s => s.flagVuelta == vueltaBool);
                ViewBag.vueltaBook = vueltaBool; // se pasa a la vista para hacer el filtro en los displayname de la taba
            }
            if (!String.IsNullOrEmpty(DDlEstadoGarantia))
            {
                int estadoGarantiaInt = Convert.ToInt32(DDlEstadoGarantia); // conversion afuera de la sentencia LINQ (nose porqe)
                garantia = garantia.Where(s => s.estadoGarantia == estadoGarantiaInt);
                ViewBag.estadoGarantiaInt = estadoGarantiaInt; // se pasa a la vista para hacer el filtro en los displayname de la taba
            }
            if (!String.IsNullOrEmpty(DDlProblema))
            {
                int problemaInt = Convert.ToInt32(DDlProblema); // conversion afuera de la sentencia LINQ (nose porqe)
                garantia = garantia.Where(s => s.problema == problemaInt);
                ViewBag.problemaInt = problemaInt; // se pasa a la vista para hacer el filtro en los displayname de la taba
            }

            /// ORDENAMIENTO
            ViewBag.SortingName = String.IsNullOrEmpty(Sorting_Order) ? "por_alumno" : "por_alumno_desc";
            ViewBag.SortingDate = Sorting_Order == "por_fecha_ingreso" ? "por_fecha_ingreso_desc" : "por_fecha_ingreso";
            // ordenamiento
            switch (Sorting_Order)
            {
                case "por_alumno":
                    garantia = garantia.OrderBy(gar => gar.vista.alumno1.apellido);
                    break;
                case "por_alumno_desc":
                    garantia = garantia.OrderByDescending(gar => gar.vista.alumno1.apellido);
                    break;
                case "por_fecha_ingreso":
                    garantia = garantia.OrderBy(gar => gar.fechaIngreso);
                    break;
                case "por_fecha_ingreso_desc":
                    garantia = garantia.OrderByDescending(gar => gar.fechaIngreso);
                    break;
                default:
                    garantia = garantia.OrderByDescending(gar => gar.fechaIngreso);
                    break;
            }

            /// PAGINACION
            int pageSize = 8;
            int pageNumber = (page ?? 1);

            return View(garantia.ToPagedList(pageNumber, pageSize));
        }

        public ActionResult garantiaResumida(String busqueda) {
            var garantia = from gar in db.garantia select gar;
            if (!String.IsNullOrEmpty(busqueda))
            {
                garantia = garantia.Where(s => s.vista.alumno.Contains(busqueda));
            }

            return View(garantia.ToList());
        }

        // GET: /garantia/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            garantia garantia = db.garantia.Find(id);
            if (garantia == null)
            {
                return HttpNotFound();
            }
            return PartialView(garantia);
        }

        // GET: /garantia/Create
        /// la idea es que no se use el create
        [Authorize(Roles = "Admin")]
        public ActionResult Create()
        {
            ViewBag.estadoGarantia = new SelectList(db.estadoGarantia, "id_estado_garantia", "descripcion");
            ViewBag.idVista = new SelectList(db.vista, "idVista", "ndeserie");
            ViewBag.problema = new SelectList(db.problema, "id_problema", "descripcion");
            return View();
        }
        
        // POST: /garantia/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult Create([Bind(Include="idGarantia,idVista,estadoGarantia,problema,comentarios,fechaIngreso,ticket,flagRetirada,fechaRetirada,flagVuelta,fechaVuelta")] garantia garantia)
        {
            if (ModelState.IsValid)
            {
                db.garantia.Add(garantia);
                db.SaveChanges();
                return RedirectToAction("Index");
            }


            ViewBag.estadoGarantia = new SelectList(db.estadoGarantia, "id_estado_garantia", "descripcion", garantia.estadoGarantia);
            ViewBag.idVista = new SelectList(db.vista, "idVista", "ndeserie", garantia.idVista);
            ViewBag.problema = new SelectList(db.problema, "id_problema", "descripcion", garantia.problema);
            return View(garantia);
        }

        // GET: /garantia/Edit/5
        [Authorize(Roles = "Admin")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            garantia garantia = db.garantia.Find(id);
            if (garantia == null)
            {
                return HttpNotFound();
            }

             //garantia.fechaIngreso = DateTime.ParseExact(garantia.fechaIngreso.ToString(), "dd/MM/yyyy", CultureInfo.InvariantCulture);
            
            ViewBag.estadoGarantia = new SelectList(db.estadoGarantia, "id_estado_garantia", "descripcion", garantia.estadoGarantia);
            ViewBag.idVista = new SelectList(db.vista, "idVista", "ndeserie", garantia.idVista);
            ViewBag.problema = new SelectList(db.problema, "id_problema", "descripcion", garantia.problema);
            return View(garantia);
        }

        // POST: /garantia/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult Edit([Bind(Include="idGarantia,idVista,estadoGarantia,problema,comentarios,fechaIngreso,ticket,flagRetirada,fechaRetirada,flagVuelta,fechaVuelta")] garantia garantia)
        {
            if (ModelState.IsValid)
            {
                db.Entry(garantia).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.estadoGarantia = new SelectList(db.estadoGarantia, "id_estado_garantia", "descripcion", garantia.estadoGarantia);
            ViewBag.idVista = new SelectList(db.vista, "idVista", "ndeserie", garantia.idVista);
            ViewBag.problema = new SelectList(db.problema, "id_problema", "descripcion", garantia.problema);
            return View(garantia);
        }

        // GET: /garantia/Delete/5
        [Authorize(Roles = "Admin")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            garantia garantia = db.garantia.Find(id);
            if (garantia == null)
            {
                return HttpNotFound();
            }
            return PartialView(garantia);
        }

        // POST: /garantia/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult DeleteConfirmed(int id)
        {
            garantia garantia = db.garantia.Find(id);
            //db.garantia.Remove(garantia);
            garantia.eliminado = true;
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
