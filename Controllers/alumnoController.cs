using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using PagedList;

namespace gestionaMelon
{
    public class alumnoController : Controller
    {
        private gestionaMelonBDDEntities db = new gestionaMelonBDDEntities();


        // BUSCAR

        public ActionResult Buscar(int? page, string busqueda) {
            var alumnos = from vis in db.alumno select vis;
            // ordeno alumnos para que no de error la paginacion
            alumnos = alumnos.OrderBy(item => item.apellido);

            /// VIEW BAG
            ViewBag.busqueda = busqueda;

            /// FILTRO de busqueda
            if (!String.IsNullOrEmpty(busqueda)){
                alumnos = alumnos.Where(s => s.nombre.Contains(busqueda) || 
                                             s.apellido.Contains(busqueda)|| 
                                             s.netbook.ndeserie.Contains(busqueda)
                                             );
            }
   
            /// PAGINACION
            /// 
            int pageSize = 6;
            int pageNumber = (page ?? 1);

            return PartialView(alumnos.ToPagedList(pageNumber, pageSize));
        }

      
        // GET: /alumno/
        public ActionResult Index(int? page, string fk_id_curso, string busquedaString, string CurrentFilter)
        {
            // recupero todos los alumnos de db
            var alumno = from item in db.alumno select item;
            // los ordeno por el apellido // SI NO SE ORDENA DA ERROR LA PAGINACION 
            alumno = alumno.OrderBy(item => item.apellido);


            //////////VIEW BAGS ///////////
            // cargar los ddlists de la lista
            ViewBag.fk_id_curso = new SelectList(db.CursosT, "id_cursos", "cursos_descripcion");

            /////// FILTRAR ////////////
            /// por curso
            if (!String.IsNullOrEmpty(fk_id_curso)){ 
                int fk_id_curso_int = Convert.ToInt32(fk_id_curso); // conversion afuera de la sentencia LINQ (nose porqe)
                alumno = alumno.Where(s => s.fk_id_curso == fk_id_curso_int); //mostrar solo los alumnos con ese id curso
                ViewBag.fk_id_curso_int = fk_id_curso_int; // manda el id curso para linkiar en la vista (en el boton de paginacion)
            }

            // busqueda
            if (!String.IsNullOrEmpty(busquedaString))
            {
                alumno= alumno.Where(s => s.nombre.Contains(busquedaString) || s.apellido.Contains(busquedaString)
                                                                            || s.dni.Contains(busquedaString)
                                                                            || s.mail.Contains(busquedaString)
                                                                            || s.netbook.ndeserie.Contains(busquedaString));
            }
            else{ // si no se escribió nada en busqueda devuelve los demas filtros
                busquedaString = CurrentFilter;
            }
            ViewBag.CurrentFilter = busquedaString;


            //// PAGINACION ////////////
            int pageSize = 8;
            int pageNumber = (page ?? 1);

            return View(alumno.ToPagedList(pageNumber, pageSize));
        }

        // GET: /alumno/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            alumno alumno = db.alumno.Find(id);
            if (alumno == null)
            {
                return HttpNotFound();
            }
            return View(alumno);
        }

        // GET: /alumno/Create
        [Authorize(Roles = "Admin")]
        public ActionResult Create()
        {
            // solo cargará el ddlist con las netbooks que no esten asignadas a ningun alumno
            var netbooks = db.netbook.SqlQuery("select * from netbook where id_netbook not in (select fk_id_netbook from alumno) or id_netbook = 2").ToList<netbook>();
            // agrego una netbook con ndeserie "SIN ASIGNAR" E ID=2 para siempre este la opcion de sin asignar
            netbook netbook = new netbook(); 
                netbook.ndeserie = "sin asignar";
                netbook.id_netbook = 2;

            netbooks.Add(netbook); // agrego y ordeno descendente
            netbooks.OrderByDescending(a => a.id_netbook);

            ViewBag.fk_id_curso = new SelectList(db.CursosT, "id_cursos", "cursos_descripcion");
            ViewBag.fk_id_netbook = new SelectList(netbooks, "id_netbook", "ndeserie");
            return View();
        }

        // POST: /alumno/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult Create([Bind(Include="id_alumno, nombre, apellido, telefono, mail, dni, fk_id_curso, fk_id_netbook")] alumno alumno)
        {
            if (ModelState.IsValid)
            {
                db.alumno.Add(alumno);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.fk_id_curso = new SelectList(db.CursosT, "id_cursos", "cursos_descripcion", alumno.fk_id_curso);
            ViewBag.fk_id_netbook = new SelectList(db.netbook, "id_netbook", "ndeserie", alumno.fk_id_netbook);
            return View(alumno);
        }

        // GET: /alumno/Edit/5
        [Authorize(Roles = "Admin")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            alumno alumno = db.alumno.Find(id);
            if (alumno == null)
            {
                return HttpNotFound();
            }

            ViewBag.fk_id_curso = new SelectList(db.CursosT, "id_cursos", "cursos_descripcion", alumno.fk_id_curso);
            ViewBag.fk_id_netbook = new SelectList(db.netbook, "id_netbook", "ndeserie", alumno.fk_id_netbook);
            return View(alumno);
        }

        // POST: /alumno/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult Edit([Bind(Include = "id_alumno, nombre, apellido, telefono, mail, dni ,fk_id_curso, fk_id_netbook")] alumno alumno)
        {
            if (ModelState.IsValid)
            {
                db.Entry(alumno).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.fk_id_curso = new SelectList(db.CursosT, "id_cursos", "cursos_descripcion", alumno.fk_id_curso);
            ViewBag.fk_id_netbook = new SelectList(db.netbook, "id_netbook", "ndeserie", alumno.fk_id_netbook);
            return View(alumno);
        }

        // GET: /alumno/Delete/5
        [Authorize(Roles = "Admin")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            alumno alumno = db.alumno.Find(id);
            if (alumno == null)
            {
                return HttpNotFound();
            }
            return View(alumno);
        }

        // POST: /alumno/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult DeleteConfirmed(int id)
        {
            alumno alumno = db.alumno.Find(id);
            db.alumno.Remove(alumno);
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
