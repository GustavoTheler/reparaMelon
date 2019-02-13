using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;

namespace gestionaMelon.Controllers
{
    public class reportesController : Controller
    {
        private gestionaMelonBDDEntities db = new gestionaMelonBDDEntities();
        //
        // GET: /reportes/
        public ActionResult Index(DateTime? fechaDesde, DateTime? fechaHasta)
        {
            if (fechaDesde != null){
                ViewBag.fechaDesde = fechaDesde;
            }
            else {
                fechaDesde = new DateTime(2018, 1, 1);
                ViewBag.fechaDesde = fechaDesde;
            }
            if (fechaHasta != null){
                ViewBag.fechaHasta = fechaHasta;
            }
            else {
                fechaHasta = DateTime.Today;
                ViewBag.fechaHasta = fechaHasta;
            }

            var ingresosTotales = db.vista.Where(item => item.fechaIngreso >= fechaDesde & item.fechaIngreso <= fechaHasta).Count();
            ViewBag.ingresosTotales = ingresosTotales;

            int tiempoPromedioReparacion;
            try
            {
                tiempoPromedioReparacion = db.Database.SqlQuery<int>("SELECT avg(DATEDIFF(day, fechaIngreso, fechaRetirada)) AS DateDiff from vista where fechaIngreso > @fechaDesde and fechaIngreso < @fechaHasta", new SqlParameter("@fechaDesde", fechaDesde), new SqlParameter("@fechaHasta", fechaHasta)).First();
            }
            catch { tiempoPromedioReparacion = 0; }
            
            ViewBag.tiempoPromedioReparacion = tiempoPromedioReparacion;


            var results = db.vista.Where(a => a.fechaIngreso >= fechaDesde & a.fechaIngreso <= fechaHasta).GroupBy(item => item.estado.descripcion).Select(a => new { a.Key, Count = a.Count() });

            ArrayList xValue = new ArrayList();
            ArrayList yValue = new ArrayList();


            results.ToList().ForEach(rs => xValue.Add(rs.Key));
            results.ToList().ForEach(rs => yValue.Add(rs.Count));


            int i = 0;
            string porEstados = "";
            foreach (string item in xValue) {
                porEstados += item + ": ";
                porEstados += "\a" + yValue[i] + "\n" + "/a";
                i++;
            }

            ViewBag.porEstados = porEstados;
            

            return View();
        }

        public ActionResult reportePrueba(DateTime? fechaDesde, DateTime? fechaHasta) {

            ArrayList xValue = new ArrayList();
            ArrayList yValue = new ArrayList();

            /// HACE DISTINTIAS COSAS DEPENDIENDO SI SE PUSO FECHADESDE FECHAHASTA O NULL TODO
            /// VER BIEN porque es una porqueria asi hecho (por ahora dejo asi)
            if (fechaDesde == null & fechaHasta == null) {
                var results = db.vista.GroupBy(item => item.fechaIngreso).Select(g => new { g.Key, Count = g.Count() });
                results.ToList().ForEach(rs => xValue.Add(rs.Key));
                results.ToList().ForEach(rs => yValue.Add(rs.Count));
            }else if (fechaDesde == null){
                var results = db.vista.Where(item => item.fechaIngreso <= fechaHasta).GroupBy(item => item.fechaIngreso).Select(g => new { g.Key, Count = g.Count() });
                results.ToList().ForEach(rs => xValue.Add(rs.Key));
                results.ToList().ForEach(rs => yValue.Add(rs.Count));
            } else if (fechaHasta == null){
                var results = db.vista.Where(item => item.fechaIngreso >= fechaDesde).GroupBy(item => item.fechaIngreso).Select(g => new { g.Key, Count = g.Count() });
                results.ToList().ForEach(rs => xValue.Add(rs.Key));
                results.ToList().ForEach(rs => yValue.Add(rs.Count));
            }
            else {
                var results = db.vista.Where(item => item.fechaIngreso >= fechaDesde & item.fechaIngreso <= fechaHasta).GroupBy(item => item.fechaIngreso).Select(g => new { g.Key, Count = g.Count() });
                results.ToList().ForEach(rs => xValue.Add(rs.Key));
                results.ToList().ForEach(rs => yValue.Add(rs.Count));
            }

            new Chart(width: 600, height: 400)
            .AddTitle("Ingresadas en reparación")
            .AddSeries(chartType: "Column", xValue: xValue, yValues: yValue)
            .SetYAxis(title: "Cantidad")
            .Write("bmp");

            return null;
        }

        public Action reportePorEstados(DateTime? fechaDesde, DateTime? fechaHasta) {
            if (fechaDesde == null) {
                fechaDesde = new DateTime(2018, 1, 1);
            }
            if (fechaHasta == null) {
                fechaHasta = DateTime.Now;
            }

            var results = db.vista.Where(a => a.fechaIngreso >= fechaDesde & a.fechaIngreso <= fechaHasta).GroupBy(item => item.estado.descripcion).Select(a => new { a.Key, Count = a.Count() });

            ArrayList xValue = new ArrayList();
            ArrayList yValue = new ArrayList();

            results.ToList().ForEach(rs => xValue.Add(rs.Key));
            results.ToList().ForEach(rs => yValue.Add(rs.Count));

            new Chart(width: 400, height: 400)
            .AddTitle("Por estados")
            .AddSeries(chartType: "Pie", xValue: xValue, yValues: yValue)
            .SetYAxis(title: "Cantidad")
            .Write("bmp");


            

            return null;
        }
        
	}
}