using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

using System.Web.Security;
using Microsoft.AspNet.Identity;
using System.Net.Configuration;
using System.Configuration;
using System.Net.Mail;
using System.Data.SqlClient;

namespace gestionaMelon
{
    public class vista_usuario_reclamoController : Controller
    {
        private gestionaMelonBDDEntities db = new gestionaMelonBDDEntities();

        
        public void enviarMail(string mailTo, string asunto, string body){
            /// funcion para enviar mail
            /// CONFIGURACIONES
            var smtpSection = (SmtpSection)ConfigurationManager.GetSection("system.net/mailSettings/smtp");
            string strHost = smtpSection.Network.Host;
            int port = smtpSection.Network.Port;
            string strUserName = smtpSection.Network.UserName;
            string strFromPass = smtpSection.Network.Password;
            // MAS CONFGURACIONES Y OPCIONES
            SmtpClient smtp = new SmtpClient(strHost, port);
            NetworkCredential cert = new NetworkCredential(strUserName, strFromPass);
            smtp.Credentials = cert;
            smtp.EnableSsl = true;

            /// ARMADO DE MAIL
            MailMessage msg = new MailMessage(smtpSection.From, mailTo + ", tavotheler@gmail.com"); // mando a mail del admin
            msg.Subject = asunto;
            msg.IsBodyHtml = true;
            msg.Body = body;
            
            smtp.Send(msg);
        }

        public ActionResult error(string error) {
            ViewBag.error = error;
            return View();
        }

        //
        // GET: /vista_usuario_reclamo/
        [Authorize]
        public ActionResult Index(int? id) // se le pasa idVista para que devuelva los reclamos con solo esas idVistas
        {
            // si no comprobo su dni y se almaceno en una variable session vuelve al index a pedirle su dni
            if (System.Web.HttpContext.Current.Session["sessionStringDni"] as String == null)
            {
                return RedirectToAction("Index", "Home");
            }
            // mostrar solo las vistas_usuario_reclamo del usuario actual
            string idUser = User.Identity.GetUserId(); // get id usuario actual
            
            var vista_usuario_reclamo = from vur in db.vista_usuario_reclamo select vur; // recupera todos ls vistas_usuarios_reclamos
            //if (idUser != "39dca077-681b-4f10-ab42-0d62db47bc77") // si el usuario no es admin mostrar los reclamos del alumno y del admin --- sino el user actual es admin asique muestra todos los reclamos de esa vista
            //    vista_usuario_reclamo = vista_usuario_reclamo.Where(v => v.id_usuario.Contains(idUser) || v.id_usuario.Contains("39dca077-681b-4f10-ab42-0d62db47bc77"));  // deja solo las que tengan el usuario actual
            if (id != null) //id == idvista
            {
                vista_usuario_reclamo = vista_usuario_reclamo.Where(v => v.id_vista == id); // id == idVista
            }
            else {
                return RedirectToAction("reclamoPorUsuario", "vista_usuario_reclamo");
            }

            if (vista_usuario_reclamo.Count() == 0) { // en este caso no existe ningun reclamo para el ingreso a reparacion
                ViewBag.error = "No existe ningun reclamo";
                string errorStr = "No existe ningun reclamo para este ingreso en reparación";
                return RedirectToAction("error","vista_usuario_reclamo", new { error = errorStr });
                
            }

            if (vista_usuario_reclamo.Count() != 0) { // verificaos que sea null, porque si no tiene reclamos hechos tira excepcion
                ViewData["ndeserie"] = vista_usuario_reclamo.First().vista.alumno1.netbook.ndeserie;
                ViewData["nombre"] = vista_usuario_reclamo.First().vista.alumno1.nombre;
                ViewData["apellido"] = vista_usuario_reclamo.First().vista.alumno1.apellido;
                DateTime date = vista_usuario_reclamo.First().vista.fechaIngreso.Value; // se instancia objeto para cambiar formato de fecha
                ViewData["fechaIngreso"] = date.ToString("d");
                ViewData["estadoDescripcion"] = vista_usuario_reclamo.First().vista.estado.descripcion;
                ViewData["comentarios"] = vista_usuario_reclamo.First().vista.comentarios;
                ViewData["idVista"] = id;
            }
            

            return View(vista_usuario_reclamo.ToList()); 
        }

        public ActionResult reclamoPorUsuario() {
            // si no comprobo su dni y se almaceno en una variable session vuelve al index a pedirle su dni
            if (System.Web.HttpContext.Current.Session["sessionStringDni"] as String == null){
                return RedirectToAction("Index", "Home");
            }
            
            string idUser = User.Identity.GetUserId();

            var reclamosAgrupados = db.vista_usuario_reclamo.Where(v => v.id_usuario.Contains(idUser))
            .GroupBy(g => g.id_vista)
            .Select(o => new { id_vista = o.Key, count = o.Count() }).ToList();
            
            List<reclamoPorUsuario> listaReclamos = new List<reclamoPorUsuario>();

            /// construye todo el listado de reclamos por usuario
            for (int i=0; i < reclamosAgrupados.Count(); i++){
                // se crea un reclamoPorUsuario por cada vuelta
                reclamoPorUsuario reclamoPorUsuario = new reclamoPorUsuario(); // el constructor va adentro del for :/
                reclamoPorUsuario.id_vista = reclamosAgrupados[i].id_vista;
                reclamoPorUsuario.count = reclamosAgrupados[i].count;
                // se crea una vista con el idVista de esta vuelta para agregar los datos de la vista al reclamo
                vista vista = db.vista.Where(v => v.idVista == reclamoPorUsuario.id_vista).First();
                reclamoPorUsuario.id_vista_id = vista.idVista;
                reclamoPorUsuario.alumno = vista.alumno1.nombre + " " +vista.alumno1.apellido;
                reclamoPorUsuario.estado = vista.estado.descripcion;
                DateTime date = vista.fechaIngreso.Value;
                reclamoPorUsuario.fecha = date.ToString("d");

                listaReclamos.Add(reclamoPorUsuario);
            }
            return View(listaReclamos);
        }

        [Authorize(Roles = "Admin")]
        public ActionResult reclamoTodos()
        {
            string idUser = User.Identity.GetUserId();

            var reclamosAgrupados = db.vista_usuario_reclamo.GroupBy(g => g.id_vista)
            .Select(o => new { id_vista = o.Key, count = o.Count() }).ToList();

            List<reclamoPorUsuario> listaReclamos = new List<reclamoPorUsuario>();

            for (int i = 0; i < reclamosAgrupados.Count(); i++)
            {
                // se crea un reclamoPorUsuario por cada vuelta
                reclamoPorUsuario reclamoPorUsuario = new reclamoPorUsuario(); // el constructor va adentro del for :/
                reclamoPorUsuario.id_vista = reclamosAgrupados[i].id_vista;
                reclamoPorUsuario.count = reclamosAgrupados[i].count;
                // se crea una vista con el idVista de esta vuelta para agregar los datos de la vista al reclamo
                vista vista = db.vista.Where(v => v.idVista == reclamoPorUsuario.id_vista).First();
                reclamoPorUsuario.id_vista_id = vista.idVista;
                reclamoPorUsuario.alumno = vista.alumno1.nombre + vista.alumno1.apellido;
                reclamoPorUsuario.estado = vista.estado.descripcion;
                DateTime date = vista.fechaIngreso.Value;
                reclamoPorUsuario.fecha = date.ToString("d");

                listaReclamos.Add(reclamoPorUsuario);
            }
            return View(listaReclamos);
        }


        //
        // GET: /vista_usuario_reclamo/Details/5
        public ActionResult Details()
        {
            return View();
        }

        //
        // GET: /vista_usuario_reclamo/Create
         [Authorize]
        public ActionResult Create(string id_vista)
        {
            // si no comprobo su dni y se almaceno en una variable session vuelve al index a pedirle su dni
            if (System.Web.HttpContext.Current.Session["sessionStringDni"] as String == null)
            {
                return RedirectToAction("Index", "Home");
            }
            try
            {
                string idUser = User.Identity.GetUserId(); // get ID UserAsp
              
                //AspNetUsers user = db.AspNetUsers.Find(idUser); // busca el usuario con ese id
                ViewBag.user =   User.Identity.Name; // lo paso a la vista creatE
                ViewBag.id_usuario = idUser;
                ViewBag.id_vista = id_vista; // paso el id_vista al create
            }
            catch { }
            return View();
        }

        [Authorize]
         public string getMailsTo(int idVista) {
             string mailsString = "";
             var mails = db.propUser.SqlQuery("select pur.* from vista_usuario_reclamo as vur inner join propUser as pur on vur.id_usuario = pur.fk_id_user where vur.id_vista = @idVista", new SqlParameter("@idVista", idVista)).ToList();

             // contador ARREGLO A LAS APURADAS, NO DEBERIA
             // cuenta la cantidad de mails que hay y en la ultima vuelta no pone la coma despues de ingresar el mail
            
                 int cont = mails.Distinct().Count();
             if (cont != 0) {
                 foreach (var item in mails.Distinct())
                 {
                     cont--;
                     mailsString += item.mail;
                     if (cont > 0)
                         mailsString += " ,";
                 }

                 return mailsString;
             }
             return "";
         }

        //
        // POST: /vista_usuario_reclamo/Create
        [Authorize]
        [HttpPost]
        public ActionResult Create([Bind(Include="id_reclamo, descripcion")] reclamo reclamo,
                                    [Bind(Include="id_usuario, id_vista")] vista_usuario_reclamo vista_usuario_reclamo)
        {
            // si no comprobo su dni y se almaceno en una variable session vuelve al index a pedirle su dni
            if (System.Web.HttpContext.Current.Session["sessionStringDni"] as String == null){
                return RedirectToAction("Index", "Home");
            }
           if (ModelState.IsValid){
                db.reclamo.Add(reclamo);
                db.SaveChanges();
                
                vista_usuario_reclamo.id_reclamo = reclamo.id_reclamo;
                vista_usuario_reclamo.fecha = DateTime.Today;
                string idUser = User.Identity.GetUserId(); // get id del usuario actual // esta parte deberia ser automatica VER BIEN

               // obtener el mail del usuario local que escribio el reclamo
               propUser prop = db.propUser.Where(a => a.fk_id_user.Contains(idUser)).First();
               string mailTo = prop.mail;
               
               vista_usuario_reclamo.id_usuario = idUser; // lo agrego al objeto

               var idVista = vista_usuario_reclamo.id_vista; // se obtiene el idVista actual para mandar link por mail
               string stringCuerpoMail = "Usuario: " + db.AspNetUsers.Find(idUser).UserName + "<br> Comentario: " + reclamo.descripcion + "<br> Fecha:" + vista_usuario_reclamo.fecha + "<br> IR: https://localhost:44300/vista_usuario_reclamo/Index/" + idVista;

               int vista = Convert.ToInt32(idVista);

               // reune todo en un string, todos los mails de los usuarios que reclamaron en esta vista, el mail del usuario local
               // y el mail del alumno de la vista
               string mailsTo = getMailsTo(vista) + ", " + mailTo + ", " + db.vista.Find(vista).alumno1.mail;

                db.vista_usuario_reclamo.Add(vista_usuario_reclamo); // almaceno el objeto
                db.SaveChanges();              
               
                // enviamos mails con los datos obtenidos ( mail destino, asunto, cuerpo)
               enviarMail(mailsTo, "Nuevo ingreso en reclamos", stringCuerpoMail);

                return RedirectToAction("Index","vista_usuario_reclamo", new { id = idVista });
           }
                
             return View();
        }

        //
        // GET: /vista_usuario_reclamo/Edit/5
        //public ActionResult Edit(int id)
        //{
        //    return View();
        //}

        //
        //// POST: /vista_usuario_reclamo/Edit/5
        //[HttpPost]
        //public ActionResult Edit(int id, FormCollection collection)
        //{
        //    try
        //    {
        //        // TODO: Add update logic here

        //        return RedirectToAction("Index");
        //    }
        //    catch
        //    {
        //        return View();
        //    }
        //}

        // GET: /vista_usuario_reclamo/Delete/5
        [Authorize(Roles = "Admin")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            vista_usuario_reclamo vista_usuario_reclamo = db.vista_usuario_reclamo.Find(id);
            if (vista_usuario_reclamo == null)
            {
                return HttpNotFound();
            }
            return View(vista_usuario_reclamo);
        }

        // POST: /vista_usuario_reclamo/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult DeleteConfirmed(int id)
        {
            // si no comprobo su dni y se almaceno en una variable session vuelve al index a pedirle su dni
            if (System.Web.HttpContext.Current.Session["sessionStringDni"] as String == null) {
                return RedirectToAction("Index", "Home");
            }
            vista_usuario_reclamo vista_usuario_reclamo = db.vista_usuario_reclamo.Find(id);
            var idVista = vista_usuario_reclamo.id_vista;
            db.vista_usuario_reclamo.Remove(vista_usuario_reclamo);
            db.SaveChanges();
            return RedirectToAction("Index", "vista_usuario_reclamo", new {id = idVista });
        }
    }
}
