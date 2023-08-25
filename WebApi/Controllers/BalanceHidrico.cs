﻿namespace WebApi {
    using DatosOptiaqua;
    using System;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading;
    using System.Web.Http;

    /// <summary>
    /// Proporciona información del balance hídrico
    /// </summary>
    public class BalanceHidricoController : ApiController {
        /// <summary>
        /// Balance hídrico de una unidad de cultivo en una temporada.
        /// </summary>
        /// <param name="idUnidadCultivo">Identificador de la unidad de cultivo</param>
        /// <param name="fecha">Identificador de la temporada</param>
        /// <param name="actualizaFechasEtapas">Activar si se desea recalcular las fechas de las etapas para la parcela indicada</param>
        /// <returns></returns>
        [Route("api/balancehidrico/{idUnidadCultivo}/{fecha}/{actualizaFechasEtapas}")]
        public IHttpActionResult GetBalanceHidrico(string idUnidadCultivo, string fecha, bool actualizaFechasEtapas) {
            try {               
                return CacheDatosHidricos.Cache(Request.RequestUri.AbsolutePath, () => {
                    BalanceHidrico bh = BalanceHidrico.Balance(idUnidadCultivo, DateTime.Parse(fecha), actualizaFechasEtapas);
                    var ret = Json(bh.LineasBalance);                    
                    return ret;
                });

            } catch (Exception ex) {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Retorna resumen de los datos hídricos a una fecha.
        /// </summary>
        /// <param name="idUnidadCultivo"></param>
        /// <param name="fecha"></param>
        /// <returns></returns>
        [Authorize]
        [Route("api/DatosHidricos/{idUnidadCultivo}/{fecha}")]
        public IHttpActionResult GetDatosHidricos(string idUnidadCultivo, string fecha) {
            try {
                DateTime dFecha = DateTime.Parse(fecha);
                ClaimsIdentity identity = Thread.CurrentPrincipal.Identity as ClaimsIdentity;
                int idUsuario = int.Parse(identity.Claims.SingleOrDefault(c => c.Type == "IdRegante").Value);
                string role = identity.Claims.SingleOrDefault(c => c.Type == ClaimTypes.Role).Value;
                string idTemporada = DB.TemporadaDeFecha(idUnidadCultivo, dFecha);
                if (!DB.EstaAutorizado(idUsuario, role, idUnidadCultivo, idTemporada))
                    return Unauthorized();
                return CacheDatosHidricos.Cache(Request.RequestUri.AbsolutePath+"Usuario"+idUsuario.ToString(), () => {
                    BalanceHidrico bh = BalanceHidrico.Balance(idUnidadCultivo, dFecha);
                    return Json(bh.DatosEstadoHidrico(dFecha));
                });

            } catch (Exception ex) {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Listado de los balances hídricos
        /// </summary>
        /// <param name="idRegante"></param>
        /// <param name="idUnidadCultivo"></param>
        /// <param name="idMunicipio"></param>
        /// <param name="idCultivo"></param>
        /// <param name="fecha"></param>
        /// <returns></returns>        
        [HttpGet]
        [Authorize]
        [Route("api/DatosHidricos/{idRegante}/{idUnidadCultivo}/{idMunicipio}/{idCultivo}/{fecha}")]
        public IHttpActionResult GetDatosHidricosList(int? idRegante, string idUnidadCultivo, int? idMunicipio, string idCultivo, string fecha) {
            try {
                ClaimsIdentity identity = Thread.CurrentPrincipal.Identity as ClaimsIdentity;
                int idUsuario = int.Parse(identity.Claims.SingleOrDefault(c => c.Type == "IdRegante").Value);
                string role = identity.Claims.SingleOrDefault(c => c.Type == ClaimTypes.Role).Value;

                return CacheDatosHidricos.Cache(Request.RequestUri.AbsolutePath+"Usuario"+idUsuario, () => {
                    object lDatosHidricos = BalanceHidrico.DatosHidricosList(idRegante, idUnidadCultivo, idMunicipio, idCultivo, fecha, role, idUsuario);
                    return  Json(lDatosHidricos);                    
                });

            } catch (Exception ex) {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Retornar los Riegos de una unidad de cultivo en una temporada
        /// </summary>
        /// <param name="idUnidadCultivo"></param>
        /// <param name="fecha"></param>
        /// <returns></returns>
        [Authorize]
        [Route("api/Riegos/{idUnidadCultivo}/{fecha}")]
        public IHttpActionResult GetRiegos(string idUnidadCultivo, string fecha) {
            try {
                DateTime dFecha = DateTime.Parse(fecha);
                string idTemporada = DB.TemporadaDeFecha(idUnidadCultivo, dFecha);
                if (idTemporada == null)
                    return BadRequest("La unidad de cultivo no está definida para la temporada");

                ClaimsIdentity identity = Thread.CurrentPrincipal.Identity as ClaimsIdentity;
                int idUsuario = int.Parse(identity.Claims.SingleOrDefault(c => c.Type == "IdRegante").Value);
                string role = identity.Claims.SingleOrDefault(c => c.Type == ClaimTypes.Role).Value;
                if (!DB.EstaAutorizado(idUsuario, role, idUnidadCultivo, idTemporada))
                    return Unauthorized();

                return CacheDatosHidricos.Cache(Request.RequestUri.AbsolutePath+"Usuario"+idUsuario.ToString(), () => {
                    return Json(DB.DatosRiegosList(idUnidadCultivo, idTemporada));
                });

            } catch (Exception ex) {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Retornar las lluvias registradas para una unidad de cultivo en una temporada
        /// </summary>
        /// <param name="idUnidadCultivo"></param>
        /// <param name="fecha"></param>
        /// <returns></returns>
        [Authorize]
        [Route("api/Lluvias/{idUnidadCultivo}/{fecha}")]
        public IHttpActionResult GetLluvias(string idUnidadCultivo, string fecha) {
            try {

                ClaimsIdentity identity = Thread.CurrentPrincipal.Identity as ClaimsIdentity;
                int idUsuario = int.Parse(identity.Claims.SingleOrDefault(c => c.Type == "IdRegante").Value);
                string role = identity.Claims.SingleOrDefault(c => c.Type == ClaimTypes.Role).Value;

                return CacheDatosHidricos.Cache(Request.RequestUri.AbsolutePath + "Usuario" + idUsuario.ToString(), () => {
                    string idTemporada = DB.TemporadaDeFecha(idUnidadCultivo, DateTime.Parse(fecha));
                    if (!DB.EstaAutorizado(idUsuario, role, idUnidadCultivo, idTemporada))
                        return Unauthorized();
                    return Json(DB.DatosLluviaList(idUnidadCultivo, idTemporada));
                });

            } catch (Exception ex) {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        ///  ResumenDiario
        /// </summary>
        /// <param name="idUnidadCultivo">The idUnidadCultivo<see cref="string"/></param>
        /// <param name="fecha"></param>                
        /// <returns>The <see cref="IHttpActionResult"/></returns>
        [HttpGet]
        [Route("api/ResumenDiario/{idUnidadCultivo}/{fecha}")]
        public IHttpActionResult ResumenDiario(string idUnidadCultivo, string fecha) {
            try {
                DateTime dFecha = DateTime.Parse(fecha);

                return CacheDatosHidricos.Cache(Request.RequestUri.AbsolutePath, () => {
                    BalanceHidrico bh = BalanceHidrico.Balance(idUnidadCultivo, dFecha);
                    System.Web.Http.Results.JsonResult<Models.ResumenDiario> ret = Json(bh.ResumenDiario(dFecha));
                    return ret;
                });

            } catch (Exception ex) {
                return BadRequest(ex.Message);
            }
        }


        [HttpGet]
        [Route("api/Recalcula/")]
        public IHttpActionResult Recalcula() {
            try {
                CacheDatosHidricos.RecreateAll();
                return Json("OK");
            } catch (Exception ex) {
                CacheDatosHidricos.recalculando = false;
                return BadRequest(ex.Message);
            }
        }
    }
}
