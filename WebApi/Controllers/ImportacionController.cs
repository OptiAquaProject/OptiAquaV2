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
    public class ImportacionController : Controller {

        public ActionResult Importacion() {
            ViewBag.Title = "Importación para la creación masica de unidades de cultivo.";
            return View();
        }

        public ActionResult ImportacionAnalisisSuelo() {
            ViewBag.Title = "Importación masiva de análisis de suelos";
            return View();
        }

        public ActionResult EspecificacionesImportacion() {
            return View();
        }

        public ActionResult EspecificacionesimportacionAnalisisSuelo() {
            return View();
        }

        public ActionResult ImportacionMapas() {
            var lMapaVersion = DB.DatosMapasVersion();
            ViewBag.lMapaVersion = lMapaVersion;
            return View();
        }

        [HttpPost]
        public string ImportarMapasPost() {
            try {
                if (Request.Files.Count == 0)
                    return "KO";
                HttpPostedFileBase fileBase = Request.Files[0];
                HttpPostedFileBase fileCatastral = Request.Files[1];

                var fileNameBase = DB.PathRoot + DateTime.Now.Ticks.ToString() + Path.GetExtension(fileBase.FileName);
                fileBase.SaveAs(fileNameBase);

                var fileNameCatastral = DB.PathRoot + DateTime.Now.Ticks + 1.ToString() + Path.GetExtension(fileCatastral.FileName);
                fileCatastral.SaveAs(fileNameCatastral);

                var idVersion = HttpContext.Request.Params["idVersion"];
                var fb = HttpContext.Request.Params["fileBase"];
                DB.EliminarMapas(idVersion);
                var err = ImportMapas.ImportarMapaBase(fileNameBase, idVersion);
                if (!string.IsNullOrWhiteSpace(err))
                    return err;
                err = ImportMapas.ImportarMapaCatastral(fileNameCatastral, idVersion);
                if (!string.IsNullOrWhiteSpace(err))
                    return err;
                System.IO.File.Delete(fileNameBase);
                System.IO.File.Delete(fileNameCatastral);
                return "OK";
            } catch (Exception ex) {
                return "KO";
            }
        }

        [HttpPost]
        public string EliminarMapas() {
            var idVersion = HttpContext.Request.Params["paramJson"];
            DB.EliminarMapas(idVersion);
            return "OK";
        }

        [HttpPost]
        public string ImportarUCPost() {
            try {
                //var lErrores = Importacion.Importar(param);
                if (Request.Files.Count == 0)
                    return "Error";
                HttpPostedFileBase fileBase = Request.Files[0];
                var excel = MiniExcelLibs.MiniExcel.Query<ImportItemUCExcel>(fileBase.InputStream).ToList();

                var nif = HttpContext.Request.Params["NifRegante"];
                var pass = HttpContext.Request.Params["PassRegante"];
                if (!DB.IsCorrectPassword(new Models.LoginRequest { NifRegante = nif, Password = pass }, out var regante)) {
                    var lErr = new List<ErrorItem> {
                        new ErrorItem{NLinea=0, Descripcion="Usuario o contraseña no válidos" }
                    };
                    return Newtonsoft.Json.JsonConvert.SerializeObject(lErr );
                }
                var lErrores = ImportarUcFromExcel(nif, pass, excel, out var nImportados);
                if (lErrores.Count > 0)
                    return Newtonsoft.Json.JsonConvert.SerializeObject(lErrores);
                else {
                    return "OK:" + nImportados.ToString();
                }
            } catch (Exception ex) {
                return "Error:" + ex.Message;
            }
        }
    }

    public class ImportItemUCExcel {
        public int Linea { set; get; }
        /// <summary>
        /// Gets or sets the IdUnidadCultivo.
        /// </summary>
        /// 

        public string IdUnidadCultivo { set; get; }

        /// <summary>
        /// Gets or sets the IdRegante.
        /// </summary>
        public int IdRegante { set; get; }

        /// <summary>
        /// Gets or sets the IdEstacion.
        /// </summary>
        public int IdEstacion { set; get; }

        /// <summary>
        /// Gets or sets the Alias.
        /// </summary>
        public string Alias { set; get; }

        /// <summary>
        /// Gets or sets the IdTemporada.
        /// </summary>
        public string IdTemporada { set; get; }

        /// <summary>
        /// Gets or sets the IdParcelaIntList.
        /// </summary>
        public string IdParcelaIntList { set; get; }

        /// <summary>
        /// Gets or sets the IdCultivo.
        /// </summary>
        public int IdCultivo { set; get; }

        /// <summary>
        /// Gets or sets the FechaSiembra.
        /// </summary>
        public DateTime FechaSiembra { set; get; }

        /// <summary>
        /// Gets or sets the IdTipoRiego.
        /// </summary>
        public int IdTipoRiego { set; get; }

        /// <summary>
        /// Gets or sets the SuperficieM2.
        /// </summary>
        public double? SuperficieM2 { set; get; }
    }

}
