﻿namespace DatosOptiaqua {
    using Models;
    using Newtonsoft.Json;
    using NPoco;
    using Org.BouncyCastle.Crypto;
    using Org.BouncyCastle.Crypto.Signers;
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data.SqlTypes;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using webapi;
    using webapi.Utiles;
    using static WebApi.DatosExtraController;

    /// <summary>
    /// Capa de acceso a las base de datos OptiAqua y Nebula en SQl Server
    /// Para simplificar el acceso se hace uso de la librería NPoco - https://github.com/schotime/NPoco
    /// La cadena de conexión CadenaConexionOptiAqua se define como parámetro de la aplicación.
    /// La cadena de conexión Nebula se define como parámetro de la aplicación.
    /// </summary>
    public static partial class DB {

        public static string PathRoot { get; set; }
        public static int IdEstacionDefault = 503;

        /// <summary>
        /// Gets the CadenaConexionOptiAqua.
        /// </summary>

        internal static void ImportarHidrantes() {
            Database db = ConexionOptiaqua;
            List<Dictionary<string, string>> ldat = db.Fetch<Dictionary<string, string>>("select * from tempHidrantes");
            List<string> lUcsRepetidas = ldat.Select(x => x["Hid"]).ToList();
            List<string> lUcs = lUcsRepetidas.Distinct().ToList();
            foreach (string idUc in lUcs) {

            }
        }

        public class cParam {
            public double? TBase { get; set; }
            public double ProfRaizInicial { get; set; }
            public double ProfRaizMax { get; set; }
            public double ModCobCoefA { get; set; }
            public double ModCobCoefB { get; set; }
            public double? ModCobCoefC { get; set; }
            public double ModAltCoefA { get; set; }
            public double ModAltCoefB { get; set; }
            public double? ModAltCoefC { get; set; }
            public double ModRaizCoefA { get; set; }
            public double ModRaizCoefB { get; set; }
            public double? ModRaizCoefC { get; set; }
            public double? AlturaInicial { get; set; }
            public double? AlturaFinal { get; set; }
            public double IntegralEmergencia { get; set; }
            public double CoberturaInicial { get; set; }
            public double CoberturaFinal { get; set; }
        }


        /*
        internal static void PropagarJsonCultivo() {
            Database db = DB.ConexionOptiaqua;
            List<Cultivo> lCultivos = db.Fetch<Cultivo>();
            foreach (Cultivo c in lCultivos) {
                Dictionary<string, double> dParam = new Dictionary<string, double> {
                    { "ModCobCoefA", c.ModCobCoefA },
                    { "ModCobCoefB", c.ModCobCoefB }
                };
                if (c.ModCobCoefC != null)
                    dParam.Add("ModCobCoefC", c.ModCobCoefC ?? 0);
                dParam.Add("ModAltCoefA", c.ModAltCoefA);
                dParam.Add("ModAltCoefB", c.ModAltCoefB);
                if (c.ModAltCoefC != null)
                    dParam.Add("ModAltCoefC", c.ModAltCoefC ?? 0);
                dParam.Add("ModRaizCoefA", c.ModRaizCoefA);
                dParam.Add("ModRaizCoefB", c.ModRaizCoefB);
                if (c.ModRaizCoefC != null)
                    dParam.Add("ModRaizCoefC", c.ModRaizCoefC ??0);
                
                string json = Newtonsoft.Json.JsonConvert.SerializeObject(dParam, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                c.ParametrosJson = json;
                db.Save(c);
            }
        }
        */

        internal static void QuitarParametrosJson() {
            Database db = DB.ConexionOptiaqua;
            var lEtepas = db.Fetch<UnidadCultivoCultivoEtapas>();
            foreach (var e in lEtepas) {
                Dictionary<string, double> dParam = JsonConvert.DeserializeObject<Dictionary<string, double>>(e.ParametrosJson);
                if (dParam.Keys.Contains("TBase"))
                    dParam.Remove("TBase");

                if (dParam.Keys.Contains("ProfRaizInicial"))
                    dParam.Remove("ProfRaizInicial");

                if (dParam.Keys.Contains("ProfRaizMax"))
                    dParam.Remove("ProfRaizMax");

                if (dParam.Keys.Contains("IntegralEmergencia"))
                    dParam.Remove("IntegralEmergencia");

                string json = Newtonsoft.Json.JsonConvert.SerializeObject(dParam, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                e.ParametrosJson = json;
                db.Save(e);
            }

        }

        internal static void PropagarEtapas() {
            Database db = DB.ConexionOptiaqua;
            List<UnidadCultivoCultivoEtapas> lCultivoCultivoEtapas = db.Fetch<UnidadCultivoCultivoEtapas>();
            int i = 0;
            foreach (UnidadCultivoCultivoEtapas unidadCultivoCultivoEtapa in lCultivoCultivoEtapas) {
                i++;
                UnidadCultivoCultivo uniCul = new UnidadCultivoCultivo {
                    IdUnidadCultivo = unidadCultivoCultivoEtapa.IdUnidadCultivo,
                    IdTemporada = unidadCultivoCultivoEtapa.IdTemporada
                };
                uniCul = db.SingleById<UnidadCultivoCultivo>(uniCul);

                CultivoEtapas etapa = new CultivoEtapas {
                    IdCultivo = uniCul.IdCultivo,
                    OrdenEtapa = unidadCultivoCultivoEtapa.IdEtapaCultivo
                };
                etapa = db.SingleOrDefaultById<CultivoEtapas>(etapa);
                if (unidadCultivoCultivoEtapa.ParametrosJson != etapa.ParametrosJson) {
                    unidadCultivoCultivoEtapa.ParametrosJson = etapa.ParametrosJson;
                    db.Save(unidadCultivoCultivoEtapa);
                }
            }
        }

        internal static void PropagarEtapas2() {
            Database db = DB.ConexionOptiaqua;
            List<UnidadCultivoCultivoEtapas> lEtapas = db.Fetch<UnidadCultivoCultivoEtapas>();
            int i = 0;
            foreach (UnidadCultivoCultivoEtapas unidadCultivoCultivoEtapa in lEtapas) {
                i++;

                UnidadCultivoCultivo uniCul = new UnidadCultivoCultivo {
                    IdUnidadCultivo = unidadCultivoCultivoEtapa.IdUnidadCultivo,
                    IdTemporada = unidadCultivoCultivoEtapa.IdTemporada
                };
                uniCul = db.SingleById<UnidadCultivoCultivo>(uniCul);

                CultivoEtapas etapa = new CultivoEtapas {
                    IdCultivo = uniCul.IdCultivo,
                    OrdenEtapa = unidadCultivoCultivoEtapa.IdEtapaCultivo
                };
                etapa = db.SingleOrDefaultById<CultivoEtapas>(etapa);
                unidadCultivoCultivoEtapa.AlturaInicial = etapa.AlturaInicial;
                unidadCultivoCultivoEtapa.AlturaFinal = etapa.AlturaFinal;
                unidadCultivoCultivoEtapa.IdTipoCalculoAltura = etapa.IdTipoCalculoAltura;
                unidadCultivoCultivoEtapa.IdTipoCalculoCobertura = etapa.IdTipoCalculoCobertura;
                unidadCultivoCultivoEtapa.IdTipoCalculoLongitudRaiz = etapa.IdTipoCalculoLongitudRaiz;
                //unidadCultivoCultivoEtapa.ParametrosJson = etapa.ParametrosJson;
                db.Save(unidadCultivoCultivoEtapa);

            }
        }


#if DEBUG
        public static string CadenaConexionOptiAqua = "CadenaConexionOptiAqua-Debug";

        public static string CadenaConexionNebula => "CadenaConexionNebula-Debug";

#else
        public static string CadenaConexionOptiAqua = "CadenaConexionOptiAqua";
        public static string CadenaConexionNebula => "CadenaConexionNebula";
#endif

        /// <summary>
        /// Gets the ConexionOptiaqua.
        /// </summary>
        //static private Database dbConexion =null;
        public static Database ConexionOptiaqua {
            get {
                var cnnStr = ConfigurationManager.ConnectionStrings[DB.CadenaConexionOptiAqua].ConnectionString;
                return new Database(cnnStr, DatabaseType.SqlServer2012, System.Data.SqlClient.SqlClientFactory.Instance);
            }
        }

        public static Database ConexionNebula {
            get {
                var cnnStr = ConfigurationManager.ConnectionStrings[DB.CadenaConexionNebula].ConnectionString;
                return new Database(cnnStr, DatabaseType.SqlServer2012, System.Data.SqlClient.SqlClientFactory.Instance);
            }
        }

        /// <summary>
        /// The InsertaEvento.
        /// </summary>
        /// <param name="txt">The txt<see cref="string"/>.</param>
        internal static void InsertaEvento(string txt) => DB.ConexionOptiaqua.Insert(new EventosPoco { Evento = txt });

        /// <summary>
        /// The IsCorrectPassword.
        /// </summary>
        /// <param name="login">The login<see cref="LoginRequest"/>.</param>
        /// <param name="regante">The regante<see cref="Regante"/>.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        public static bool IsCorrectPassword(LoginRequest login, out Regante regante) {
            regante = null;
            try {
                Database db = DB.ConexionOptiaqua;
                regante = db.SingleOrDefault<Regante>("select * from regante where nif=@0", login.NifRegante);
                if (regante == null)
                    return false;
                string pass1 = BuildPassword(login.NifRegante, login.Password);
                return regante.Contraseña == pass1;
            } catch {
                return false;
            }
        }

        /// <summary>
        /// Retorna si el password es correcto para en nif indicado.
        /// </summary>
        /// <param name="nif">The nif<see cref="string"/>.</param>
        /// <param name="password">The password<see cref="string"/>.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        public static bool IsCorrectPassword(string nif, string password) {
            try {
                Database db = DB.ConexionOptiaqua;
                Regante regante = db.SingleOrDefault<Regante>("select * from regante where nif=@0", nif);
                if (regante == null)
                    return false;
                string pass1 = BuildPassword(nif, password);
                return regante.Contraseña == pass1;
            } catch {
                return false;
            }
        }

        /// <summary>
        /// Retorna lista de avisos con los filtros indicados según los parámetros. 
        /// Pasar parámetro con valor '' si no se desea filtrar por campo.
        /// </summary>
        /// <param name="idAviso">idAviso<see cref="string"/>.</param>
        /// <param name="idAvisoTipo">idAvisoTipo<see cref="int?"/>.</param>
        /// <param name="fInicio">fInicio<see cref="DateTime?"/>.</param>
        /// <param name="fFin">fFin<see cref="DateTime?"/>.</param>
        /// <param name="de">de<see cref="string"/>.</param>
        /// <param name="para">para<see cref="string"/>.</param>
        /// <param name="search">search<see cref="string"/>.</param>
        /// <returns><see cref="object"/>.</returns>
        public static object AvisosList(string idAviso, int? idAvisoTipo, DateTime? fInicio, DateTime? fFin, string de, string para, string search) {
            Database db = DB.ConexionOptiaqua;
            idAviso = idAviso.Quoted();
            string strFInicio = fInicio?.ToString().Quoted() ?? "''";
            string strFFin = fFin?.ToString().Quoted() ?? "''";
            string strIdAvisoTipo = idAvisoTipo?.ToString() ?? "''";
            de = de.Quoted();
            para = para.Quoted();
            search = search.Quoted();
            string sql = $"SELECT * FROM AvisosList({idAviso},{strIdAvisoTipo},{strFInicio},{strFFin},{de},{para},{search})";
            return db.Fetch<object>(sql);
        }

        /// <summary>
        /// The MultimediaList.
        /// </summary>
        /// <param name="idMultimedia">The idMultimedia<see cref="int?"/>.</param>
        /// <param name="idMultimediaTipo">The idMultimediaTipo<see cref="int?"/>.</param>
        /// <param name="fInicio">The fInicio<see cref="DateTime?"/>.</param>
        /// <param name="fFin">The fFin<see cref="DateTime?"/>.</param>
        /// <param name="activa">The activa<see cref="int?"/>.</param>
        /// <param name="search">The search<see cref="string"/>.</param>
        /// <returns>The <see cref="object"/>.</returns>
        public static object MultimediaList(int? idMultimedia, int? idMultimediaTipo, DateTime? fInicio, DateTime? fFin, int? activa, string search) {
            Database db = DB.ConexionOptiaqua;
            string strFInicio = fInicio?.ToString().Quoted() ?? "''";
            string strFFin = fFin?.ToString().Quoted() ?? "''";
            string strIdMultimedia = idMultimedia?.ToString() ?? "''";
            string strIdMultimediaTipo = idMultimediaTipo?.ToString() ?? "''";
            string strActiva = activa?.ToString() ?? "''";
            search = search.Quoted();
            string sql = $"SELECT * FROM MultimediaList({strIdMultimedia},{strIdMultimediaTipo},{strFInicio},{strFFin},{strActiva},{search})";
            return db.Fetch<object>(sql);
        }

        /// <summary>
        /// The MultimediaTipoList.
        /// </summary>
        /// <param name="idMultimediaTipo">The idMultimediaTipo<see cref="int?"/>.</param>
        /// <param name="search">The search<see cref="string"/>.</param>
        /// <returns>The <see cref="object"/>.</returns>
        public static object MultimediaTipoList(int? idMultimediaTipo, string search) {
            Database db = DB.ConexionOptiaqua;
            string strIdMultimediaTipo = idMultimediaTipo?.ToString() ?? "''";
            search = search.Quoted();
            string sql = $"SELECT * FROM MultimediaTipoList({strIdMultimediaTipo},{search})";
            return db.Fetch<object>(sql);
        }

        /// <summary>
        /// The EstaAutorizado.
        /// </summary>
        /// <param name="idUsuario">The idUsuario<see cref="int"/>.</param>
        /// <param name="role">The role<see cref="string"/>.</param>
        /// <param name="idUnidadCultivo">The idUnidadCultivo<see cref="string"/>.</param>
        /// <param name="idTemporada">The idTemporada<see cref="string"/>.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        internal static bool EstaAutorizado(int idUsuario, string role, string idUnidadCultivo, string idTemporada) {
            if (role == "admin")
                return true;
            if (role == "asesor") {
                List<string> lAsesor = DB.AsesorUnidadCultivoList(idUsuario);
                return lAsesor.Contains(idUnidadCultivo);
            }
            if (role == "dbo")
                return DB.LaUnidadDeCultivoPerteneceAlReganteEnLaTemporada(idUnidadCultivo, idUsuario, idTemporada);
            return false;
        }

        internal static List<string> UnidadCultivoDelete(string lIdUnidadCultivos, string idTemporada) {
            var ret = new List<string>();
            var db = DB.ConexionOptiaqua;
            var temporada = db.SingleOrDefaultById<Temporada>(idTemporada);
            if (temporada == null) {
                ret.Add("No se encontrol temporada");
                return ret;
            }
            var lUniCul = lIdUnidadCultivos.Split(',').ToList();
            if (lUniCul.Count == 0) {
                ret.Add("No se han indicado unididades de cultivo");
                return ret;
            }

            foreach (var iuc in lUniCul) {
                var ucc = new UnidadCultivoCultivo {
                    IdUnidadCultivo = iuc,
                    IdTemporada = idTemporada
                };
                var n1 = db.Delete<UnidadCultivoDatosExtra>("where IdUnidadCultivo=@0 and fecha between @1 and @2", iuc, temporada.FechaInicial, temporada.FechaFinal);
                var nDel = db.Delete(ucc);
                if (nDel == 1) {
                    ret.Add($"Eliminada UC: {iuc}");
                } else {
                    ret.Add($"No se puedo eliminar UC: {iuc}");
                }
            }
            return ret;
        }

        /// <summary>
        /// The EstaAutorizado.
        /// </summary>
        /// <param name="idUsuario">The idUsuario<see cref="int"/>.</param>
        /// <param name="role">The role<see cref="string"/>.</param>
        /// <param name="idUnidadCultivo">The idUnidadCultivo<see cref="string"/>.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        internal static bool EstaAutorizado(int idUsuario, string role, string idUnidadCultivo) {
            if (role == "admin")
                return true;
            if (role == "asesor") {
                List<string> lAsesor = DB.AsesorUnidadCultivoList(idUsuario);
                return lAsesor.Contains(idUnidadCultivo);
            }
            if (role == "dbo")
                return DB.LaUnidadDeCultivoPerteneceAlRegante(idUnidadCultivo, idUsuario);
            return false;
        }

        /// <summary>
        /// The EstaAutorizado.
        /// </summary>
        /// <param name="idUsuario">The idUsuario<see cref="int"/>.</param>
        /// <param name="role">The role<see cref="string"/>.</param>
        /// <param name="idParcela">The idParcela<see cref="int"/>.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        internal static bool EstaAutorizado(int idUsuario, string role, int idParcela) {
            if (role == "admin")
                return true;
            if (role == "asesor") {
                List<int> lAsesor = DB.AsesorParcelaList(idUsuario);
                return lAsesor.Contains(idParcela);
            }
            if (role == "dbo")
                return DB.LaParcelaPerteneceAlRegante(idUsuario, idParcela);
            return false;
        }

        /// <summary>
        /// The FechaSiembra.
        /// </summary>
        /// <param name="idUnidadCultivo">The idUnidadCultivo<see cref="string"/>.</param>
        /// <param name="idTemporada">The idTemporada<see cref="string"/>.</param>
        /// <returns>The <see cref="DateTime?"/>.</returns>
        internal static DateTime? FechaSiembra(string idUnidadCultivo, string idTemporada) {
            Database db = DB.ConexionOptiaqua;
            UnidadCultivoCultivoEtapas reg = new UnidadCultivoCultivoEtapas { IdUnidadCultivo = idUnidadCultivo, IdTemporada = idTemporada, IdEtapaCultivo = 1 };
            UnidadCultivoCultivoEtapas ret = db.SingleOrDefaultById<UnidadCultivoCultivoEtapas>(reg);
            return ret?.FechaInicioEtapa;
        }

        /// <summary>
        /// Crea y almacena contraseña por defecto para regante.
        /// </summary>
        public static void CrearPassw() {
            Database db = DB.ConexionOptiaqua;
            List<Regante> lr = db.Fetch<Regante>("select * from regante");
            foreach (Regante r in lr) {
                if (r.Role != "admin") {
                    string pass1 = BuildPassword(r.NIF, "Pass" + r.IdRegante.ToString());
                    r.Contraseña = pass1;
                    r.Role = "dbo";
                    db.Save(r);
                }
            }
        }

        /// <summary>
        /// Crear contraseña encriptada a partir del nif del regante.
        /// </summary>
        /// <param name="nifRegante">nifRegante<see cref="string"/>.</param>
        /// <param name="password">password<see cref="string"/>.</param>
        /// <returns><see cref="string"/>.</returns>
        public static string BuildPassword(string nifRegante, string password) {
            if (string.IsNullOrEmpty(nifRegante)) {
                nifRegante = "0000000000000";
            }
            string cpass = Encriptacion.XorIt(nifRegante, password) + "0000000000000";
            cpass = cpass.Substring(1, 12); // 12 como máximo            
            string ret = Encriptacion.Encripta(cpass);
            return ret;
        }

        /// <summary>
        /// Crear o actualizar datos de temporada.
        /// </summary>
        /// <param name="temporada">param<see cref="Temporada"/>.</param>
        /// <returns><see cref="object"/>.</returns>
        public static object TemporadaSave(Temporada temporada) {
            Database db = DB.ConexionOptiaqua;
            db.Save(temporada);
            return "OK";
        }

        /// <summary>
        /// The MultimediaSave.
        /// </summary>
        /// <param name="multimedia">The multimedia<see cref="MultimediaPost"/>.</param>
        /// <returns>The <see cref="object"/>.</returns>
        public static object MultimediaSave(MultimediaPost multimedia) {
            Database db = DB.ConexionOptiaqua;
            DateTime? fechaExpira = null;
            if (DateTime.TryParse(multimedia.Expira, out DateTime tempFecha))
                fechaExpira = tempFecha;
            Multimedia m = new Multimedia {
                Autor = multimedia.Autor,
                Descripcion = multimedia.Descripcion,
                Expira = fechaExpira,
                Fecha = DateTime.Parse(multimedia.Fecha),
                IdMultimedia = multimedia.IdMultimedia,
                IdMultimediaTipo = multimedia.IdMultimediaTipo,
                Titulo = multimedia.Titulo,
                Url = multimedia.Url
            };
            db.Save(m);
            return m.IdMultimedia.ToString();
        }

        internal static string AsesorUnidadCultivoSave(int idRegante, List<string> lUnidadesCultivo) {
            Database db = DB.ConexionOptiaqua;
            Regante regante = db.SingleById<Regante>(idRegante);
            if (regante.Role != "asesor")
                return "El regante indicado no tienen role de asesor";
            db.Delete<AsesorUnidadCultivo>("wHERE IDREGANTE=@0", idRegante);
            foreach (string iduc in lUnidadesCultivo) {
                AsesorUnidadCultivo reg = new AsesorUnidadCultivo { IdRegante = idRegante, IdUnidadCultivo = iduc };
                db.Insert(reg);
            }
            return "Eliminada anterior lista de unidades de cultivo, se ha creado una nueva con las " + lUnidadesCultivo.Count + " unidades de cultivo indicadas";
        }

        /// <summary>
        /// The MultimediaDelete.
        /// </summary>
        /// <param name="idMultimedia">The idMultimedia<see cref="int"/>.</param>
        /// <returns>The <see cref="object"/>.</returns>
        public static object MultimediaDelete(int idMultimedia) {
            Database db = DB.ConexionOptiaqua;
            db.DeleteWhere<Multimedia>("IdMultimedia=@0", idMultimedia);
            return "OK";
        }

        /// <summary>
        /// The MultimediaTipoSave.
        /// </summary>
        /// <param name="multimediaTipo">The multimediaTipo<see cref="Multimedia_Tipo"/>.</param>
        /// <returns>The <see cref="object"/>.</returns>
        public static object MultimediaTipoSave(Multimedia_Tipo multimediaTipo) {
            Database db = DB.ConexionOptiaqua;
            db.Save(multimediaTipo);
            return multimediaTipo.IdMultimediaTipo.ToString();
        }

        /// <summary>
        /// The MultimediaTipoDelete.
        /// </summary>
        /// <param name="idMultimediaTipo">The idMultimediaTipo<see cref="int"/>.</param>
        /// <returns>The <see cref="object"/>.</returns>
        public static object MultimediaTipoDelete(int idMultimediaTipo) {
            Database db = DB.ConexionOptiaqua;
            db.DeleteWhere<Multimedia_Tipo>("IdMultimediaTipo=@0", idMultimediaTipo);
            return "OK";
        }

        /// <summary>
        /// GeoLocParcelasList.
        /// </summary>
        /// <param name="idUnidadCultivo">idUnidadCultivo<see cref="string"/>.</param>
        /// <param name="idTemporada">idTemporada<see cref="string"/>.</param>
        /// <returns><see cref="List{GeoLocParcela}"/>.</returns>
        public static List<GeoLocParcela> GeoLocParcelasList(string idUnidadCultivo, string idTemporada) {
            List<GeoLocParcela> ret = new List<GeoLocParcela>();
            Database db = DB.ConexionOptiaqua;
            string sql = "SELECT dbo.Parcela.IdParcelaInt, dbo.Parcela.IdMunicipio, dbo.Parcela.IdPoligono, dbo.Parcela.IdParcela, Parcela.GEO.ToString() AS Geo, dbo.Municipio.Municipio, dbo.Parcela.GID ";
            sql += " FROM dbo.Parcela INNER JOIN dbo.UnidadCultivoParcela ON dbo.Parcela.IdParcelaInt = dbo.UnidadCultivoParcela.IdParcelaInt INNER JOIN ";
            sql += " dbo.Municipio ON dbo.Parcela.IdMunicipio = dbo.Municipio.IdMunicipio ";
            sql += "WHERE dbo.UnidadCultivoParcela.IdUnidadCultivo=@0 AND dbo.UnidadCultivoParcela.IdTemporada=@1 ";
            ret = db.Fetch<GeoLocParcela>(sql, idUnidadCultivo, idTemporada);
            return ret;
        }

        /// <summary>
        /// ParcelasList.
        /// </summary>
        /// <returns><see cref="object"/>.</returns>
        public static object ParcelasList() {
            Database db = DB.ConexionOptiaqua;
            string sql;
            sql = "Select IdParcelaInt, IdRegante, Descripcion, SuperficieM2 from parcela";
            return db.Fetch<object>(sql);
        }

        /// <summary>
        /// UnidadesDeCultivoList
        /// </summary>
        /// <param name="lTemporadas"></param>
        /// <param name="idUsuario"></param>
        /// <param name="role"></param>
        /// <returns></returns>
        public static object UnidadesDeCultivoList(List<string> lTemporadas, int idUsuario, string role) {
            List<UnidadCultivoConSuperficieYGeoLoc> ret = new List<UnidadCultivoConSuperficieYGeoLoc>();
            Database db = DB.ConexionOptiaqua;
            foreach (string idTemporada in lTemporadas) {
                string sql = "Select DISTINCT IdUnidadCultivo, IdParcelaInt, IdRegante from UnidadCultivoParcela where IdTemporada=@0 ";
                List<UnidadCultivoConSuperficieYGeoLoc> deUnaTemporada = db.Fetch<UnidadCultivoConSuperficieYGeoLoc>(sql, idTemporada);
                foreach (UnidadCultivoConSuperficieYGeoLoc item in deUnaTemporada) {
                    if (!DB.EstaAutorizado(idUsuario, role, item.IdUnidadCultivo))
                        continue;
                    item.Hidrantes = DB.HidrantesListJson(item.IdUnidadCultivo, idTemporada);
                    item.SuperficieM2 = UnidadCultivoExtensionM2(item.IdUnidadCultivo, idTemporada);
                    List<GeoLocParcela> lGeoLocParcelas = DB.GeoLocParcelasList(item.IdUnidadCultivo, idTemporada);
                    item.GeoLocJson = Newtonsoft.Json.JsonConvert.SerializeObject(lGeoLocParcelas);
                    ret.Add(item);
                }
            }
            return ret;
        }

        /// <summary>
        /// Devuele el registro TipoEstres inficado por su identificador.
        /// </summary>
        /// <param name="idTipoEstres">The idTipoEstres<see cref="string"/>.</param>
        /// <returns>The <see cref="TipoEstres"/>.</returns>
        internal static TipoEstres TipoEstres(string idTipoEstres) {
            Database db = DB.ConexionOptiaqua;
            TipoEstres ret = db.SingleById<TipoEstres>(idTipoEstres);
            return ret;
        }

        /// <summary>
        /// UnidadCultivoTemporadaCosteM3Agua.
        /// </summary>
        /// <param name="idUnidadCultivo">idUnidadCultivo<see cref="string"/>.</param>
        /// <param name="idTemporada">idTemporada<see cref="string"/>.</param>
        /// <returns><see cref="double?"/>.</returns>
        public static double UnidadCultivoTemporadaCosteM3Agua(string idUnidadCultivo, string idTemporada) {
            if (string.IsNullOrEmpty(idTemporada) || string.IsNullOrWhiteSpace(idUnidadCultivo))
                return 0;
            Database db = DB.ConexionOptiaqua;
            string sql = "Select CosteM3Agua from UnidadCultivoCultivo where idUnidadCultivo=@0  and IdTemporada=@1;";
            double? ret = db.SingleOrDefault<double?>(sql, idUnidadCultivo, idTemporada);
            if (ret == null) {
                sql = "Select CosteM3Agua from Temporada where IdTemporada=@0;";
                ret = db.FirstOrDefault<double?>(sql, idTemporada);
            }
            return ret ?? 0;
        }

        /// <summary>
        /// The ListaTipoEstres.
        /// </summary>
        /// <returns>The <see cref="Dictionary{string, TipoEstres}"/>.</returns>
        internal static Dictionary<string, TipoEstres> ListaTipoEstres() {
            Database db = DB.ConexionOptiaqua;
            List<TipoEstres> ret = db.Fetch<TipoEstres>();
            return ret.ToDictionary(x => x.IdTipoEstres);
        }

        /// <summary>
        /// The ListaEstresUmbral.
        /// </summary>
        /// <returns>The <see cref="Dictionary{string, List{TipoEstresUmbral}}"/>.</returns>
        internal static Dictionary<string, List<TipoEstresUmbral>> ListaEstresUmbral() {
            Database db = DB.ConexionOptiaqua;
            List<TipoEstresUmbral> lUmbrales = db.Fetch<TipoEstresUmbral>();
            Dictionary<string, List<TipoEstresUmbral>> ret = new Dictionary<string, List<TipoEstresUmbral>>();
            foreach (TipoEstresUmbral umbral in lUmbrales) {
                if (!ret.Keys.Contains(umbral.IdTipoEstres))
                    ret.Add(umbral.IdTipoEstres, new List<TipoEstresUmbral>());
                ret[umbral.IdTipoEstres].Add(umbral);
            }
            return ret;
        }

        /// <summary>
        /// UnidadCultivoTemporadaCosteM3AguaSave.
        /// </summary>
        /// <param name="param">param<see cref="ParamPostCosteM3Agua"/>.</param>
        /// <returns><see cref="object"/>.</returns>
        public static object UnidadCultivoTemporadaCosteM3AguaSave(ParamPostCosteM3Agua param) {
            if (string.IsNullOrWhiteSpace(param.IdUnidadCultivo) || string.IsNullOrWhiteSpace(param.IdTemporada))
                return "Error en actualización";
            Database db = DB.ConexionOptiaqua;
            if (param.CosteM3Agua <= 0)
                param.CosteM3Agua = null;
            UnidadCultivoCultivo ucc = db.SingleOrDefault<UnidadCultivoCultivo>("where IdUnidadCultivo=@0 AND IdTemporada=@1", param.IdUnidadCultivo, param.IdTemporada);
            if (ucc != null) {
                ucc.CosteM3Agua = param.CosteM3Agua;
            }
            db.Save(ucc);
            return "OK";
        }

        /// <summary>
        /// ParcelasList.
        /// </summary>
        /// <param name="idUnidadCultivo">idUnidadCultivo<see cref="string"/>.</param>
        /// <param name="idTemporada">idTemporada<see cref="string"/>.</param>
        /// <returns><see cref="object"/>.</returns>
        public static object IdParcelasList(string idUnidadCultivo, string idTemporada) {
            Database db = DB.ConexionOptiaqua;
            string sql = "SELECT DISTINCT dbo.Parcela.IdParcelaInt, dbo.Parcela.Descripcion, dbo.UnidadCultivoParcela.IdRegante, dbo.Parcela.SuperficieM2, dbo.UnidadCultivoParcela.IdUnidadCultivo ";
            sql += " FROM dbo.UnidadCultivoParcela INNER JOIN ";
            sql += " dbo.Parcela ON dbo.UnidadCultivoParcela.IdParcelaInt = dbo.Parcela.IdParcelaInt";
            sql += " WHERE dbo.UnidadCultivoParcela.IdUnidadCultivo = @0 AND dbo.UnidadCultivoParcela.IdTemporada=@1";
            return db.Fetch<object>(sql, idUnidadCultivo, idTemporada);
        }

        /// <summary>
        /// DatosRiegosList.
        /// </summary>
        /// <param name="idUnidadCultivo">idUnidadCultivo<see cref="string"/>.</param>
        /// <param name="idTemporada">idTemporada<see cref="string"/>.</param>
        /// <returns><see cref="object"/>.</returns>
        public static object DatosRiegosList(string idUnidadCultivo, string idTemporada) {
            List<DatosRiego> redDatosRiegos = new List<DatosRiego>();
            Temporada t = Temporada(idTemporada);
            UnidadCultivo uc = UnidadCultivo(idUnidadCultivo);
            UnidadCultivoCultivo ucc = UnidadCultivoCultivo(idUnidadCultivo, idTemporada);

            DateTime desdeFecha = ucc?.FechaSiembra() ?? t.FechaInicial;
            DateTime hastaFecha = t.FechaFinal < DateTime.Today ? DateTime.Today : t.FechaFinal;


            List<Riego> lRiegos = RiegosList(idUnidadCultivo, desdeFecha, hastaFecha);

            double superficie = DB.UnidadCultivoExtensionM2(idUnidadCultivo, idTemporada) / 1000;
            if (superficie == 0)
                superficie = double.MinValue;

            foreach (Riego r in lRiegos)
                redDatosRiegos.Add(new DatosRiego {
                    Fecha = r.Fecha,
                    M3 = r.RiegoM3,
                    Mm = (r.RiegoM3) / superficie,
                    Obtencion = "S",
                    IdTemporada = t.IdTemporada,
                    IdUnidadCultivo = idUnidadCultivo,
                    UnidadCultivo = uc?.Alias ?? ""
                });

            List<UnidadCultivoDatosExtra> lExtra = DatosExtraList(idUnidadCultivo);
            foreach (UnidadCultivoDatosExtra extra in lExtra)
                if (extra.Fecha >= desdeFecha && extra.Fecha <= hastaFecha && (extra.RiegoM3 ?? 0) > 0) {
                    DatosRiego find = redDatosRiegos.Find(f => f.Fecha == extra.Fecha);
                    if (find != null)
                        redDatosRiegos.Remove(find);
                    redDatosRiegos.Add(new DatosRiego {
                        Fecha = extra.Fecha,
                        M3 = extra.RiegoM3 ?? 0,
                        Mm = (extra.RiegoM3 ?? 0) / superficie,
                        Obtencion = "A",
                        IdTemporada = t.IdTemporada,
                        IdUnidadCultivo = idUnidadCultivo,
                        UnidadCultivo = uc.Alias
                    });
                }
            List<DatosLLuviaORiego> ret = new List<DatosLLuviaORiego>();
            foreach (DatosRiego rie in redDatosRiegos) {
                DatosLLuviaORiego dat = new DatosLLuviaORiego {
                    IdTipoAportacion = "Riego",
                    Fecha = rie.Fecha,
                    IdTemporada = rie.IdTemporada,
                    IdUnidadCultivo = rie.IdUnidadCultivo,
                    Mm = rie.Mm,
                    M3 = rie.M3,
                    Obtencion = rie.Obtencion,
                    UnidadCultivo = rie.IdUnidadCultivo
                };
                ret.Add(dat);
            }
            return ret;
        }

        /// <summary>
        /// TipoEstresUmbralList.
        /// </summary>
        /// <param name="idTipoEstres">idTipoEstres<see cref="string"/>.</param>
        /// <returns><see cref="List{TipoEstresUmbral}"/>.</returns>
        internal static List<TipoEstresUmbral> TipoEstresUmbralOrderList(string idTipoEstres) {
            Database db = DB.ConexionOptiaqua;
            string sql = $"SELECT * FROM TipoEstresUmbral Where IdTipoEstres='{idTipoEstres}' order by umbralMaximo";
            List<TipoEstresUmbral> ret = db.Fetch<TipoEstresUmbral>(sql);
            return ret;
        }


        public static void RiegosNebulaListRefresh(DateTime? ultimaActualizacion) {
            Database dbNebula = DB.ConexionNebula;
            if (ultimaActualizacion == null)
                ultimaActualizacion = new DateTime(1995, 1, 1);
            //string sql = $"SELECT IdUnidadCultivo, RiegoM3, Fecha from riegos where idUnidadCultivo='{idUnidadCultivo}' AND  fecha >=@0 and fecha<=@1";
            List<Riego> lRiegos = dbNebula.Fetch<Riego>("SELECT * FROM RIEGOS WHERE FECHA>=@0 ORDER BY FECHA", ((DateTime)ultimaActualizacion).AddDays(-5));
            Database db = DB.ConexionOptiaqua;
            int nDel = db.Delete<Riego>("WHERE FECHA>=@0", ((DateTime)ultimaActualizacion).AddDays(-5));
            db.InsertBulk(lRiegos);
            //lRiegos.ForEach(x => db.Save(x));
        }


        public static List<string> HidrantesList_SIN_USO(string IdUnidadCultivo, string idTemporada) {
            List<int> lParcelas = DB.ParcelasDeUnidadCultivo(IdUnidadCultivo, idTemporada);
            string strList = "(" + string.Join(",", lParcelas) + ")";
            Database dbNebula = DB.ConexionNebula;
            string sql = "SELECT ID_HYDRANT FROM V_VALVULA_HIDRANTE_PARCELA_Overline WHERE idPlotGAdmin in " + strList;
            List<string> ret = dbNebula.Fetch<string>(sql);
            return ret;
        }

        public static string HidrantesListJson(string IdUnidadCultivo, string idTemporada) {
            List<int> lParcelas = DB.ParcelasDeUnidadCultivo(IdUnidadCultivo, idTemporada);
            if (lParcelas.Count == 0) {
                //string nada = Newtonsoft.Json.JsonConvert.SerializeObject("");
                return "[]";
            }
            string strList = "(" + string.Join(",", lParcelas) + ")";
            Database db = DB.ConexionNebula;
            string sql = "SELECT DISTINCT HIDRANTE as H,VALVULA as V FROM V_VALVULA_HIDRANTE_PARCELA_Overline WHERE idPlotGAdmin in " + strList;
            List<Dictionary<string, object>> lDat = db.Fetch<Dictionary<string, object>>(sql).Distinct().ToList();
            string ret = Newtonsoft.Json.JsonConvert.SerializeObject(lDat);
            return ret;
        }

        /// <summary>
        /// DatosLluviaList.
        /// </summary>
        /// <param name="idUnidadCultivo">idUnidadCultivo<see cref="string"/>.</param>
        /// <param name="idTemporada">idTemporada<see cref="string"/>.</param>
        /// <returns><see cref="object"/>.</returns>
        public static object DatosLluviaList(string idUnidadCultivo, string idTemporada) {
            List<DatosLLuvia> retDatosLluvia = new List<DatosLLuvia>();
            Temporada temporada = Temporada(idTemporada);
            if (temporada == null) {
                temporada = Temporada(DB.TemporadaActiva());
            }

            var idEstacion = DB.EstacionDeUC(idUnidadCultivo, idTemporada);
            UnidadCultivo uc = UnidadCultivo(idUnidadCultivo);
            UnidadCultivoCultivo ucc = UnidadCultivoCultivo(idUnidadCultivo, idTemporada);

            //string estacion = Estacion(idEstacion).Nombre;

            DateTime desdeFecha = ucc.FechaSiembra() ?? temporada.FechaInicial;
            DateTime hastaFecha = temporada.FechaFinal < DateTime.Today ? DateTime.Today : temporada.FechaFinal;

            List<DatoClimatico> lLluvia = DatosClimaticosList(desdeFecha, hastaFecha, idEstacion);

            foreach (DatoClimatico ll in lLluvia)
                retDatosLluvia.Add(new DatosLLuvia {
                    Fecha = ll.Fecha,
                    Mm = ll.Precipitacion,
                    Obtencion = "S",
                    IdEstacion = ll.IdEstacion,
                    IdTemporada = temporada.IdTemporada,
                    IdUnidadCultivo = uc.IdUnidadCultivo,
                    UnidadCultivo = uc.Alias,
                });

            List<UnidadCultivoDatosExtra> lExtra = DatosExtraList(idUnidadCultivo);
            foreach (UnidadCultivoDatosExtra extra in lExtra)
                if (extra.Fecha >= desdeFecha && extra.Fecha <= hastaFecha && (extra.LluviaMm ?? 0) > 0) {
                    DatosLLuvia find = retDatosLluvia.Find(f => f.Fecha == extra.Fecha);
                    if (find != null)
                        retDatosLluvia.Remove(find);
                    retDatosLluvia.Add(new DatosLLuvia {
                        Fecha = extra.Fecha,
                        Mm = extra.LluviaMm ?? 0,
                        Obtencion = "A",
                        IdEstacion = idEstacion,
                        IdTemporada = temporada.IdTemporada,
                        IdUnidadCultivo = uc.IdUnidadCultivo,
                        UnidadCultivo = uc.Alias
                    });
                }
            List<DatosLLuviaORiego> ret = new List<DatosLLuviaORiego>();
            foreach (DatosLLuvia llu in retDatosLluvia) {
                DatosLLuviaORiego dat = new DatosLLuviaORiego {
                    IdTipoAportacion = "Lluvia",
                    Fecha = llu.Fecha,
                    IdEstacion = llu.IdEstacion,
                    IdTemporada = llu.IdTemporada,
                    IdUnidadCultivo = llu.IdUnidadCultivo,
                    Mm = llu.Mm,
                    Obtencion = llu.Obtencion,
                    UnidadCultivo = llu.IdUnidadCultivo
                };
                ret.Add(dat);
            }
            return ret;
        }

        /// <summary>
        /// ParcelaList.
        /// </summary>
        /// <param name="IdTemporada">IdTemporada<see cref="string"/>.</param>
        /// <param name="IdParcela">IdParcela<see cref="string"/>.</param>
        /// <param name="IdRegante">IdRegante<see cref="string"/>.</param>
        /// <param name="IdMunicipio">IdMunicipio<see cref="string"/>.</param>
        /// <param name="Search">Search<see cref="string"/>.</param>
        /// <returns><see cref="object"/>.</returns>
        public static object ParcelaList(string IdTemporada, string IdParcela, string IdRegante, string IdMunicipio, string Search) {
            Database db = DB.ConexionOptiaqua;
            IdTemporada = IdTemporada.Quoted();
            Search = Search.Quoted();
            string sql = $"SELECT * FROM ParcelaList({IdTemporada},{IdParcela},{IdRegante},{IdMunicipio},{Search})";
            return db.Fetch<object>(sql);
        }

        /// <summary>
        /// ReganteList.
        /// </summary>
        /// <param name="strFecha">strFecha<see cref="string"/>.</param>
        /// <param name="IdRegante">IdRegante<see cref="string"/>.</param>
        /// <param name="IdUnidadCultivo">IdUnidadCultivo<see cref="string"/>.</param>
        /// <param name="IdParcela">IdParcela<see cref="string"/>.</param>
        /// <param name="Search">Search<see cref="string"/>.</param>
        /// <returns><see cref="object"/>.</returns>
        public static object ReganteList(string strFecha, string IdRegante, string IdUnidadCultivo, string IdParcela, string Search) {
            Database db = DB.ConexionOptiaqua;
            string idTemporada = DB.TemporadaDeFecha(IdUnidadCultivo, DateTime.Parse(strFecha));
            if (idTemporada == null)
                idTemporada = TemporadaActiva();
            IdUnidadCultivo = IdUnidadCultivo.Quoted();
            Search = Search.Quoted();
            string sql = $"SELECT * FROM ReganteList('{idTemporada}',{IdRegante},{IdUnidadCultivo},{IdParcela},{Search})";
            return db.Fetch<object>(sql);
        }

        /// <summary>
        /// PluviometriaSave.
        /// </summary>
        /// <param name="idTemporada">idTemporada<see cref="string"/>.</param>
        /// <param name="idUnidadCultivo">idUnidadCultivo<see cref="string"/>.</param>
        /// <param name="pluviometria">pluviometria<see cref="double"/>.</param>
        public static void PluviometriaSave(string idTemporada, string idUnidadCultivo, double pluviometria) {
            Database db = DB.ConexionOptiaqua;
            UnidadCultivoCultivo unidadCultivoCultivo = db.Single<UnidadCultivoCultivo>("SELECT * FROM UnidadCultivoCultivo WHERE IdTemporada=@0 and idUnidadCultivo=@1");
            unidadCultivoCultivo.Pluviometria = pluviometria;
            db.Save(unidadCultivoCultivo);
        }

        /// <summary>
        /// Temporada.
        /// </summary>
        /// <param name="idTemporada">idTemporada<see cref="string"/>.</param>
        /// <returns><see cref="Temporada"/>.</returns>
        public static Temporada Temporada(string idTemporada) {
            if (string.IsNullOrWhiteSpace(idTemporada))
                return null;
            Database db = DB.ConexionOptiaqua;
            return db.SingleOrDefaultById<Temporada>(idTemporada);
        }

        /// <summary>
        /// UnidadCultivoList.
        /// </summary>
        /// <param name="idTemporada">IdTemporada<see cref="string"/>.</param>
        /// <param name="idUnidadCultivo">IdUnidadCultivo<see cref="string"/>.</param>
        /// <param name="idRegante">IdRegante<see cref="string"/>.</param>
        /// <param name="idCultivo">IdCultivo<see cref="string"/>.</param>
        /// <param name="idMunicipio">idMunicipio<see cref="string"/>.</param>
        /// <param name="idTipoRiego">IdTipoRiego<see cref="string"/>.</param>
        /// <param name="idEstacion">IdEstacion<see cref="string"/>.</param>
        /// <param name="idPoligono"></param>
        /// <param name="idParcela"></param>
        /// <param name="search">Search<see cref="string"/>.</param>
        /// <param name="idUsuario">.</param>
        /// <param name="role">.</param>
        /// <returns><see cref="object"/>.</returns>
        public static object UnidadCultivoList(string idTemporada, string idUnidadCultivo, string idRegante, string idCultivo, string idMunicipio, string idTipoRiego, string idPoligono, string idParcela, string search, int idUsuario, string role) {
            idTemporada = idTemporada.Unquoted();
            Database db = DB.ConexionOptiaqua;
            if (string.IsNullOrWhiteSpace(idTemporada)) {
                idTemporada = DB.TemporadaActiva() ?? "";
            }

            idTemporada = idTemporada.Quoted();
            idUnidadCultivo = idUnidadCultivo.Quoted();
            search = search.Quoted();
            string sql = $"SELECT * FROM UnidadcultivoList({idTemporada.Quoted()},{idUnidadCultivo},{idRegante},{idCultivo},{idMunicipio},{idTipoRiego},{search})";

            idTemporada = idTemporada.Unquoted();
            idPoligono = idPoligono.Unquoted();
            idParcela = idParcela.Unquoted();
            List<Dictionary<string, object>> lRet = db.Fetch<Dictionary<string, object>>(sql);
            List<Dictionary<string, object>> lValidos = new List<Dictionary<string, object>>();
            List<string> lAsesor = new List<string>();
            if (role == "asesor")
                lAsesor = DB.AsesorUnidadCultivoList(idUsuario);
            foreach (Dictionary<string, object> dic in lRet) {
                string idUC = dic["IdUnidadCultivo"] as string;
                //Console.WriteLine(idUC);
                //  if (idUC=="2744_R1")
                //      Debug.WriteLine(idUC);
                if (role == "asesor") {
                    if (!lAsesor.Contains(idUC))
                        continue;

                } else if (role == "dbo") {
                    if (DB.LaUnidadDeCultivoPerteneceAlReganteEnLaTemporada(idUC, idUsuario, idTemporada) == false)
                        continue;
                }
                DB.DatosParcelasList(idUC, idTemporada, out string poligonos, out string parcelas, out string refCastrales);
                if (!string.IsNullOrWhiteSpace(idPoligono)) {
                    if (!poligonos.Split('#').Contains(idPoligono))
                        continue;
                }
                if (!string.IsNullOrWhiteSpace(idParcela)) {
                    if (!parcelas.Split('#').Contains(idParcela))
                        continue;
                }
                dic.Add("SuperficieM2", UnidadCultivoExtensionM2(idUC, idTemporada));
                dic.Add("FechaSiembra", DB.FechaSiembra(idUC, idTemporada));
                dic.Add("IdParcelas:", parcelas);
                dic.Add("IdPoligonos:", poligonos);
                dic.Add("HidranteTomaJson", DB.HidrantesListJson(idUC, idTemporada));
                lValidos.Add(dic);
                List<GeoLocParcela> lGeoLocParcelas = DB.GeoLocParcelasList(idUC, idTemporada);
                string geo = Newtonsoft.Json.JsonConvert.SerializeObject(lGeoLocParcelas);
                dic.Add("GeoLocJson", geo);
            }
            return lValidos;
        }

        /// <summary>
        /// TemporadaActiva (si no hay ninguna marcada como activa devuelve la que tiene la fecha final mas alta).
        /// </summary>
        /// <returns><see cref="string"/>.</returns>
        public static string TemporadaActiva() {
            Database db = DB.ConexionOptiaqua;
            string ret = db.Fetch<string>("SELECT IdTemporada from temporada WHERE ACTIVA=1")[0];
            if (ret == null) {
                string sql = " SELECT IdTemporada FROM dbo.Temporada WHERE(FechaFinal = (SELECT TOP(1) MAX(FechaFinal) AS Expr1 FROM dbo.Temporada AS Temporada_1))";
                ret = db.Fetch<string>(sql)[0];
            }
            return ret;
        }

        /// <summary>
        /// UnidadCultivoDatosAmpliados.
        /// </summary>
        /// <param name="fecha"></param>
        /// <param name="idUnidadCultivoFiltro"></param>        
        /// <returns><see cref="object"/>.</returns>
        public static List<UnidadCultivoDatosAmpliados> UnidadCultivoDatosAmpliados(DateTime fecha, string idUnidadCultivoFiltro) {
            Database db = DB.ConexionOptiaqua;
            string filtro = "";
            if (!string.IsNullOrWhiteSpace(idUnidadCultivoFiltro))
                filtro += $" WHERE IdUnidadCultivo='{idUnidadCultivoFiltro}' ";

            string sql = $"SELECT * FROM UnidadCultivoDatosAmpliados " + filtro;
            List<UnidadCultivoDatosAmpliados> ret = db.Fetch<UnidadCultivoDatosAmpliados>(sql);
            foreach (UnidadCultivoDatosAmpliados dat in ret) {
                string idTemporada = DB.TemporadaDeFecha(dat.IdUnidadCultivo, fecha);
                if (string.IsNullOrWhiteSpace(idTemporada) || idTemporada != dat.IdTemporada) {
                    dat.IdTemporada = null;
                    continue;
                }
                ObtenerMunicicioParaje(idTemporada, dat.IdUnidadCultivo, out string provincias, out string municipios, out string parajes);
                dat.Provincia = provincias;
                dat.Municipio = municipios;
                dat.Paraje = parajes;
                dat.FechaSiembra = DB.FechaSiembra(dat.IdUnidadCultivo, idTemporada);
                dat.Hidrantes = DB.HidrantesListJson(dat.IdUnidadCultivo, idTemporada);
                DatosParcelasList(dat.IdUnidadCultivo, idTemporada, out string poligonos, out string parcelas, out string refCatastrales);
                dat.Parcelas = parcelas;
                dat.Poligonos = poligonos;
                dat.RefCatastrales = refCatastrales;
            }
            ret.RemoveAll(x => x.IdTemporada == null);
            return ret;
        }

        public class RefCatasPolPar {
            public string RefCatastral { get; set; }
            public string IdPoligono { get; set; }
            public string IdParcela { get; set; }
        }

        private static void DatosParcelasList(string idUnidadCultivo, string idTemporada, out string poligonos, out string parcelas, out string refCatastrales) {
            Database db = DB.ConexionOptiaqua;
            string sql = " SELECT Parcela.RefCatastral, Parcela.IdPoligono, Parcela.IdParcela FROM Parcela";
            sql += " INNER JOIN UnidadCultivoParcela ON Parcela.IdParcelaInt = UnidadCultivoParcela.IdParcelaInt ";
            sql += " WHERE(UnidadCultivoParcela.IdUnidadCultivo = @0) AND (UnidadCultivoParcela.IdTemporada = @1)";
            List<RefCatasPolPar> lRefCatasPolPar = db.Fetch<RefCatasPolPar>(sql, idUnidadCultivo, idTemporada);
            poligonos = string.Join("#", lRefCatasPolPar.Select(x => x.IdPoligono.Trim()).Distinct());
            parcelas = string.Join("#", lRefCatasPolPar.Select(x => x.IdParcela.Trim()).Distinct());
            refCatastrales = string.Join("#", lRefCatasPolPar.Where(x => x.RefCatastral != null).Select(x => x.RefCatastral?.Trim()).Distinct());
        }



        /// <summary>
        /// Regante.
        /// </summary>
        /// <param name="idRegante">idRegante<see cref="int?"/>.</param>
        /// <returns><see cref="object"/>.</returns>
        public static object Regante(int? idRegante) {
            if (idRegante == null)
                return null;
            Database db = DB.ConexionOptiaqua;
            return db.SingleById<Regante>(idRegante);
        }

        /// <summary>
        /// ReganteUpdate.
        /// Retorna la clave del cliente
        /// Si idRegane =-1 se creará nuevo
        /// </summary>
        /// <param name="rp">rp<see cref="RegantePost"/>.</param>
        public static string ReganteUpdate(RegantePost rp) {
            Database db = DB.ConexionOptiaqua;
            Regante regante = new Regante {
                NIF = rp.NIF,
                IdRegante = rp.IdRegante,
                IdGadmin = rp.IdGadmin,
                Nombre = rp.Nombre,
                Direccion = rp.Direccion,
                CodigoPostal = rp.CodigoPostal,
                Poblacion = rp.Poblacion,
                Provincia = rp.Provincia,
                Pais = rp.Pais,
                Telefono = rp.Telefono,
                TelefonoSMS = rp.TelefonoSMS,
                Email = rp.Email,
                Role = rp.Role,
                WebActive = true
            };
            regante.Contraseña = BuildPassword(regante.NIF, "Pass" + regante.IdRegante.ToString());
            if (regante.Role != "dbo" && regante.Role != "admin" && regante.Role != "asesor") {
                return "Error. El role puede ser:dbo,admin,asesor";
            }
            db.Save(regante);
            return "Pass" + regante.IdRegante.ToString();
        }

        /// <summary>
        /// RiegoTipo.
        /// </summary>
        /// <param name="idTipoRiego">idTipoRiego<see cref="int?"/>.</param>
        /// <returns><see cref="RiegoTipo"/>.</returns>
        public static RiegoTipo RiegoTipo(int? idTipoRiego) {
            if (idTipoRiego == null)
                return null;
            Database db = DB.ConexionOptiaqua;
            return db.SingleById<RiegoTipo>(idTipoRiego);
        }

        /// <summary>
        /// LaUnidadDeCultivoPerteneceAlRegante.
        /// </summary>
        /// <param name="IdUnidadCultivo">IdUnidadCultivo<see cref="string"/>.</param>
        /// <param name="idRegante">idRegante<see cref="int"/>.</param>
        /// <param name="IdTemporada">IdTemporada<see cref="string"/>.</param>
        /// <returns><see cref="bool"/>.</returns>
        public static bool LaUnidadDeCultivoPerteneceAlRegante(string IdUnidadCultivo, int idRegante, string IdTemporada) {
            Database db = DB.ConexionOptiaqua;
            string sql = "Select IdRegante from UnidadCultivoParcela Where IdUnidadCultivo=@0 and IdTemporada=@1 and idRegante=@2";
            List<object> lu = db.Fetch<object>(sql, IdUnidadCultivo, IdTemporada, idRegante);
            return lu.Count != 0;
        }

        /// <summary>
        /// RegantesList.
        /// </summary>
        /// <returns><see cref="object"/>.</returns>
        public static object RegantesList() {
            Database db = DB.ConexionOptiaqua;
            string sql = "Select IdRegante, Nombre, Telefono, TelefonoSMS, Email from Regante";
            return db.Fetch<object>(sql);
        }

        /// <summary>
        /// The AsesorUnidadCultivoList.
        /// </summary>
        /// <param name="idUsuario">The idUsuario<see cref="int"/>.</param>
        /// <returns>The <see cref="List{string}"/>.</returns>
        internal static List<string> AsesorUnidadCultivoList(int idUsuario) {
            Database db = DB.ConexionOptiaqua;
            return db.Fetch<string>("select IdUnidadCultivo from AsesorUnidadCultivo where idRegante=@0", idUsuario);
        }

        internal static List<int> AsesorParcelaList(int idUsuario) {
            Database db = DB.ConexionOptiaqua;
            return db.Fetch<int>("select IdParcelaInt from AsesorParcelas where IdAsesor=@0 ", idUsuario);
        }

        /// <summary>
        /// LaUnidadDeCultivoPerteneceAlRegante.
        /// </summary>
        /// <param name="idUnidadCultivo">idUnidadCultivo<see cref="string"/>.</param>
        /// <param name="idRegante">idRegante<see cref="int"/>.</param>
        /// <returns><see cref="bool"/>.</returns>
        public static bool LaUnidadDeCultivoPerteneceAlRegante(string idUnidadCultivo, int idRegante) {
            Database db = DB.ConexionOptiaqua;
            UnidadCultivo unidadCultivo = db.SingleOrDefaultById<UnidadCultivo>(idUnidadCultivo);
            if (unidadCultivo == null)
                return false;
            return unidadCultivo.IdRegante == idRegante;
        }

        /// <summary>
        /// PasswordSave.
        /// </summary>
        /// <param name="login">login<see cref="LoginRequest"/>.</param>
        /// <returns><see cref="bool"/>.</returns>
        public static bool PasswordSave(LoginRequest login) {
            Database db = DB.ConexionOptiaqua;
            Regante regante = db.SingleOrDefault<Regante>("Where NIF=@0", login.NifRegante);
            regante.Contraseña = BuildPassword(login.NifRegante, login.Password);
            db.Save(regante);
            return true;
        }

        /// <summary>
        /// LaUnidadDeCultivoPerteneceAlReganteEnLaTemporada.
        /// </summary>
        /// <param name="idUnidadCultivo">idUnidadCultivo<see cref="string"/>.</param>
        /// <param name="idRegante">idRegante<see cref="int"/>.</param>
        /// <param name="idTemporada">idTemporada<see cref="string"/>.</param>
        /// <returns><see cref="bool"/>.</returns>
        public static bool LaUnidadDeCultivoPerteneceAlReganteEnLaTemporada(string idUnidadCultivo, int idRegante, string idTemporada) {
            Database db = DB.ConexionOptiaqua;
            string sql = "Select IdUnidadCultivo from UnidadCultivoCultivo where IdUnidadCultivo=@0 and IdRegante=@1 and idTemporada=@2";
            string unidadCultivo = db.SingleOrDefault<string>(sql, idUnidadCultivo, idRegante, idTemporada);
            return unidadCultivo != null;
        }

        /// <summary>
        /// The LaParcelaPerteneceAlRegante.
        /// </summary>
        /// <param name="idParcela">The idParcela<see cref="int"/>.</param>
        /// <param name="idUsuario">The idUsuario<see cref="int"/>.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        public static bool LaParcelaPerteneceAlRegante(int idParcela, int idUsuario) {
            Database db = DB.ConexionOptiaqua;
            string sql = "select idParcelaInt from Parcela where IdParcela=@0 and IdRegante=@1";
            bool pertenece = db.SingleOrDefault<int?>(sql, idParcela, idUsuario) != null;
            if (pertenece)
                return true;
            sql = "select idParcelaInt from UnidadCultivoParcela where IdParcelaInt=@0 and IdRegante=@1";
            pertenece = db.SingleOrDefault<int?>(sql, idParcela, idUsuario) != null;
            return pertenece;
        }

        /// <summary>
        /// The ListaUnidadesCultivoQueCumplenFiltro.
        /// </summary>
        /// <param name="idMunicipio">The idMunicipio<see cref="int?"/>.</param>
        /// <param name="idCultivo">The idCultivo<see cref="string"/>.</param>
        /// <param name="idRegante">The idRegante<see cref="int?"/>.</param>
        /// <returns>The <see cref="List{string}"/>.</returns>
        internal static List<string> ListaUnidadesCultivoQueCumplenFiltro(int? idMunicipio, string idCultivo, int? idRegante) {
            Database db = DB.ConexionOptiaqua;
            string sql = "SELECT Distinct IdUnidadCultivo from FiltroParcelasDatosHidricos ";
            string filtro = " Where ";
            if (idMunicipio != null) {
                sql += filtro + "idMunicipio=" + idMunicipio.ToString();
                filtro = " and ";
            }

            if (idCultivo.Unquoted() != "") {
                sql += filtro + " IdCultivo=" + idCultivo;
                filtro = " and ";
            }

            if (idRegante != null)
                sql += filtro + "IdRegante=" + idRegante.ToString();

            return db.Fetch<string>(sql);
        }

        /// <summary>
        /// TemporadasList.
        /// </summary>
        /// <returns><see cref="List{Temporada}"/>.</returns>
        public static List<Temporada> TemporadasList() {
            Database db = DB.ConexionOptiaqua;
            string sql = "Select * from Temporada;";
            return db.Fetch<Temporada>(sql);
        }

        /// <summary>
        /// ParajesList.
        /// </summary>
        /// <returns><see cref="object"/>.</returns>
        public static object ParajesList() {
            Database db = DB.ConexionOptiaqua;
            string sql = "Select * from ParajeAmpliado;";
            return db.Fetch<object>(sql);
        }

        /// <summary>
        /// ProvinciaList.
        /// </summary>
        /// <returns><see cref="object"/>.</returns>
        public static object ProvinciaList() {
            Database db = DB.ConexionOptiaqua;
            string sql = "Select * from Provincia;";
            return db.Fetch<object>(sql);
        }

        /// <summary>
        /// CultivosList.
        /// </summary>
        /// <returns><see cref="object"/>.</returns>
        public static object CultivosList() {
            Database db = DB.ConexionOptiaqua;
            string sql = "Select * from Cultivo;";
            return db.Fetch<object>(sql);
        }

        /// <summary>
        /// MunicipiosList.
        /// </summary>
        /// <returns><see cref="object"/>.</returns>
        public static object MunicipiosList() {
            Database db = DB.ConexionOptiaqua;
            string sql = "SELECT dbo.Municipio.IdMunicipio, dbo.Municipio.Municipio, dbo.Provincia.IdProvincia, dbo.Provincia.Provincia FROM dbo.Municipio INNER JOIN  dbo.Provincia ON dbo.Municipio.IdProvincia = dbo.Provincia.IdProvincia";
            return db.Fetch<object>(sql);
        }

        /// <summary>
        /// DatosExtraList.
        /// </summary>
        /// <param name="idUnidadCultivo">idUnidadCultivo<see cref="string"/>.</param>
        /// <returns><see cref="List{UnidadCultivoDatosExtra}"/>.</returns>
        public static List<UnidadCultivoDatosExtra> DatosExtraList(string idUnidadCultivo) {
            Database db = DB.ConexionOptiaqua;
            string sql;
            sql = "Select * from  UnidadCultivoDatosExtra where IdUnidadCultivo=@0";
            List<UnidadCultivoDatosExtra> ret = db.Fetch<UnidadCultivoDatosExtra>(sql, idUnidadCultivo);
            return ret;
        }

        /// <summary>
        /// DatosExtraList.
        /// </summary>
        /// <param name="idUnidadCultivo">idUnidadCultivo<see cref="string"/>.</param>
        /// <param name="fecha">fecha<see cref="DateTime"/>.</param>
        /// <returns><see cref="List{UnidadCultivoDatosExtra}"/>.</returns>
        public static List<UnidadCultivoDatosExtra> DatosExtraList(string idUnidadCultivo, DateTime fecha) {
            Database db = DB.ConexionOptiaqua;
            string sql;
            sql = "Select * from  UnidadCultivoDatosExtra where IdUnidadCultivo=@0 and fecha=@1";
            List<UnidadCultivoDatosExtra> ret = db.Fetch<UnidadCultivoDatosExtra>(sql, idUnidadCultivo, fecha);
            return ret;
        }

        /// <summary>
        /// FechaConfirmadaSave.
        /// </summary>
        /// <param name="IdUnidadCultivo">IdUnidadCultivo<see cref="string"/>.</param>
        /// <param name="temporada">temporada<see cref="string"/>.</param>
        /// <param name="nEtapa">nEtapa<see cref="int"/>.</param>
        /// <param name="fechaConfirmada">fechaConfirmada<see cref="DateTime"/>.</param>
        public static void FechaConfirmadaSave(string IdUnidadCultivo, string temporada, int nEtapa, DateTime fechaConfirmada) {
            try {
                Database db = DB.ConexionOptiaqua;
                UnidadCultivoCultivoEtapas dat = new UnidadCultivoCultivoEtapas {
                    IdUnidadCultivo = IdUnidadCultivo,
                    IdTemporada = temporada,
                    IdEtapaCultivo = nEtapa
                };
                dat = db.SingleOrDefaultById<UnidadCultivoCultivoEtapas>(dat);
                if (dat != null) {
                    dat.FechaInicioEtapaConfirmada = fechaConfirmada;
                    db.Save(dat);
                } else {
                    throw new Exception("Error accediendo a UnidadCultivoCultivoEtapas\n.");
                }
            } catch (Exception ex) {
                string msgErr = "Error cargando ParcelasCultivosEtapas.\n ";
                msgErr += ex.Message;
                throw new Exception(msgErr);
            }
        }

        /// <summary>
        /// CultivoEtapasList.
        /// </summary>
        /// <param name="idCultivo">idCultivo<see cref="int?"/>.</param>
        /// <returns><see cref="List{CultivoEtapas}"/>.</returns>
        public static List<CultivoEtapas> CultivoEtapasList(int? idCultivo) {
            if (idCultivo == null)
                return null;
            Database db = DB.ConexionOptiaqua;
            List<CultivoEtapas> listaCF = db.Fetch<CultivoEtapas>("Select * from CultivoEtapas Where IdCultivo=@0", idCultivo);
            return listaCF;
        }

        /// <summary>
        /// The MateriaOrganicaTipo.
        /// </summary>
        /// <param name="idMateriaOrganicaTipo">The idMateriaOrganicaTipo<see cref="string"/>.</param>
        /// <returns>The <see cref="MateriaOrganicaTipo"/>.</returns>
        public static MateriaOrganicaTipo MateriaOrganicaTipo(string idMateriaOrganicaTipo) {
            Database db = DB.ConexionOptiaqua;
            MateriaOrganicaTipo ret = db.SingleById<MateriaOrganicaTipo>(idMateriaOrganicaTipo);
            return ret;
        }

        /// <summary>
        /// MateriaOrganicaTipo.
        /// </summary>
        /// <returns><see cref="List{MateriaOrganicaTipo}"/>.</returns>
        public static List<MateriaOrganicaTipo> MateriaOrganicaTipoList() {
            Database db = DB.ConexionOptiaqua;
            List<MateriaOrganicaTipo> ret = db.Fetch<MateriaOrganicaTipo>("select * from MateriaOrganicaTipo");
            return ret;
        }

        /// <summary>
        /// The DatosExtraSave.
        /// </summary>
        /// <param name="param">The param<see cref="PostDatosExtraParam"/>.</param>
        public static void DatosExtraSave(PostDatosExtraParam param) {
            try {
                if (DateTime.TryParse(param.Fecha, out DateTime fs) == false) {
                    throw new Exception("Error. El formato de la fecha no es correcto.\n");
                }
                Database db = DB.ConexionOptiaqua;
                UnidadCultivoDatosExtra dat = new UnidadCultivoDatosExtra() { IdUnidadCultivo = param.IdUnidadCultivo, Fecha = fs };
                dat = db.SingleOrDefaultById<UnidadCultivoDatosExtra>(dat);
                if (dat == null)
                    dat = new UnidadCultivoDatosExtra();
                dat.IdUnidadCultivo = param.IdUnidadCultivo;
                dat.Fecha = fs;
                if (param.Cobertura != -1)
                    dat.Cobertura = param.Cobertura;
                if (param.Lluvia != -1)
                    dat.LluviaMm = param.Lluvia;
                if (param.Altura != -1)
                    dat.Altura = param.Altura;
                if (param.DriEnd != -1)
                    dat.DriEnd = param.DriEnd;

                if (param.RiegoM3 != -1) {
                    dat.RiegoM3 = param.RiegoM3;
                    param.RiegoHr = DB.ConversionM3AHorasRiego((double)param.RiegoM3, param.IdUnidadCultivo, fs);
                    param.RiegoMm = DB.ConversionM3RiegoAMm((double)param.RiegoM3, param.IdUnidadCultivo, fs);
                } else if (param.RiegoHr != -1) {
                    dat.RiegoM3 = DB.ConversionHorasRiegoAM3((double)param.RiegoHr, param.IdUnidadCultivo, fs);
                    param.RiegoM3 = dat.RiegoM3;
                    param.RiegoMm = param.RiegoM3 / 1000;
                } else if (param.RiegoMm != -1) {
                    dat.RiegoM3 = DB.ConversionMmRiegoAM3((double)param.RiegoMm, param.IdUnidadCultivo, fs);
                    param.RiegoM3 = dat.RiegoM3;
                    param.RiegoHr = DB.ConversionM3AHorasRiego((double)param.RiegoM3, param.IdUnidadCultivo, fs);
                }

                // Si se indica riego 0 m3 se pone a nulo.
                if (dat.RiegoM3 == 0)
                    dat.RiegoM3 = null;

                db.Save(dat);
            } catch (Exception ex) {
                string msgErr = "Error al guardar datos extra.\n ";
                msgErr += ex.Message;
                throw new Exception(msgErr);
            }
        }

        /// <summary>
        /// The ConversionHorasRiegoAM3.
        /// </summary>
        /// <param name="horasRiego">The horasRiego<see cref="double"/>.</param>
        /// <param name="idUnidadCultivo">The idUnidadCultivo<see cref="string"/>.</param>
        /// <param name="fecha">The fecha<see cref="DateTime"/>.</param>
        /// <returns>The <see cref="double"/>.</returns>
        private static double ConversionHorasRiegoAM3(double horasRiego, string idUnidadCultivo, DateTime fecha) {
            string idTemporada = DB.TemporadaDeFecha(idUnidadCultivo, fecha);
            double superficieM2 = UnidadCultivoExtensionM2(idUnidadCultivo, idTemporada);
            double pluviometriaRiego = DB.UnidadCultivoCultivo(idUnidadCultivo, idTemporada).Pluviometria;
            double m3 = horasRiego * pluviometriaRiego * superficieM2 / 1000;
            return m3;
        }

        /// <summary>
        /// The ConversionMmRiegoAM3.
        /// </summary>
        /// <param name="mmRiego">The mmRiego<see cref="double"/>.</param>
        /// <param name="idUnidadCultivo">The idUnidadCultivo<see cref="string"/>.</param>
        /// <param name="fecha">The fecha<see cref="DateTime"/>.</param>
        /// <returns>The <see cref="double"/>.</returns>
        private static double ConversionMmRiegoAM3(double mmRiego, string idUnidadCultivo, DateTime fecha) {
            string idTemporada = DB.TemporadaDeFecha(idUnidadCultivo, fecha);
            double superficieM2 = UnidadCultivoExtensionM2(idUnidadCultivo, idTemporada);
            double m3 = mmRiego / 1000 * superficieM2;
            return m3;
        }

        /// <summary>
        /// The ConversionM3RiegoAMm.
        /// </summary>
        /// <param name="m3">The m3<see cref="double"/>.</param>
        /// <param name="idUnidadCultivo">The idUnidadCultivo<see cref="string"/>.</param>
        /// <param name="fecha">The fecha<see cref="DateTime"/>.</param>
        /// <returns>The <see cref="double"/>.</returns>
        private static double ConversionM3RiegoAMm(double m3, string idUnidadCultivo, DateTime fecha) {
            string idTemporada = DB.TemporadaDeFecha(idUnidadCultivo, fecha);
            double superficieM2 = UnidadCultivoExtensionM2(idUnidadCultivo, idTemporada);
            double mm = m3 * 1000 / superficieM2;
            return mm;
        }

        /// <summary>
        /// The ConversionM3AHorasRiego.
        /// </summary>
        /// <param name="m3">The m3<see cref="double"/>.</param>
        /// <param name="idUnidadCultivo">The idUnidadCultivo<see cref="string"/>.</param>
        /// <param name="fecha">The fecha<see cref="DateTime"/>.</param>
        /// <returns>The <see cref="double"/>.</returns>
        private static double ConversionM3AHorasRiego(double m3, string idUnidadCultivo, DateTime fecha) {
            string idTemporada = DB.TemporadaDeFecha(idUnidadCultivo, fecha);
            if (idTemporada == null)
                throw new Exception("No hay definida una temporada para unidad de cultivo y fecha.");
            double supertificeM2 = UnidadCultivoExtensionM2(idUnidadCultivo, idTemporada);
            double pluviometriaRiego = DB.UnidadCultivoCultivo(idUnidadCultivo, idTemporada).Pluviometria;
            double divisor = pluviometriaRiego * supertificeM2 / 1000;
            double horasRiego = 0;
            if (divisor != 0)
                horasRiego = m3 / divisor;
            return horasRiego;
        }

        /// <summary>
        /// The ElementosGruesosTipo.
        /// </summary>
        /// <param name="IdElementosGruesos">The IdElementosGruesos<see cref="string"/>.</param>
        /// <returns>The <see cref="ElementosGruesosTipo"/>.</returns>
        public static ElementosGruesosTipo ElementosGruesosTipo(string IdElementosGruesos) {
            Database db = DB.ConexionOptiaqua;
            ElementosGruesosTipo ret = db.SingleOrDefaultById<ElementosGruesosTipo>(IdElementosGruesos);
            if (ret == null)
                throw new Exception();
            return ret;
        }

        /// <summary>
        /// ElementosGruesosTipo.
        /// </summary>
        /// <returns><see cref="List{ElementosGruesosTipo}"/>.</returns>
        public static List<ElementosGruesosTipo> ElementosGruesosTipoList() {
            Database db = DB.ConexionOptiaqua;
            List<ElementosGruesosTipo> ret = db.Fetch<ElementosGruesosTipo>("select * from ElementosGruesosTipo");
            return ret;
        }

        /// <summary>
        /// TemporadasUnidadCultivoList.
        /// </summary>
        /// <param name="idUnidadCultivo">idUnidadCultivo<see cref="string"/>.</param>
        /// <returns><see cref="List{string}"/>.</returns>
        public static List<string> TemporadasUnidadCultivoList(string idUnidadCultivo) {
            Database db = DB.ConexionOptiaqua;
            string sql;
            sql = $"Select Distinct IdTemporada from UnidadCultivoCultivo where IdUnidadCultivo ='{idUnidadCultivo}'";
            List<string> ret = db.Fetch<string>(sql);
            return ret;
        }

        /// <summary>
        /// Retorna los datos del suelo para el horizonte indicado.
        /// Si horizonte=="ALL" retorna todos.
        /// </summary>
        /// <param name="idParcelaInt">.</param>
        /// <param name="idHorizonte">.</param>
        /// <returns>.</returns>
        public static List<ParcelaSuelo> PacelaHorizonte(int idParcelaInt, string idHorizonte) {
            List<ParcelaSuelo> ret = null;
            Database db = DB.ConexionOptiaqua;
            if (idHorizonte == "ALL")
                ret = db.Fetch<ParcelaSuelo>("Select * from ParcelaSuelo where IdParcelaInt=@0 ", idParcelaInt);
            else
                ret = db.Fetch<ParcelaSuelo>("Select * from ParcelaSuelo where IdParcelaInt =@0 and idHorizonte=@1 ", idParcelaInt, int.Parse(idHorizonte));
            return ret;
        }

        /// <summary>
        /// ParcelasDatosExtrasList.
        /// </summary>
        /// <param name="IdUnidadCultivo">IdUnidadCultivo<see cref="string"/>.</param>
        /// <param name="desdeFecha">desdeFecha<see cref="DateTime"/>.</param>
        /// <param name="hastaFecha">hastaFecha<see cref="DateTime"/>.</param>
        /// <returns><see cref="List{UnidadCultivoDatosExtra}"/>.</returns>
        public static List<UnidadCultivoDatosExtra> ParcelasDatosExtrasList(string IdUnidadCultivo, DateTime desdeFecha, DateTime hastaFecha) {
            if (IdUnidadCultivo == null || desdeFecha == null || hastaFecha == null)
                return null;
            Database db = DB.ConexionOptiaqua;
            string sql = "where fecha BETWEEN @0 AND @1 AND IdUnidadCultivo=@2";
            return db.Fetch<UnidadCultivoDatosExtra>(sql, desdeFecha, hastaFecha, IdUnidadCultivo);
        }

        /// <summary>
        /// Retorno lista de riegos.
        /// </summary>
        /// <param name="idUnidadCultivo">.</param>
        /// <param name="fechaSiembra">.</param>
        /// <param name="fechaFinal">.</param>
        /// <returns>.</returns>
        public static List<Riego> RiegosList(string idUnidadCultivo, DateTime fechaSiembra, DateTime fechaFinal) {
            DateTime? ultAct = Config.GetDateTime("FechaUltimaActualizacionNebula");
            if (ultAct == null || ultAct < DateTime.Today) {
                RiegosNebulaListRefresh(ultAct);
                Config.SetDateTime("FechaUltimaActualizacionNebula", DateTime.Today);
            }
            if (idUnidadCultivo == null || fechaSiembra == null || fechaFinal == null)
                return null;
            Database db = DB.ConexionOptiaqua;

            #region Unidad de cultivo virtual
            var idUnidadCultivoVirtual = db.SingleById<UnidadCultivo>(idUnidadCultivo)?.TomarRiegosDeIdCultivo;
            if (!string.IsNullOrWhiteSpace(idUnidadCultivoVirtual))
                idUnidadCultivo = idUnidadCultivoVirtual;
            #endregion

            string sql = "WHERE idUnidadCultivo=@0 and fecha BETWEEN @1 and @2";
            return db.Fetch<Riego>(sql, idUnidadCultivo, fechaSiembra, fechaFinal);
        }

        /// <summary>
        /// Retorna los datos de la tabla ParcelasCultivo.
        /// </summary>
        /// <param name="idUnidadCultivo">.</param>
        /// <param name="idTemporada">.</param>
        /// <returns>.</returns>
        public static UnidadCultivoCultivo UnidadCultivoCultivo(string idUnidadCultivo, string idTemporada) {
            if (idUnidadCultivo == null || idTemporada == null)
                return null;
            Database db = DB.ConexionOptiaqua;
            string sql;
            sql = "Select * from UnidadCultivoCultivo where idUnidadCultivo=@0 AND IdTemporada=@1";
            return db.SingleOrDefault<UnidadCultivoCultivo>(sql, idUnidadCultivo, idTemporada);
        }

        /// <summary>
        /// UnidadCultivo.
        /// </summary>
        /// <param name="idUnidadCultivo">idUnidadCultivo<see cref="string"/>.</param>
        /// <returns><see cref="UnidadCultivo"/>.</returns>
        public static UnidadCultivo UnidadCultivo(string idUnidadCultivo) {
            if (idUnidadCultivo == null)
                return null;
            Database db = DB.ConexionOptiaqua;
            return db.SingleOrDefaultById<UnidadCultivo>(idUnidadCultivo);
        }

        /// <summary>
        /// UnidadCultivoList.
        /// </summary>
        /// <returns><see cref="List{UnidadCultivo}"/>.</returns>
        public static List<UnidadCultivo> UnidadCultivoList() {
            Database db = DB.ConexionOptiaqua;
            return db.Fetch<UnidadCultivo>();
        }

        /// <summary>
        /// Retorna una clase con todos los valores de la parcela IdParcela desde BD.
        /// </summary>
        /// <param name="idParcela">.</param>
        /// <returns>.</returns>
        public static Parcela Parcela(int idParcela) {
            Database db = null;
            Parcela ret = null;
            try {
                db = DB.ConexionOptiaqua;
                string sql = "SELECT IdParcelaInt, IdGadmin, IdRegante, IdProvincia, IdMunicipio, IdPoligono, IdParcela, IdParaje, Descripcion, Longitud, Latitud, XUTM, YUTM, Huso, Altitud, RefCatastral, GID, SuperficieM2 FROM dbo.Parcela";
                sql += " where idParcelaInt=" + idParcela.ToString();
                ret = db.Single<Parcela>(sql);
            } catch (Exception ex) {
                throw new Exception("No se pudo cargar parcela:" + idParcela.ToString() + " -" + ex.Message);
            }
            return ret;
        }

        /// <summary>
        /// Carga los datos del cultivo referenciado.
        /// </summary>
        /// <param name="idCultivo">.</param>
        /// <returns>.</returns>
        public static Cultivo Cultivo(int? idCultivo) {
            if (idCultivo == null)
                return null;
            Database db = DB.ConexionOptiaqua;
            var ret = db.SingleOrDefaultById<Cultivo>(idCultivo);
            if (ret == null) {
                throw new Exception($"No se encuenta cultivo:{idCultivo}");
            }
            return ret;
        }

        /// <summary>
        /// UnidadCultivoCultivoEtapasList.
        /// </summary>
        /// <param name="idUnidadCultivo">idUnidadCultivo<see cref="string"/>.</param>
        /// <param name="idTemporada">idTemporada<see cref="string"/>.</param>
        /// <returns><see cref="List{UnidadCultivoCultivoEtapas}"/>.</returns>
        public static List<UnidadCultivoCultivoEtapas> UnidadCultivoCultivoEtapasList(string idUnidadCultivo, string idTemporada) {
            if (idUnidadCultivo == null || idTemporada == null)
                return null;
            Database db = DB.ConexionOptiaqua;
            string sql;
            sql = "Select * from UnidadCultivoCultivoEtapas where IdUnidadCultivo =@0 AND IDTemporada=@1";
            return db.Fetch<UnidadCultivoCultivoEtapas>(sql, idUnidadCultivo, idTemporada);
        }

        /// <summary>
        /// ParcelasCultivo.
        /// </summary>
        /// <param name="IdParcela">IdParcela<see cref="int"/>.</param>
        /// <param name="temporada">temporada<see cref="string"/>.</param>
        /// <returns><see cref="UnidadCultivoCultivo"/>.</returns>
        public static UnidadCultivoCultivo ParcelasCultivo(int IdParcela, string temporada) {
            Database db = DB.ConexionOptiaqua;
            string sql;
            sql = "Select * from ParcelasCultivoEtapas where IdParcela =" + IdParcela + " AND IDTemporada='" + temporada + "' ";
            return db.SingleOrDefault<UnidadCultivoCultivo>(sql);
        }

        public static List<DatosSuelo> ParcelaSueloList(string idUC, string idTemporada) {

            var lDatosExtra = UnidadCultivoDatosExtraSuelo(idUC, idTemporada);
            if (lDatosExtra!=null && lDatosExtra.Count>0)
                return lDatosExtra; 

            var lComunidadRegantes = DatosSueloComunidadRegantes(idUC, idTemporada);
            // si tenemos datos de la unidad de cultivo 
            if (lComunidadRegantes != null && lComunidadRegantes.Count > 0)
                return lComunidadRegantes;

            var db = DB.ConexionOptiaqua;
            var lDatosSueloParcelas = new List<DatosSuelo>();
            var idVersionMapa = db.Single<string>($"select IdVersionMapa from Temporada where idTemporada='{idTemporada}'");
            if (idVersionMapa != null) {
                var sqlParcelas = $"select * from ParcelasDeUC where IdTemporada='{idTemporada}' and IdUnidadCultivo='{idUC}'";
                var lParcelas = db.Fetch<Parcela>(sqlParcelas);
                foreach (var parcela in lParcelas) {
                    var dsCatastral = DatosSueloMapasCatastrales(idVersionMapa, parcela, out double lat, out double lng);
                    if (dsCatastral != null) {
                        lDatosSueloParcelas.Add(dsCatastral);
                        if (parcela.Latitud == null || parcela.Longitud == null) {
                            parcela.Latitud = lat;
                            parcela.Longitud = lng;
                        }
                    }
                    var dsBase = DatosSueloMapasBase(idVersionMapa, parcela);
                    if (dsBase != null) {
                        var ds1 = dsBase[0];
                        if (dsCatastral == null || ds1.ProfundidadCM > dsCatastral.ProfundidadCM) {
                            lDatosSueloParcelas.Add(ds1);
                        }
                        if (dsBase.Count > 1) {
                            var ds2 = dsBase[1];
                            if (dsCatastral == null || ds2.ProfundidadCM > dsCatastral.ProfundidadCM) {
                                lDatosSueloParcelas.Add(ds2);
                            }
                        }
                    }
                }
            }
            if (lDatosSueloParcelas.Count == 0)
                throw new Exception($"No se encontraron datos de suelo para la unidad de cultivo: '{idUC}' en la temporada: '{idTemporada}'");
            double totalSuperficie = 0;
            foreach (var parcela in lDatosSueloParcelas) {
                parcela.Superficie = db.SingleOrDefault<double>($"select SuperficieM2 from Parcela where idParcelaInt={parcela.IdParcelaInt}");
                totalSuperficie += parcela.Superficie;
            }
            var porParcela = lDatosSueloParcelas.GroupBy(x => x.IdParcelaInt).Select(x => new { x.Key, list = x.ToList() }).ToList();
            // EN ldsBase ahora tenemos datos del suelo para todas las parcelas. Si lo datos se han tomado de MapaBese con dos horizontes, sino con un solo horizonte.
            // Ahora lo tenemos que fusionar en lista única.
            var ret = new List<DatosSuelo>();
            double hAnt = 0;
            if (porParcela.Count == 0)
                return ret;

            var hEstudio = lDatosSueloParcelas.Where(x => x.ProfundidadCM > hAnt).Min(x => x.ProfundidadCM);
            while (hEstudio > 0) {
                double arena_dividendo = 0, limo_dividendo = 0, arcilla_dividendo = 0, eg_dividendo = 0, mo_dividendo = 0;
                foreach (var par in porParcela) {
                    var ds = par.list.FirstOrDefault(x => x.ProfundidadCM <= hEstudio);
                    if (ds != null) {
                        arena_dividendo += ds.Arena * ds.Superficie;
                        limo_dividendo += ds.Limo * ds.Superficie;
                        arcilla_dividendo += ds.Arcilla * ds.Superficie;
                        eg_dividendo += ds.ElementosGruesos * ds.Superficie;
                        mo_dividendo += ds.MateriaOrganica * ds.Superficie;
                    }
                }
                var reg = new DatosSuelo {
                    ProfundidadCM = hEstudio,
                    Superficie = totalSuperficie,
                    Arena = arena_dividendo / totalSuperficie,
                    Limo = limo_dividendo / totalSuperficie,
                    Arcilla = arcilla_dividendo / totalSuperficie,
                    ElementosGruesos = eg_dividendo / totalSuperficie,
                    MateriaOrganica = mo_dividendo / totalSuperficie,
                    IdParcelaInt = -1
                };
                ret.Add(reg);
                hAnt = hEstudio;
                var lSigH = lDatosSueloParcelas.Where(x => x.ProfundidadCM > hAnt);
                if (lSigH.Any())
                    hEstudio = lSigH.Min(x => x.ProfundidadCM);
                else
                    hEstudio = 0;
            }
            return ret;
        }

        public static List<DatosSuelo> UnidadCultivoDatosExtraSuelo(string idUC, string idTemporada) {
            var ret = DB.ConexionOptiaqua.Fetch<DatosSuelo>("select * from UnidadCultivoDatosExtraSuelo where IdUnidadCultivo=@0 and IdTemporada=@1",idUC,idTemporada);
            return ret;
        }

        public static List<DatosSuelo> DatosSueloComunidadRegantes(string idUC, string idTemporada) {

            Database db = DB.ConexionOptiaqua;
            var sql = $@"
                SELECT dbo.ParcelaSuelo.*, dbo.Parcela.SuperficieM2 as Superficie FROM dbo.Parcela INNER JOIN
                    dbo.ParcelaSuelo ON dbo.Parcela.IdParcelaInt = dbo.ParcelaSuelo.IdParcelaInt INNER JOIN
                    dbo.UnidadCultivoParcela ON dbo.Parcela.IdParcelaInt = dbo.UnidadCultivoParcela.IdParcelaInt
                WHERE (dbo.UnidadCultivoParcela.IdTemporada = '{idTemporada}') AND (dbo.UnidadCultivoParcela.IdUnidadCultivo = '{idUC}')            
            ";

            var lDatosSueloParcelas = db.Fetch<DatosSuelo>(sql);
            if (lDatosSueloParcelas.Count == 0)
                return lDatosSueloParcelas;
            double hAnt = 0;
            var porParcela = lDatosSueloParcelas.GroupBy(x => x.IdParcelaInt).Select(x => new { x.Key, list = x.ToList() }).ToList();
            // EN ldsBase ahora tenemos datos del suelo para todas las parcelas. Si lo datos se han tomado de MapaBese con dos horizontes, sino con un solo horizonte.
            // Ahora lo tenemos que fusionar en lista única.
            var ret = new List<DatosSuelo>();
            double totalSuperficie = porParcela.Sum(x => x.list[0].Superficie);
            if (totalSuperficie == 0)
                totalSuperficie = 1;
            var hEstudio = lDatosSueloParcelas.Where(x => x.ProfundidadCM > hAnt).Min(x => x.ProfundidadCM);
            while (hEstudio > 0) {
                double arena_dividendo = 0, limo_dividendo = 0, arcilla_dividendo = 0, eg_dividendo = 0, mo_dividendo = 0;
                foreach (var par in porParcela) {
                    var ds = par.list.FirstOrDefault(x => x.ProfundidadCM <= hEstudio);
                    if (ds != null) {
                        arena_dividendo += ds.Arena * ds.Superficie;
                        limo_dividendo += ds.Limo * ds.Superficie;
                        arcilla_dividendo += ds.Arcilla * ds.Superficie;
                        eg_dividendo += ds.ElementosGruesos * ds.Superficie;
                        mo_dividendo += ds.MateriaOrganica * ds.Superficie;
                    }
                }
                var reg = new DatosSuelo {
                    ProfundidadCM = hEstudio,
                    Superficie = totalSuperficie,
                    Arena = arena_dividendo / totalSuperficie,
                    Limo = limo_dividendo / totalSuperficie,
                    Arcilla = arcilla_dividendo / totalSuperficie,
                    ElementosGruesos = eg_dividendo / totalSuperficie,
                    MateriaOrganica = mo_dividendo / totalSuperficie,
                    IdParcelaInt = -1
                };
                ret.Add(reg);
                hAnt = hEstudio;
                var lSigH = lDatosSueloParcelas.Where(x => x.ProfundidadCM > hAnt);
                if (lSigH.Any())
                    hEstudio = lSigH.Min(x => x.ProfundidadCM);
                else
                    hEstudio = 0;
            }
            return ret;
        }

        public static List<DatosSuelo> DatosSueloMapasBase(string idVersionMapa, Parcela parcela) {
            Database db = DB.ConexionOptiaqua;
            var ret = new List<DatosSuelo>();
            var lBase = new List<MapaBaseSqlserverPoco>();


            var lat = parcela.Latitud.ToString().Replace(",", ".");
            var lng = parcela.Longitud.ToString().Replace(",", ".");
            if (string.IsNullOrWhiteSpace(lat) || string.IsNullOrWhiteSpace(lng))
                throw new Exception($"La pacela '{parcela.IdParcelaInt}' no tiene valores latitud y longitud válidos");
            var sqlBase = @"
                    SELECT IdVersion, ID, ID_INTER_PROSP_RELIEVE, Litologia, Cuenca, Proy_GEODE, COORD_X, COORD_Y, CLASIFICAC, HS_PROF_cm, HS_ARENA_Porc, HS_ARCILLA_Porc, HS_LIMO_Porc, HS_TEXTURA, HS_EGRUESO_Porc, 
                         HS_CALIZAA_g_Kg, HS_CARBONA_g_Kg, HS_CE_dS_m, HS_MATORG_Porc, HS_PH, HS_CIC_meq100g, SC_ESPESOR_cm, SC_ARENA_Porc, SC_ARCILLA_Porc, SC_LIMO_Porc, SC_TEXTURA, SC_EGRUESO_Porc, 
                         SC_CALIZAA_g_Kg, SC_CARBONA_g_Kg, SC_CE_dS_m, SC_MATORG_Porc, SC_PH, SC_CIC_meq_100g, PROF_EFECTIVA_cm, GEOFORMA, SUELO1, SUELO2, SUELO3
                    FROM dbo.MapaBase
                 ";
            sqlBase += $" where geom.STContains( geometry::STGeomFromText('POINT({lng}  {lat})', 4326)) =1";
            var rBase = db.SingleOrDefault<MapaBaseSqlserverPoco>(sqlBase);
            if (rBase == null)
                return null;
            // ignorar si tiene valores null
            if (rBase.HS_ARCILLA_Porc == null && rBase.HS_LIMO_Porc == null && rBase.HS_ARENA_Porc == null && rBase.HS_EGRUESO_Porc == null && rBase.HS_MATORG_Porc == null)
                return null;

            var ds1 = new DatosSuelo {
                IdParcelaInt = parcela.IdParcelaInt,
                ProfundidadCM = rBase.HS_PROF_cm ?? 0,
                Arcilla = rBase.HS_ARCILLA_Porc.Value/100.0,
                Arena = rBase.HS_ARENA_Porc.Value/100.0,
                Limo = rBase.HS_LIMO_Porc.Value / 100.0,
                ElementosGruesos = rBase.HS_EGRUESO_Porc.Value / 100.0,
                MateriaOrganica = rBase.HS_MATORG_Porc.Value / 100.0
            };
            ret.Add(ds1);

            var ds2 = new DatosSuelo {
                IdParcelaInt = parcela.IdParcelaInt,
                ProfundidadCM = rBase.PROF_EFECTIVA_cm ?? 0,
                Arcilla = rBase.SC_ARCILLA_Porc.Value / 100.0,
                Arena = rBase.SC_ARENA_Porc.Value / 100.0,
                Limo = rBase.SC_LIMO_Porc.Value / 100.0,
                ElementosGruesos = rBase.SC_EGRUESO_Porc.Value / 100.0,
                MateriaOrganica = rBase.SC_MATORG_Porc.Value / 100.0
            };
            if (ds2.ProfundidadCM >= ds1.ProfundidadCM)// solo si aporta información se suma
                ret.Add(ds2);

            return ret;
        }

        public static DatosSuelo DatosSueloMapasCatastrales(string idVersionMapa, Parcela parcela, out double lat, out double lng) {
            Database db = DB.ConexionOptiaqua;

            var sqlCatastral = @"SELECT        IdVersion, ID, ID_MUESTRA, ORIGEN_MUESTRA, FECHA_MUESTRA, PROVINCIA, MUNICIPIO, POLIGONO, PARCELA,
                                   REF_CATAST, Latitud, Longitud, HS_PROF_cm, HS_ARENA_Porc, HS_ARCILLA_Porc, HS_LIMO_Porc, HS_TEXTURA, HS_EGRUESO_Porc, 
                                    HS_MATORG_Porc, HS_PH, PROF_EFECTIVA_cm, OBSERVACIONES FROM dbo.MapaCatastral ";
            sqlCatastral += $"where Provincia={parcela.IdProvincia} and  Municipio={parcela.IdMunicipio} and Poligono={parcela.IdPoligono} and Parcela = {parcela.IdParcela} and IdVersion='{idVersionMapa}' ";
            var catastral = db.SingleOrDefault<MapaCatastralSqlserverPoco>(sqlCatastral);
            lat = 0;
            lng = 0;
            // ignorar si tiene valores null
            if (catastral == null)
                return null;
            if (catastral.HS_ARCILLA_Porc == null && catastral.HS_LIMO_Porc == null && catastral.HS_ARENA_Porc == null && catastral.HS_EGRUESO_Porc == null && catastral.HS_MATORG_Porc == null)
                return null;
                //throw new Exception($"Los datos en el mapa catastral '{idVersionMapa}' para la parcela '{parcela.IdParcelaInt}' no so válidos");
            lat = catastral.Latitud ?? 0;
            lng = catastral.Longitud ?? 0;
            double profSuperf = catastral.HS_PROF_cm ?? 0;
            if (catastral.PROF_EFECTIVA_cm != null && catastral.PROF_EFECTIVA_cm < profSuperf) {
                profSuperf = catastral.PROF_EFECTIVA_cm.Value;
            }

            var ds = new DatosSuelo {
                IdParcelaInt = parcela.IdParcelaInt,
                ProfundidadCM = profSuperf,
                Arcilla = catastral.HS_ARCILLA_Porc/100.0 ?? 0,
                Arena = catastral.HS_ARENA_Porc / 100.0 ?? 0,
                Limo = catastral.HS_LIMO_Porc / 100.0 ?? 0,
                ElementosGruesos = catastral.HS_EGRUESO_Porc / 100.0 ?? 0,
                MateriaOrganica = catastral.HS_MATORG_Porc / 100.0 ?? 0
            };
            return ds;
        }

        public static void MapasCatastralesDatosLngLat(Parcela parcela, out double lat, out double lng) {
            Database db = DB.ConexionOptiaqua;

            var sqlCatastral = @"SELECT Latitud, Longitud FROM dbo.MapaCatastral ";
            sqlCatastral += $"where Provincia={parcela.IdProvincia} and  Municipio={parcela.IdMunicipio} and Poligono={parcela.IdPoligono} and Parcela = {parcela.IdParcela} ";
            var catastral = db.FirstOrDefault<MapaCatastralSqlserverPoco>(sqlCatastral);
            lat = 0;
            lng = 0;
            if (catastral == null)
                return;
            lat = catastral.Latitud ?? 0;
            lng = catastral.Longitud ?? 0;
        }


        /// <summary>
        /// Actualiza los datos climáticos almacenados.
        /// Se conecta a el api del SIAR si hace al menos una hora que no lo ha hecho y actualiza los datos desde última acualización.
        /// Si se actualizó hace menos de 4 días actuliza los últimos 4 días.
        /// </summary>
        public static void DatosClimaticosSiarForceRefresh() {
            var emailNotificacionErrorDatosClimaticos = "siar.cida@larioja.org";
            try {
                DateTime? ultimaFechaEnTabla = DB.UltimaFechaDeEstacion();
                List<int> lEstaciones = DB.EstacionesIdList();
                if (ultimaFechaEnTabla == null)
                    ultimaFechaEnTabla = new DateTime(2000, 01, 01);

                var nDiasAtras = DB.ConfigLoadInt("ActualizarDatosClimaticosNDias") ?? 4;
                DateTime desdeFecha = ((DateTime)ultimaFechaEnTabla).AddDays(-nDiasAtras); // Añado 4 días a la lista
                DateTime hastaFecha = DateTime.Today;

                foreach (var idEstacion in lEstaciones) {
                    List<DatoClimatico> datClima = DatosClimaticosSiarList(desdeFecha, hastaFecha, idEstacion);
                    DB.DatosClimaticosSave(datClima);
                    if (nDiasAtras + 1 != datClima.Count && datClima.Max(x => x.Fecha) != DateTime.Today) {
                        emailNotificacionErrorDatosClimaticos = DB.ConfigLoad("EmailNotificacionErrorDatosClimaticos");
                        var subject = "Error en lectura de datos climáticos. Fecha: " + hastaFecha.Date.ToShortDateString();
                        var body = DateTime.Now + " -" + subject;
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                        var ultimaFechaEnvioEmailError = DB.ConfigLoadDate("ultimaFechaEnvioEmailError");
                        if (ultimaFechaEnvioEmailError == null || DateTime.Today > ultimaFechaEnvioEmailError) {
                            Email.SendMail(emailNotificacionErrorDatosClimaticos, subject, body, null);
                            DB.ConfigSave("ultimaFechaEnvioEmailError", DateTime.Today.ToShortDateString());
                        }
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    }
                }
            } catch {
                emailNotificacionErrorDatosClimaticos = DB.ConfigLoad("EmailNotificacionErrorDatosClimaticos");
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                Email.SendMail(emailNotificacionErrorDatosClimaticos, "Error en lectura de datos climáticos", "Error indeterminado");
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                // continua sin datos del SIAR
            }
        }

        private static List<int> EstacionesIdList() {
            Database db = DB.ConexionOptiaqua;
            return db.Fetch<int>("select * from EstacionIdList");
        }

        /// <summary>
        /// Llama a refrescar datos climáticos si aún no se ha hecho a fecha actual.
        /// </summary>
        public static void DatosClimaticosSiarRefresh() {
            DateTime? ultimaFechaActualizacionDatosCliematicosSiar = Config.GetDateTime("FechaUltimaActualizacionSiar");
            if (ultimaFechaActualizacionDatosCliematicosSiar == null || ultimaFechaActualizacionDatosCliematicosSiar.Value.Date < DateTime.Today) {
                DB.DatosClimaticosSiarForceRefresh();
                Config.SetDateTime("FechaUltimaActualizacionSiar", DateTime.Today);
            }
        }

        /// <summary>
        /// DatosClimaticosSave.
        /// </summary>
        /// <param name="lDatClima">lDatClima<see cref="List{DatoClimatico}"/>.</param>
        private static void DatosClimaticosSave(List<DatoClimatico> lDatClima) {
            Database db = DB.ConexionOptiaqua;
            foreach (DatoClimatico datCli in lDatClima) {
                db.Save(datCli);
            }
        }

        /// <summary>
        /// UltimaFechaDeEstacion.
        /// </summary>
        /// <returns><see cref="DateTime?"/>.</returns>
        private static DateTime? UltimaFechaDeEstacion() {
            Database db = DB.ConexionOptiaqua;
            string sql;
            sql = "SELECT MIN(MaxFecha) AS MinFecha FROM dbo.DatoClimaticoMaxFecha";
            return db.Single<DateTime?>(sql);
        }

        /// <summary>
        /// DatosClimaticosList.
        /// </summary>
        /// <param name="desdeFecha">desdeFecha<see cref="DateTime?"/>.</param>
        /// <param name="hastaFecha">hastaFecha<see cref="DateTime?"/>.</param>
        /// <param name="idEstacion">idEstacion<see cref="int?"/>.</param>
        /// <returns><see cref="List{DatoClimatico}"/>.</returns>
        public static List<DatoClimatico> DatosClimaticosList(DateTime? desdeFecha, DateTime? hastaFecha, int? idEstacion) {
            if (desdeFecha == null || hastaFecha == null || idEstacion == null)
                return null;
            // Refrescar la base de datos con los datos de Siar si es necesario
            DB.DatosClimaticosSiarRefresh();
            Database db = DB.ConexionOptiaqua;
            string sql = "Select * from DatoClimatico where fecha BETWEEN  @0 AND @1 AND IDESTACION=@2";
            return db.Fetch<DatoClimatico>(sql, desdeFecha, hastaFecha, idEstacion);
        }

        /// <summary>
        /// DatosClimaticosSiarList.
        /// </summary>
        /// <param name="desdeFecha">desdeFecha<see cref="DateTime"/>.</param>
        /// <param name="hastaFecha">hastaFecha<see cref="DateTime"/>.</param>
        /// <param name="idEstacion">idEstacion<see cref="int"/>.</param>
        /// <returns><see cref="List{DatoClimatico}"/>.</returns>
        public static List<DatoClimatico> DatosClimaticosSiarList(DateTime desdeFecha, DateTime hastaFecha, int idEstacion) {
            try {
                string sURL;
                //sURL Ejemplo = "http://apisiar.larioja.org/v1/datos-calculo-riego?estacion=501&fechaInicio=2017-08-07&fechaFin=2017-08-10";
                sURL = "http://apisiar.larioja.org/v1/datos-calculo-riego?";
                sURL += "estacion=" + idEstacion.ToString();
                sURL += "&fechaInicio=" + desdeFecha.ToString("yyyy-MM-dd");
                sURL += "&fechaFin=" + hastaFecha.ToString("yyyy-MM-dd");
                WebClient wc = new System.Net.WebClient();
                string json = wc.DownloadString(sURL);
                RootApiSiar dataSiar = Newtonsoft.Json.JsonConvert.DeserializeObject<RootApiSiar>(json);
                List<DatoClimatico> lista = new List<DatoClimatico>();
                if (dataSiar?.data == null)
                    return lista;
                foreach (DatoClimaticoApiSiar dat in dataSiar.data) {
                    DatoClimatico dc = new DatoClimatico {
                        IdEstacion = int.Parse(dat.Estacion),
                        Fecha = Convert.ToDateTime(dat.Fecha),
                        Eto = dat.ET0 == "NA" ? 0 : double.Parse(dat.ET0, CultureInfo.InvariantCulture),
                        TempMedia = dat.TAirMd == "NA" ? 0 : double.Parse(dat.TAirMd, CultureInfo.InvariantCulture),
                        HumedadMedia = dat.HRMn == "NA" ? 0 : double.Parse(dat.HRMn, CultureInfo.InvariantCulture),
                        VelViento = dat.VWindMd == "NA" ? 0 : double.Parse(dat.VWindMd, CultureInfo.InvariantCulture),
                        Precipitacion = dat.PAcum == "NA" ? 0 : double.Parse(dat.PAcum, CultureInfo.InvariantCulture)
                    };
                    lista.Add(dc);
                }
                return lista;
            } catch {
                string msgErr = "Error cargando datos climáticos para parametros.\n";
                msgErr += "Desde Fecha: " + desdeFecha.ToShortDateString() + "\n";
                msgErr += "Hasta Fecha: " + hastaFecha.ToShortDateString() + "\n";
                msgErr += "Estación: " + idEstacion.ToString() + "\n";
                throw new Exception(msgErr);
            }
        }

        public static void ParcelaSueloSave(int idParcelaInt, int idHorizonte, double limo, double arcilla, double arena, double matOrg, double eleGru, double prof) {
            try {
                Database db = DB.ConexionOptiaqua;
                ParcelaSuelo ps = new ParcelaSuelo {
                    IdParcelaInt = idParcelaInt,
                    IdHorizonte = idHorizonte,
                    Limo = limo,
                    Arcilla = arcilla,
                    Arena = arena,
                    MateriaOrganica = matOrg,
                    ElementosGruesos = eleGru,
                    ProfuncidadCM = prof
                };
                db.Save(ps);
            } catch (Exception ex) {
                throw new Exception("Error dando de alta horizonte.\n" + ex.Message);
            }
        }

        /// <summary>
        /// UnidadCultivoCultivoTemporadaSave.
        /// </summary>
        /// <param name="IdUnidadCultivo">IdUnidadCultivo<see cref="string"/>.</param>
        /// <param name="idTemporada">idTemporada<see cref="string"/>.</param>
        /// <param name="idCultivo">idCultivo<see cref="int"/>.</param>
        /// <param name="idRegante">idRegante<see cref="int"/>.</param>
        /// <param name="idTipoRiego">idTipoRiego<see cref="int"/>.</param>
        /// <param name="fechaSiembra">fechaSiembra<see cref="string"/>.</param>
        public static void UnidadCultivoCultivoTemporadaSave(string IdUnidadCultivo, string idTemporada, int idCultivo, int idRegante, int idTipoRiego, string fechaSiembra) {
            Database db = null;
            try {
                db = DB.ConexionOptiaqua;
                db.BeginTransaction();
                if (DateTime.TryParse(fechaSiembra, out DateTime fs) == false) {
                    throw new Exception("Error. La fecha de siembra no es correcta. ");
                }

                // validar Unidad de cultivo                
                if (db.Exists<UnidadCultivo>(IdUnidadCultivo) == false) {
                    throw new Exception("Error. No existe la unida de cultivo indicada.");
                }

                // validar Cultivo
                if (db.Exists<Cultivo>(idCultivo) == false) {
                    throw new Exception("Error. No existe el cultivo indicado. ");
                }

                // validar Regante
                if (db.Exists<Regante>(idRegante) == false) {
                    throw new Exception("Error. No existe el Regante indicado. ");
                }

                // validar TipoRiego
                if (db.Exists<RiegoTipo>(idTipoRiego) == false) {
                    throw new Exception("Error. No existe el tipo de Riego indicado.");
                }

                //Si existe, se elimina
                db.Execute(" delete from UnidadCultivoCultivo where IdUnidadCultivo=@0 and IdTemporada=@1 and IdCultivo=@2 ", IdUnidadCultivo, idTemporada, idCultivo);

                // Crear Registro Parcelas Cultivo
                UnidadCultivoCultivo uniCulCul = new UnidadCultivoCultivo {
                    IdUnidadCultivo = IdUnidadCultivo,
                    IdCultivo = idCultivo,
                    IdRegante = idRegante,
                    IdTemporada = idTemporada,
                    IdTipoRiego = idTipoRiego,
                    Pluviometria = PluviometriaTipica(idTipoRiego)
                };
                db.Insert(uniCulCul);

                // Leer Cultivo Etapas de IdCultivo
                List<CultivoEtapas> listaCF = db.Fetch<CultivoEtapas>("Select * from CultivoEtapas Where IdCultivo=@0", idCultivo);
                if (listaCF.Count == 0) {
                    throw new Exception("Error. No existe una definición de las Etapas para el cultivo indicado.");
                }

                DateTime fechaEtapa = fs;
                foreach (CultivoEtapas cf in listaCF) {
                    UnidadCultivoCultivoEtapas pcf = new UnidadCultivoCultivoEtapas {
                        IdUnidadCultivo = uniCulCul.IdUnidadCultivo,
                        IdTemporada = uniCulCul.IdTemporada,
                        IdEtapaCultivo = cf.OrdenEtapa,
                        Etapa = cf.Etapa,
                        FechaInicioEtapa = fechaEtapa
                    };
                    fechaEtapa = fechaEtapa.AddDays(cf.DuracionDiasEtapa);
                    pcf.FechaInicioEtapaConfirmada = null;
                    pcf.DefinicionPorDias = cf.DefinicionPorDias;
                    pcf.KcInicial = cf.KcInicial;
                    pcf.KcFinal = cf.KcFinal;
                    pcf.CobInicial = cf.CobInicial;
                    pcf.CobFinal = cf.CobFinal;
                    pcf.FactorDeAgotamiento = cf.FactorAgotamiento;
                    db.Insert(pcf);
                }

                db.CompleteTransaction();
                return;
            } catch (Exception ex) {
                db.AbortTransaction();
                throw new Exception("Error. No existe una definición de las Etapas para el cultivo indicado." + ex.Message);
            }
        }

        /// <summary>
        /// NParcelas.
        /// </summary>
        /// <param name="idUnidadCultivo">idUnidadCultivo<see cref="string"/>.</param>
        /// <param name="idTemporada">idTemporada<see cref="string"/>.</param>
        /// <returns><see cref="int?"/>.</returns>
        public static int? NParcelas(string idUnidadCultivo, string idTemporada) {
            Database db = DB.ConexionOptiaqua;
            string sql = "SELECT COUNT(IdParcelaInt) AS NParcelas FROM dbo.UnidadCultivoParcela GROUP BY IdUnidadCultivo, IdTemporada ";
            sql += " HAVING IdUnidadCultivo=@0 AND IdTemporada=@1";
            return db.SingleOrDefault<int?>(sql, idUnidadCultivo, idTemporada);
        }

        /// <summary>
        /// ObtenerMunicicioParaje.
        /// </summary>
        /// <param name="idTemporada">idTemporada<see cref="string"/>.</param>
        /// <param name="idUnidadCultivo">idUnidadCultivo<see cref="string"/>.</param>
        /// <param name="provincias">.</param>
        /// <param name="municipios">municipios<see cref="string"/>.</param>
        /// <param name="parajes">parajes<see cref="string"/>.</param>
        public static void ObtenerMunicicioParaje(string idTemporada, string idUnidadCultivo, out string provincias, out string municipios, out string parajes) {
            Database db = DB.ConexionOptiaqua;
            string sql = "Select Provincia,Municipio, Paraje from UnidadCultivoParaje where idTemporada=@0 and IdUnidadCultivo=@1";
            List<ProvinciaMunicipioParaje> lMunicipioProvinciaParaje = db.Fetch<ProvinciaMunicipioParaje>(sql, idTemporada, idUnidadCultivo);
            IEnumerable<string> lmunicicipos = lMunicipioProvinciaParaje.Select(x => x.Municipio).Distinct();
            IEnumerable<string> lParajes = lMunicipioProvinciaParaje.Select(x => x.Paraje).Distinct();
            IEnumerable<string> lProvincias = lMunicipioProvinciaParaje.Select(x => x.Provincia).Distinct();
            municipios = string.Join("#", lmunicicipos);
            parajes = string.Join("#", lParajes);
            provincias = string.Join("#", lProvincias);
        }

        /// <summary>
        /// UnidadesCultivoList.
        /// </summary>
        /// <param name="idRegante">idRegante<see cref="int"/>.</param>
        /// <param name="fecha">idTemporada<see cref="string"/>.</param>
        /// <returns><see cref="List{string}"/>.</returns>
        public static List<string> UnidadesCultivoList(int idRegante, DateTime fecha) {
            List<string> ret = new List<string>();
            try {
                if (fecha == null)
                    return new List<string>();
                List<string> lTemporadas = DB.TemporadasDeFecha(fecha);
                Database db = DB.ConexionOptiaqua;
                string sql = "Select Distinct IdUnidadCultivo from UnidadCultivoCultivo where IdRegante=@0 AND IdTemporada =@1";
                foreach (string idTemporada in lTemporadas) {
                    List<string> deTemporada = db.Fetch<string>(sql, idRegante, idTemporada);
                    ret.AddRange(deTemporada);
                }

            } catch {
                string msgErr = "No se pudo cargar lista de parcelas para los parámetros:\n";
                msgErr += "IdRegante:" + idRegante.ToString() + "\n";
                msgErr += "fecha:" + fecha.ToString() + "\n";
                throw new Exception(msgErr);
            }
            return ret;
        }

        /// <summary>
        /// UnidadCultivoList.
        /// </summary>
        /// <param name="idRegante">idRegante<see cref="int"/>.</param>
        /// <returns><see cref="List{string}"/>.</returns>
        public static List<string> UnidadCultivoList(int idRegante) {
            try {
                Database db = DB.ConexionOptiaqua;
                string sql;
                sql = "Select Distinct IdUnidadCultivo from UnidadcultivoCultivo where IdRegante=@0";
                return db.Fetch<string>(sql, idRegante);
            } catch {
                string msgErr = "No se pudo cargar lista de parcelas para los parámetros:\n";
                msgErr += "IdRegante:" + idRegante.ToString() + "\n";
                throw new Exception(msgErr);
            }
        }

        /// <summary>
        /// EtapasList.
        /// </summary>
        /// <param name="IdUnidadCultivo">IdUnidadCultivo<see cref="string"/>.</param>
        /// <param name="idTemporada">idTemporada<see cref="string"/>.</param>
        /// <returns><see cref="List{UnidadCultivoCultivoEtapas}"/>.</returns>
        public static List<UnidadCultivoCultivoEtapas> Etapas(string IdUnidadCultivo, string idTemporada) {
            if (string.IsNullOrWhiteSpace(idTemporada))
                return new List<UnidadCultivoCultivoEtapas>();
            Database db = DB.ConexionOptiaqua;
            string sql = "Select * from UnidadCultivoCultivoEtapas where IdUnidadCultivo=@0  AND IdTemporada=@1";
            List<UnidadCultivoCultivoEtapas> ret = db.Fetch<UnidadCultivoCultivoEtapas>(sql, IdUnidadCultivo, idTemporada);
            return ret;
        }

        /// <summary>
        /// FechasEtapasSave.
        /// </summary>
        /// <param name="lEtapas">lEtapas<see cref="List{UnidadCultivoCultivoEtapas}"/>.</param>
        public static void FechasEtapasSave(List<UnidadCultivoCultivoEtapas> lEtapas) {
            Database db = null;
            if (lEtapas == null || lEtapas.Count == 0) return;
            try {
                db = DB.ConexionOptiaqua;
                db.BeginTransaction();
                //Eliminar las actuales
                db.Execute(" delete from UnidadCultivoCultivoEtapas where IdUnidadCultivo=@0 and IdTemporada=@1 ", lEtapas[0].IdUnidadCultivo, lEtapas[0].IdTemporada);
                db.InsertBulk<UnidadCultivoCultivoEtapas>(lEtapas);
                db.CompleteTransaction();
            } catch (Exception) {
                if (db != null)
                    db.AbortTransaction();
            }
        }

        /// <summary>
        /// IdReganteDeUnidadCultivoTemporada.
        /// </summary>
        /// <param name="idUnidadCultivo">idUnidadCultivo<see cref="string"/>.</param>
        /// <param name="idTemporada">idTemporada<see cref="string"/>.</param>
        /// <returns><see cref="int?"/>.</returns>
        public static int? IdReganteDeUnidadCultivoTemporada(string idUnidadCultivo, string idTemporada) {
            int? ret = null;
            if (string.IsNullOrEmpty(idUnidadCultivo))
                return null;
            if (string.IsNullOrEmpty(idTemporada))
                return null;

            try {
                Database db = DB.ConexionOptiaqua;
                string sql;
                sql = "Select Distinct IdRegante from UnidadCultivoParcela where IdUnidadCultivo=@0 and idTemporada=@1 ";
                List<int?> lRet = db.Fetch<int?>(sql, idUnidadCultivo, idTemporada);
                if (lRet != null && lRet.Count > 0)
                    ret = lRet[0];
                return ret;
            } catch (Exception) {

                return null;
            }
        }

        /// <summary>
        /// IdReganteDeUnidadCultivo.
        /// </summary>
        /// <param name="idUnidadCultivo">idUnidadCultivo<see cref="string"/>.</param>
        /// <returns><see cref="int?"/>.</returns>
        public static int? IdReganteDeUnidadCultivo(string idUnidadCultivo) {
            int? ret = null;
            if (string.IsNullOrEmpty(idUnidadCultivo))
                return null;
            try {
                Database db = DB.ConexionOptiaqua;
                string sql;
                sql = "Select Distinct IdRegante from UnidadCultivo where IdUnidadCultivo=@0 ";
                List<int?> lRet = db.Fetch<int?>(sql, idUnidadCultivo);
                if (lRet != null && lRet.Count > 0)
                    ret = lRet[0];
                return ret;
            } catch (Exception) {

                return null;
            }
        }

        /// <summary>
        /// Retorna la lista de códigos de parcelas de una unidad de cultivo para la temporada indicada.
        /// </summary>
        /// <param name="IdUnidadCultivo">.</param>
        /// <param name="idTemporada">.</param>
        /// <returns>.</returns>
        public static List<int> ParcelasDeUnidadCultivo(string IdUnidadCultivo, string idTemporada) {

            Database db = DB.ConexionOptiaqua;
            string sql = "Select IdParcelaInt From UnidadCultivoParcela Where IdUnidadCultivo=@0 and IdTemporada=@1";
            List<int> ret = db.Fetch<int>(sql, IdUnidadCultivo, idTemporada);
            return ret;
        }

        /// <summary>
        /// UnidadCultivoExtensionM2.
        /// </summary>
        /// <param name="idUnidadCultivo">idUnidadCultivo<see cref="string"/>.</param>
        /// <param name="idTemporada">idTemporada<see cref="string"/>.</param>
        /// <returns><see cref="float"/>.</returns>
        public static double UnidadCultivoExtensionM2(string idUnidadCultivo, string idTemporada) {
            if (idUnidadCultivo == null || idTemporada == null)
                return 0;
            double? ret = 0;
            Database db = DB.ConexionOptiaqua;
            string sql = "Select SuperficieM2 From UnidadCultivoCultivo Where IdUnidadCultivo=@0 and IdTemporada=@1";
            ret = db.SingleOrDefault<float?>(sql, idUnidadCultivo, idTemporada);
            if (ret != null)
                return (double)ret;
            sql = " SELECT TOP(1) SuperficieM2 FROM UnidadCultivoCultivo WHERE(IdTemporada = @0) ORDER BY IdTemporada DESC";
            ret = db.SingleOrDefault<float?>(sql, idUnidadCultivo);
            if (ret != null)
                return (double)ret;

            sql = "  SELECT SUM(dbo.Parcela.SuperficieM2) AS Suma ";
            sql += " FROM dbo.UnidadCultivoParcela INNER JOIN ";
            sql += " dbo.Parcela ON dbo.UnidadCultivoParcela.IdParcelaInt = dbo.Parcela.IdParcelaInt ";
            sql += " GROUP BY dbo.UnidadCultivoParcela.IdUnidadCultivo, dbo.UnidadCultivoParcela.IdTemporada ";
            sql += " HAVING(dbo.UnidadCultivoParcela.IdUnidadCultivo =@0) AND(dbo.UnidadCultivoParcela.IdTemporada =@1)";
            ret = db.SingleOrDefault<double?>(sql, idUnidadCultivo, idTemporada);

            return ret ?? 0;
        }

        /// <summary>
        /// TemporadaDeFecha.
        /// </summary>
        /// <param name="idUC">IdUnidadCultivo.</param>
        /// <param name="fecha">fecha<see cref="DateTime"/>.</param>
        /// <returns><see cref="string"/>.</returns>
        public static string TemporadaDeFecha(string idUC, DateTime? fecha) {
            if (fecha == null)
                return DB.TemporadaActiva();
            if (string.IsNullOrWhiteSpace(idUC))
                return DB.TemporadaActiva();
            Database db = DB.ConexionOptiaqua;
            string sql = $"SELECT * FROM TemporadaDeFecha(@0,@1)";
            string ret = db.SingleOrDefault<string>(sql, idUC, fecha);
            if (ret != null) {
                Temporada t = DB.Temporada(ret);
                if (t.FechaInicial > fecha || t.FechaFinal < fecha)
                    ret = null;
            }
            return ret;
        }

        /// <summary>
        /// The TemporadasDeFecha.
        /// </summary>
        /// <param name="fecha">The fecha<see cref="DateTime"/>.</param>
        /// <returns>The <see cref="List{string}"/>.</returns>
        public static List<string> TemporadasDeFecha(DateTime fecha) {
            Database db = DB.ConexionOptiaqua;
            string strFecha = fecha.ToString();
            string sql = $"SELECT idTemporada FROM Temporada where @0>=FechaInicial AND @0<=FechaFinal";
            List<string> ret = db.Fetch<string>(sql, fecha);
            return ret;
        }

        /// <summary>
        /// PluviometriaTipica.
        /// </summary>
        /// <param name="idCultivo">idCultivo<see cref="int"/>.</param>
        /// <returns><see cref="double"/>.</returns>
        public static double PluviometriaTipica(int idCultivo) {
            Database db = DB.ConexionOptiaqua;
            string sql = "Select PluviometriaTipica from RiegoTipo where IdTipoRiego=@0";
            return db.Single<double>(sql, idCultivo);
        }

        /// <summary>
        /// ConfigLoad.
        /// </summary>
        /// <param name="parametro">parametro<see cref="string"/>.</param>
        /// <returns><see cref="string"/>.</returns>
        public static string ConfigLoad(string parametro) {
            Database db = DB.ConexionOptiaqua;
            return db.SingleOrDefaultById<Configuracion>(parametro)?.Valor;
        }

        public static DateTime? ConfigLoadDate(string parametro) {
            Database db = DB.ConexionOptiaqua;
            var valor = db.SingleOrDefaultById<Configuracion>(parametro)?.Valor;
            if (DateTime.TryParse(valor, out var ret))
                return ret;
            return null;
        }

        /// <summary>
        /// ConfigSave.
        /// </summary>
        /// <param name="parametro">parametro<see cref="string"/>.</param>
        /// <param name="valor">valor<see cref="string"/>.</param>
        public static void ConfigSave(string parametro, string valor) {
            Database db = DB.ConexionOptiaqua;
            Configuracion cfg = new Configuracion { Parametro = parametro, Valor = valor };
            db.Save(cfg);
        }

        public static int? ConfigLoadInt(string parametro) {
            Database db = DB.ConexionOptiaqua;
            var strValor = db.SingleOrDefaultById<Configuracion>(parametro)?.Valor;
            if (int.TryParse(strValor, out var ret))
                return ret;
            return null;
        }

        /// <summary>
        /// The TemporadaExists.
        /// </summary>
        /// <param name="idTemporada">The idTemporada<see cref="string"/>.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        internal static bool TemporadaExists(string idTemporada) {
            Database db = DB.ConexionOptiaqua;
            bool ret = db.Exists<Temporada>(idTemporada);
            return ret;
        }



        public class itemTemporadaUC {
            public string IdTemporada { set; get; }
            public string IdUC { set; get; }
        }
        internal static List<itemTemporadaUC> UnidadCultivosDePacela(int idParcelaInt) {
            using (var db = DB.ConexionOptiaqua) {
                var ret = db.Fetch<itemTemporadaUC>($"select IdTemporada,IdUnidadCultivo as IdUC from UnidadCultivoParcela where IdParcelaInt={idParcelaInt}");
                return ret;
            }
        }

        // toma los datos GIS de ZonasInfluenciaEMAsRegGeneral y actualiza el IdEstación de las Parcelas.
        public static void ActulizaEstacionParcelas() {
            using (var db = DB.ConexionOptiaqua) {
                var lPar = db.Fetch<Parcela>();
                foreach (var par in lPar) {
                    int idEstacion = IdEstacionDefault;
                    double lng = 0, lat = 0;
                    if (par.Latitud == null || par.Longitud == null) {
                        MapasCatastralesDatosLngLat(par, out lat, out lng);
                    } else {
                        var lngStr = par.Longitud.ToString().Replace(',', '.');
                        var latStr = par.Latitud.ToString().Replace(',', '.');
                        var sql = $" select codigoEMA from ZonasInfluenciaEMAsRegGeneral where geom.STContains( geometry::STGeomFromText('POINT({lngStr}  {latStr})', 4326)) =1";
                        idEstacion = db.SingleOrDefault<int>(sql);
                        if (idEstacion != 503)
                            idEstacion = idEstacion;
                        if (idEstacion == 0)
                            idEstacion = IdEstacionDefault;
                    }
                    var nExe = db.Execute($"update Parcela Set IdEstacion={idEstacion} where IdParcelaInt={par.IdParcelaInt}");
                }

            }
        }

        internal static int EstacionDeParcela(int idParcelaInt) {
            using (var db = DB.ConexionOptiaqua) {
                var par = db.SingleOrDefaultById<Parcela>(idParcelaInt);
                if (par != null) {
                    return par.IdEstacion ?? IdEstacionDefault;
                }
                return IdEstacionDefault;
            }
        }


        internal static int EstacionDeUC(string idUC, string idTemporada) {
            using (var db = DB.ConexionOptiaqua) {
                var lPar = DB.ParcelasDeUnidadCultivo(idUC, idTemporada);
                if (lPar.Count > 0) {
                    var par = db.SingleOrDefaultById<Parcela>(lPar[0]);
                    return par.IdEstacion ?? IdEstacionDefault;
                }
                return IdEstacionDefault;

            }
        }

        internal static bool ParcelaExits(int idPar) {
            using (var db = DB.ConexionOptiaqua) {
                return db.Exists<Parcela>(idPar);
            }
        }

        internal static double? ParcelaSuperficieM2(int idPar) {
            using (var db = DB.ConexionOptiaqua) {
                return db.Single<double?>($"select SuperficieM2 from Parcela where IdParcelaInt={idPar}");
            }
        }

        internal static bool CultivoExists(int idCultivo) {
            using (var db = DB.ConexionOptiaqua) {
                return db.Exists<Cultivo>(idCultivo);
            }
        }

        internal static bool ReganteExists(int idRegante) {
            using (var db = DB.ConexionOptiaqua) {
                return db.Exists<Regante>(idRegante);
            }
        }
    }
}
