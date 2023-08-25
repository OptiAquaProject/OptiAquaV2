namespace WebApi {
    using DatosOptiaqua;
    using Models;
    using System;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading;
    using System.Web.Http;
    using webapi.Utiles;

    /// <summary>
    /// Proporciona los datos de las unidades de cultivo y las propiedades de su suelo.
    /// </summary>
    public class UnidadCultivoController : ApiController {
        /// <summary>
        /// Datos de la unidad de cultivo
        /// </summary>
        /// <param name="idUnidadCultivo"></param>
        /// <returns></returns>
        [Authorize]
        [Route("api/UnidadCultivo/{idUnidadCultivo}")]
        public IHttpActionResult Get(string idUnidadCultivo) {
            try {
                return CacheDatosHidricos.Cache(Request.RequestUri.AbsolutePath, () => {
                    return Json(DB.UnidadCultivo(idUnidadCultivo));
                });
            } catch (Exception ex) {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Lista de las unidades de Cultivo para una temporada
        /// </summary>
        /// <param name="fecha"></param>
        /// <returns></returns>
        [Authorize]
        [Route("api/UnidadesDeCultivo/{fecha}")]
        public IHttpActionResult GetUnidadesDeCultivo(string fecha) {
            try {
                ClaimsIdentity identity = Thread.CurrentPrincipal.Identity as ClaimsIdentity;
                int idUsuario = int.Parse(identity.Claims.SingleOrDefault(c => c.Type == "IdRegante").Value);
                var role = identity.Claims.SingleOrDefault(c => c.Type == ClaimTypes.Role).Value;
                return CacheDatosHidricos.Cache(Request.RequestUri.AbsolutePath + "Usuario" + idUsuario.ToString(), () => {
                    var lTemporadas = DB.TemporadasDeFecha(DateTime.Parse(fecha));
                    return Json(DB.UnidadesDeCultivoList(lTemporadas, idUsuario, role));
                });
            } catch (Exception ex) {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [Route("api/UnidadCultivo/{idParcelaInt}/{idHorizonte}")]
        public IHttpActionResult GetHorizontes(int idParcelaInt, string idHorizonte) {
            try {
                var ret = Json(DB.PacelaHorizonte(idParcelaInt, idHorizonte));
                return ret;
            } catch (Exception ex) {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Defines the <see cref="ParamPostHorizonte" />
        /// </summary>
        public class ParamPostHorizonte {
            /// <summary>
            /// Gets or sets the IdUnidadCultivo
            /// </summary>
            public int IdParcelaInt { set; get; }

            /// <summary>
            /// Gets or sets the IdTemporada
            /// </summary>
            public string Fecha { set; get; }

            /// <summary>
            /// Gets or sets the IdHorizonte
            /// </summary>
            public int IdHorizonte { set; get; }

            /// <summary>
            /// Gets or sets the Limo
            /// </summary>
            public float Limo { set; get; }

            /// <summary>
            /// Gets or sets the Arcilla
            /// </summary>
            public float Arcilla { set; get; }

            /// <summary>
            /// Gets or sets the Arena
            /// </summary>
            public float Arena { set; get; }

            /// <summary>
            /// Gets or sets the MatOrg
            /// </summary>
            public float MatOrg { set; get; }

            /// <summary>
            /// Gets or sets the EleGru
            /// </summary>
            public float EleGru { set; get; }

            /// <summary>
            /// Gets or sets the Prof
            /// </summary>
            public float Prof { set; get; }
        }

        /// <summary>
        /// Actulizar datos horizonte
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        public IHttpActionResult PostHorizonte([FromBody] ParamPostHorizonte param) {
            try {
                DB.ParcelaSueloSave(param.IdParcelaInt, param.IdHorizonte, param.Limo, param.Arcilla, param.Arena, param.MatOrg, param.EleGru, param.Prof);
                CacheDatosHidricos.SetDirtyParcela(param.IdParcelaInt);
                return Ok();
            } catch (Exception) {
                return BadRequest();
            }
        }

        /// <summary>
        /// Lista datos ampliados de unidades de cultivos con filtros
        /// </summary>
        /// <param name="fecha"></param>
        /// <param name="idUnidadCultivo"></param>
        /// <param name="idRegante"></param>
        /// <param name="idCultivo"></param>
        /// <param name="idMunicipio"></param>
        /// <param name="idTipoRiego"></param>
        /// <param name="idEstacion"></param>
        /// <param name="idPoligono"></param>
        /// <param name="idParcela"></param>
        /// <param name="search"></param>     
        /// <returns></returns>
        [Authorize]
        [Route("api/UnidadCultivoList/{Fecha}/{IdUnidadCultivo}/{IdRegante}/{IdCultivo}/{IdMunicipio}/{IdTipoRiego}/{IdEstacion}/{IdPoligono}/{IdParcela}/{Search}")]
        public IHttpActionResult GetUnidadCultivoList(string fecha, string idUnidadCultivo, string idRegante, string idCultivo, string idMunicipio, string idTipoRiego, string idEstacion, string idPoligono, string idParcela, string search) {
            try {

                ClaimsIdentity identity = Thread.CurrentPrincipal.Identity as ClaimsIdentity;
                int idUsuario = int.Parse(identity.Claims.SingleOrDefault(c => c.Type == "IdRegante").Value);
                var role = identity.Claims.SingleOrDefault(c => c.Type == ClaimTypes.Role).Value;

                var idTemporada = "";
                if (DateTime.TryParse(fecha, out var dFecha))
                    idTemporada = DB.TemporadaDeFecha(idUnidadCultivo.Unquoted(), dFecha);
                else
                    idTemporada = DB.TemporadaActiva();

                return CacheDatosHidricos.Cache(Request.RequestUri.AbsolutePath + "Usuario" + idUsuario.ToString(), () => {
                    var ret = Json(DB.UnidadCultivoList(idTemporada, idUnidadCultivo, idRegante, idCultivo, idMunicipio, idTipoRiego, idPoligono, idParcela, search, idUsuario, role));
                    return ret;
                });

            } catch (Exception ex) {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Retornar datos ampliados de la unidad de cultivo.
        /// Fecha puede ser '' para presentar todos
        /// IdUnidadCultivo puede ser '' para presentar todos
        /// </summary>
        /// <param name="Fecha"></param>
        /// <param name="IdUnidadCultivo"></param>
        /// <returns></returns>
        [Authorize]
        [HttpGet]
        [Route("api/UnidadCultivoDatosAmpliados/{Fecha}/{IdUnidadCultivo}")]
        public IHttpActionResult GetUnidadCultivoDatosAmpliados(string Fecha, string IdUnidadCultivo) {
            try {
                DateTime FechaEstudio = DateTime.Today;
                if (!string.IsNullOrWhiteSpace(Fecha))
                    FechaEstudio = DateTime.Parse(Fecha);

                return CacheDatosHidricos.Cache(Request.RequestUri.AbsolutePath, () => {
                    var ret = Json(DB.UnidadCultivoDatosAmpliados(FechaEstudio, IdUnidadCultivo.Unquoted()));
                    return ret;
                });


            } catch (Exception ex) {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        ///  UnidadCultivoTemporadaCosteM3Agua
        /// </summary>
        /// <param name="Fecha">Fecha<see cref="string"/></param>
        /// <param name="IdUnidadCultivo">The IdUnidadCultivo<see cref="string"/></param>
        /// <returns>The <see cref="IHttpActionResult"/></returns>
        [Authorize]
        [HttpGet]
        [Route("api/UnidadCultivoTemporadaCosteM3Agua/{IdUnidadCultivo}/{Fecha}")]
        public IHttpActionResult UnidadCultivoTemporadaCosteM3Agua(string Fecha, string IdUnidadCultivo) {
            try {
                return CacheDatosHidricos.Cache(Request.RequestUri.AbsolutePath, () => {
                    var idTemporada = DB.TemporadaDeFecha(IdUnidadCultivo, DateTime.Parse(Fecha));
                    return Json(DB.UnidadCultivoTemporadaCosteM3Agua(IdUnidadCultivo, idTemporada));
                });
            } catch (Exception ex) {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        ///  PostUnidadCultivoTemporadaCosteM3Agua
        /// </summary>
        /// <param name="param">The param<see cref="ParamPostCosteM3Agua"/></param>
        /// <returns>The <see cref="IHttpActionResult"/></returns>
        [Authorize]
        [HttpPost]
        [Route("api/UnidadCultivoTemporadaCosteM3Agua/")]
        public IHttpActionResult PostUnidadCultivoTemporadaCosteM3Agua([FromBody] ParamPostCosteM3Agua param) {
            try {
                CacheDatosHidricos.SetDirtyUC(param.IdUnidadCultivo);
                return Json(DB.UnidadCultivoTemporadaCosteM3AguaSave(param));
            } catch (Exception ex) {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpGet]
        [Route("api/AsesorUnidadCultivo/{IdRegante}")]
        public IHttpActionResult GetAsesorUnidadCultivo(int idRegante) {
            try {
                return CacheDatosHidricos.Cache(Request.RequestUri.AbsolutePath, () => {
                    return Json(DB.AsesorUnidadCultivoList(idRegante));
                });
            } catch (Exception ex) {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpPost]
        [Route("api/AsesorUnidadCultivo/")]
        public IHttpActionResult PostAsesorUnidadCultivo([FromBody] ParamAsesorUnidadCultivo param) {
            try {
                param.LUnidadesCultivo = param.LUnidadesCultivo.Replace(";", "#");
                var lUnidadesCultivo = param.LUnidadesCultivo.Split('#').ToList();
                foreach (var iuc in lUnidadesCultivo)
                    CacheDatosHidricos.SetDirtyUC(iuc);
                return Json(DB.AsesorUnidadCultivoSave(param.IdRegante, lUnidadesCultivo));
            } catch (Exception ex) {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpDelete]
        [Route("api/UnidadCultivoDelete/{lIdUnidadCultivos}/{idTemporada}")]
        public IHttpActionResult UnidadCultivoDelete(string lIdUnidadCultivos, string idTemporada) {
            try {
                return Json(DB.UnidadCultivoDelete(lIdUnidadCultivos, idTemporada));
            } catch (Exception ex) {
                return BadRequest(ex.Message);
            }
        }
    }
}
