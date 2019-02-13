using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace gestionaMelon.Controllers
{
    [RequireHttps]
    public class HomeController : Controller
    {
        private gestionaMelonBDDEntities db = new gestionaMelonBDDEntities();
        
        public ActionResult IndexBusqueda(string busquedaString)
        {
            // si no comprobo su dni y se almaceno en una variable session vuelve al index a pedirle su dni
            if (System.Web.HttpContext.Current.Session["sessionStringDni"] as String == null) {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }


        public ActionResult Index(string dni = "0") {
            // recupero la variable almacenada en la session
            // TempData["sessionStringDni"] = System.Web.HttpContext.Current.Session["sessionStringDni"] as String; 
            // si existe, quiere decir que ya ingreso un dni y ya fue verificado. Asique lo mando al indexBusqueda
            if (System.Web.HttpContext.Current.Session["sessionStringDni"] as String != null)
            { 
                return RedirectToAction("IndexBusqueda");
            }
            // controlo si el dni pasado como parametro existe en algun alumno
            bool existeDni = db.alumno.Any(a => a.dni == dni);

            // si existe, guardo una varibale session con el dni para futuro no tener que vovler a pedirlo
            // y mando al IndexBusqueda (busqueda por alumno)
            if (existeDni)
            {
                System.Web.HttpContext.Current.Session["sessionStringDni"] = dni; 
                return RedirectToAction("IndexBusqueda");
            }
            else if (dni != "0") { // si no existe el dni dvolver error, (si es distinto a cero significa que busco en la pagina)
                ViewBag.errorDni = "El DNI no encuentra ninguna coincidencia";
            }
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult Buscador(String busqueda) {
            ViewBag.busqueda = busqueda;
            return View();
        }

        public ActionResult admin() {
            return PartialView();
        }
    }
}