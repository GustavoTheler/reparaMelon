using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using PagedList;
using System.Data.SqlClient;


namespace gestionaMelon
{
    public class vistaController : Controller
    {
        private gestionaMelonBDDEntities db = new gestionaMelonBDDEntities();

        public ActionResult verGarantia(int id) {
            //busca el idGarantia a partir del idVista y redirige a los detalles de garantia
            garantia mygarantia = db.garantia.SingleOrDefault(garantia => garantia.idVista == id);

            return RedirectToAction("Details", "garantia", new { id = mygarantia.idGarantia });
        }

        [Authorize(Roles = "Admin")]
        public ActionResult Retirada(int id)
        {
            vista vista = db.vista.Find(id);
                vista.flagRetirada = true;
                vista.fechaRetirada = DateTime.Today;
            db.Entry(vista).State = EntityState.Modified;
            
            db.SaveChanges();          
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin")]
        // GET: /vista/
        public ActionResult Index(string Sorting_Order, string busquedaString,
            string DDlRetirada, string fk_estado, int? page, string CurrentFilter,
            DateTime? fechaDesde, DateTime? fechaHasta)
        {
            // verifico las fechas desde y hasta
            if (fechaDesde != null){
                ViewBag.fechaDesde = fechaDesde;
            }
            else{
                fechaDesde = new DateTime(2018, 1, 1);
                ViewBag.fechaDesde = fechaDesde;
            }
            if (fechaHasta != null){
                ViewBag.fechaHasta = fechaHasta;}
            else{
                fechaHasta = DateTime.Today;
                ViewBag.fechaHasta = fechaHasta;
            }

            // crea lista de 3 estados (retirado, no retirada, todos) // para el filtro de arriba
            var YesOrNoTriState = new List<SelectListItem> {
                new SelectListItem { Text = "¿Retirada?", Value = "" },
                 new SelectListItem { Text = "Retirada", Value = true.ToString() },
                 new SelectListItem { Text = "No Retirada", Value = false.ToString() }
                    };
            // VIEWBAGS
            // ddlist de flagRetirada, ddlist de estado, ordenamineto nombre y fecha
            ViewBag.CurrentSort = Sorting_Order;
            ViewBag.DDlRetirada = YesOrNoTriState;
            ViewBag.fk_estado = new SelectList(db.estado, "id_estado", "descripcion");
            ViewBag.SortingName = String.IsNullOrEmpty(Sorting_Order) ? "por_alumno" : "por_alumno_desc";
            ViewBag.SortingDate = Sorting_Order == "por_fecha_ingreso" ? "por_fecha_ingreso_desc" : "por_fecha_ingreso";

            // carga todas las vistas de la bd
            var vistas = from vis in db.vista select vis;
            // dejamos solo las vistas que no esten marcadas como eliminadas, esten entre las fechas Desde y Hasta
            vistas = vistas.Where(a => a.eliminado != true & a.fechaIngreso > fechaDesde & a.fechaIngreso <= fechaHasta);
            // busquedas y filtros ////////////
            // busqueda en texto del filtro. Devuelve el listado donde encuentre el string nombre, apellido, dni, mail o numero de serie
            if (!String.IsNullOrEmpty(busquedaString))
            {
                vistas = vistas.Where(s => s.alumno1.nombre.Contains(busquedaString) || s.alumno1.apellido.Contains(busquedaString)
                                                                                     || s.alumno1.dni.Contains(busquedaString)
                                                                                     || s.alumno1.mail.Contains(busquedaString)
                                                                                     || s.alumno1.netbook.ndeserie.Contains(busquedaString))
;
            }
            else {
                busquedaString = CurrentFilter;    
                 }
            ViewBag.CurrentFilter = busquedaString;

            if (!String.IsNullOrEmpty(fk_estado)) { // filtra las vistas con el estado filtrado en la busqueda
                int fk_estado_int = Convert.ToInt32(fk_estado); // conversion afuera de la sentencia LINQ (nose porqe)
                vistas = vistas.Where(s => s.fk_estado == fk_estado_int  );
                ViewBag.fk_estado_int = fk_estado_int;
            }
            if (!String.IsNullOrEmpty(DDlRetirada)) { // filtra las vistas marcadas como retiradas o no 
                bool DDlRetiradaBool = Convert.ToBoolean(DDlRetirada); // se hace la conversion afuera de la sentencia LINQ
                vistas = vistas.Where(s => s.flagRetirada == DDlRetiradaBool);
                ViewBag.DDlRetiradaBool = DDlRetiradaBool;
            }

            ///// ordenar por click // en los titulos de la tabla
            switch (Sorting_Order)
            {
                case "por_alumno":
                    vistas = vistas.OrderBy(vis => vis.alumno1.apellido);
                    break;
                case "por_alumno_desc":
                    vistas = vistas.OrderByDescending(vis => vis.alumno1.apellido);
                    break;
                case "por_fecha_ingreso":
                    vistas = vistas.OrderBy(vis => vis.fechaIngreso);
                    break;
                case "por_fecha_ingreso_desc":
                    vistas = vistas.OrderByDescending(vis => vis.fechaIngreso);
                    break;
                default:
                    vistas = vistas.OrderByDescending(vis => vis.fechaIngreso);
                    break;
            }

            /// PAGINACION
            int pageSize = 8;
            int pageNumber = (page ?? 1);

            return View(vistas.ToPagedList(pageNumber, pageSize));
           }

        public ActionResult listaResumida(int? id) {
            // si no comprobo su dni y se almaceno en una variable session vuelve al index a pedirle su dni
            if (System.Web.HttpContext.Current.Session["sessionStringDni"] as String == null){
                return RedirectToAction("Index", "Home");
            }

            var vistas = from vis in db.vista select vis;
            if (id != null)
            {
                vistas = vistas.Where(s => s.alumno1.id_alumno == id).OrderByDescending(s => s.fechaIngreso);
            }
            // paso viewbag a listaREsumida con 'Nombre Apellido ndeserie' Ej: 'Beto Casella aa20002312'
            ViewBag.alumno = vistas.First().alumno1.nombre + " " + vistas.First().alumno1.apellido + " " + vistas.First().alumno1.netbook.ndeserie;
            // viewvag pasa a listaResumida el ultimo idVista para dirigir el "Reclamar" y enlazarlo a la ultima vista
            var ultimaVista = vistas.ToList().First();
            ViewBag.ultimoIdVista = ultimaVista.idVista;

            return View(vistas.ToList());
        }
        
        public ActionResult listaResumidaAlumnos(String busqueda, int? page)
        {
            // si no comprobo su dni y se almaceno en una variable session vuelve al index a pedirle su dni
            if (System.Web.HttpContext.Current.Session["sessionStringDni"] as String == null){
                return RedirectToAction("Index", "Home");
            }
            /// VIEW BAG ///
            ViewBag.busqueda = busqueda;
           
            var vistas = from vis in db.vista select vis;
            var alumnos = from alu in db.alumno select alu;
            //var alumnos = db.alumno.SqlQuery("select * from alumno where alumno.id_alumno in (select fk_id_alumno from vista)").ToList();
            
            ViewBag.busqueda = busqueda;

            /// filtrar todos losa lumnos que se encuentren e una vista
            if (!String.IsNullOrEmpty(busqueda)){
                var idAlumnosEnVistas = vistas.Select(s => s.fk_id_alumno);

                alumnos = alumnos.Where(a => idAlumnosEnVistas.Contains(a.id_alumno)
                                            & (
                                             (a.nombre.Contains(busqueda) ||
                                             a.apellido.Contains(busqueda) ||
                                             a.dni.Contains(busqueda)) ||
                                             a.netbook.ndeserie.Contains(busqueda))
                                             ); // y aparezca en alguna vista
            }
            else { /// SI LA BUSQUEDA ES NULL NO FILTRA POR BUSQUEDA  hice esto para que funcione la paginacion
                var idAlumnosEnVistas = vistas.Select(s => s.fk_id_alumno);
                alumnos = alumnos.Where(a => (idAlumnosEnVistas.Contains(a.id_alumno))); // todos los alumnos que aparezcan en una vista
            }
            
            // lo ordeno para que la paginacion no de error
                alumnos = alumnos.OrderBy(item => item.apellido);

            ////// PAGINACION
                int pageSize = 8;
                int pageNumber = (page ?? 1);

                return View(alumnos.ToPagedList(pageNumber, pageSize));
        }

        // GET: /vista/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            vista vista = db.vista.Find(id);
            if (vista == null)
            {
                return HttpNotFound();
            }
            return PartialView(vista);
        }

        // GET: /vista/Create
        [Authorize(Roles = "Admin")]
        public ActionResult Create(int? id)
        {
            ViewBag.fk_estado = new SelectList(db.estado, "id_estado", "descripcion");
            ViewBag.fk_id_alumno = new SelectList(db.alumno, "id_alumno", "dni");
            return View();
        }

        // POST: /vista/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult Create([Bind(Include="idVista,ndeserie,alumno,fk_estado,comentarios,fechaIngreso,flagRetirada,fechaRetirada,fk_id_alumno")] vista vista, string ndeserie_nuevo, string marca_nuevo, string modelo_nuevo)
        {
            if (ModelState.IsValid)
            {
                if (vista.fechaIngreso == null)
                {
                    vista.fechaIngreso = DateTime.Now; // carga la fecha y hora actual
                }

                vista.fk_id_alumno = Convert.ToInt16(vista.fk_id_alumno);
                db.vista.Add(vista);
                db.SaveChanges();
                    
                if (ndeserie_nuevo != "") { // si se crea una netbook junto a la vista
                    netbook netbook = new netbook();
                    netbook.ndeserie = ndeserie_nuevo;
                    netbook.marca = marca_nuevo;
                    netbook.modelo = modelo_nuevo;
                    db.netbook.Add(netbook);
                    db.SaveChanges();

                    alumno alumno = db.alumno.Find(vista.fk_id_alumno);
                    alumno.fk_id_netbook = netbook.id_netbook;
                    db.alumno.Attach(alumno);
                    db.Entry(alumno).Property(x => x.fk_id_netbook).IsModified = true;
                    db.SaveChanges();
                }
                
                // Si el estado es rota
                if (vista.fk_estado == 2) {
                    garantia nuevaGarantia = new garantia();
                    nuevaGarantia.idVista = vista.idVista;
                    nuevaGarantia.fechaIngreso = DateTime.Now;
                    db.garantia.Add(nuevaGarantia);
                    db.SaveChanges();
                    return RedirectToAction("Edit", "garantia", new { id= nuevaGarantia.idGarantia} );
                }

                return RedirectToAction("Index");
            }
            ViewBag.fk_id_alumno = new SelectList(db.alumno, "id_alumno", "dni", vista.fk_id_alumno);
            ViewBag.fk_estado = new SelectList(db.estado, "id_estado", "descripcion", vista.fk_estado);
            return View(vista);
        }

        // GET: /vista/Edit/5
        [Authorize(Roles = "Admin")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            vista vista = db.vista.Find(id);
            if (vista == null)
            {
                return HttpNotFound();
            }
            ViewBag.fk_estado = new SelectList(db.estado, "id_estado", "descripcion", vista.fk_estado);
            ViewBag.fk_id_alumno = new SelectList(db.alumno, "id_alumno", "dni");
            return View(vista);
        }

        // POST: /vista/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult Edit([Bind(Include = "idVista,ndeserie,alumno,fk_estado,comentarios,fechaIngreso,flagRetirada,fechaRetirada, fk_id_alumno")] vista vista, string ndeserie_nuevo, string marca_nuevo, string modelo_nuevo)
        {
            if (ModelState.IsValid)
            {
                db.Entry(vista).State = EntityState.Modified;
                db.SaveChanges();

                if (ndeserie_nuevo != "")
                { // si se crea una netbook junto a la vista
                    netbook netbook = new netbook();
                    netbook.ndeserie = ndeserie_nuevo;
                    netbook.marca = marca_nuevo;
                    netbook.modelo = modelo_nuevo;
                    db.netbook.Add(netbook);
                    db.SaveChanges();

                    alumno alumno = db.alumno.Find(vista.fk_id_alumno);
                    alumno.fk_id_netbook = netbook.id_netbook;
                    db.alumno.Attach(alumno);
                    db.Entry(alumno).Property(x => x.fk_id_netbook).IsModified = true;
                    db.SaveChanges();
                }


               // e almacena la cantidad de ingresos a garantia que tiene esta vista
                int e = db.garantia.SqlQuery("select * from garantia where idVista = @id", new SqlParameter("@id", vista.idVista)).Count();
               
                //int e2 = Convert.ToInt32(e);
                // si no hay un ingreso ya en garantia de esta vista y esta en estado rota
                if (e == 0 & vista.fk_estado == 2)
                {
                    garantia nuevaGarantia = new garantia();
                    nuevaGarantia.idVista = vista.idVista;
                    nuevaGarantia.fechaIngreso = DateTime.Now;
                    db.garantia.Add(nuevaGarantia);
                    db.SaveChanges();
                    return RedirectToAction("Edit", "garantia", new { id = nuevaGarantia.idGarantia });
                }

                return RedirectToAction("Index");
            }

            ViewBag.fk_estado = new SelectList(db.estado, "id_estado", "descripcion", vista.fk_estado);
            ViewBag.fk_id_alumno = new SelectList(db.alumno, "id_alumno", "dni", vista.fk_id_alumno);

            return View(vista);
        }

        // GET: /vista/Delete/5
        [Authorize(Roles = "Admin")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            vista vista = db.vista.Find(id);
            if (vista == null)
            {
                return HttpNotFound();
            }
            return PartialView(vista);
        }

        // POST: /vista/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult DeleteConfirmed(int id)
        {
            vista vista = db.vista.Find(id);
            //db.vista.Remove(vista);
            vista.eliminado = true;

            db.vista.Attach(vista);
            db.Entry(vista).State = EntityState.Modified;
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
