namespace WebApi {
    using DatosOptiaqua;
    using GeoPackageReaderFW;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Web;
    //using System.Web.Http;
    using System.Web.Mvc;
    using static DatosOptiaqua.ImportacionUC;


    /// <summary>
    /// Página de inicio
    /// </summary>
    public class HomeController : Controller {
        /// <summary>
        ///  Index - Página inicial del controlador
        /// </summary>
        /// <returns>The <see cref="ActionResult"/></returns>
        public ActionResult Index() {
            ViewBag.Title = "Página de Inicio";

            return View();
        }

    }
}
