using DatosOptiaqua;
using NetTopologySuite.IO;
using NPoco;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeoPackageReaderFW {
    internal class ImportMapas {


        public static Database DBMapa(string sqliteFileName) {
            string cnnStr = $"Data Source={sqliteFileName};Version=3;";
            return new Database(cnnStr, DatabaseType.SQLite, System.Data.SQLite.SQLiteFactory.Instance);

        }

        public static string ImportarMapaBase(string fileName, string idVersion) {
            try {
                var dbOptiaqua = DB.ConexionOptiaqua;
                var dbMapa = DBMapa(fileName);
                var srsid = dbMapa.Single<int?>("SELECT srs_id FROM gpkg_contents");
                if (srsid == null || srsid != 4326) {
                    return "El mapa base no parece estar en la proyección 4326";
                }
                var lMapa = dbMapa.Fetch<MapaBaseSqlitePoco>(@" SELECT * FROM [Mapa Base] ");
                var reader = new GeoPackageGeoReader();
                foreach (var mapa in lMapa) {

                    var geom2 = reader.Read(mapa.Geom);
                    var poligono = geom2.ToString();

                    var nMapa = new MapaBaseSqlserverPoco {
                        ID = mapa.ID,
                        IdVersion = idVersion,
                        CLASIFICAC = mapa.CLASIFICAC,
                        COORD_X = mapa.COORD_X,
                        COORD_Y = mapa.COORD_Y,
                        Cuenca = mapa.Cuenca,
                        GEOFORMA = mapa.GEOFORMA,
                        Geom = poligono,
                        HS_ARCILLA_Porc = mapa.HS_ARCILLA_Porc,
                        HS_ARENA_Porc = mapa.HS_ARENA_Porc,
                        HS_CALIZAA_g_Kg = mapa.HS_CALIZAA_g_Kg,
                        HS_CARBONA_g_Kg = mapa.HS_CARBONA_g_Kg,
                        HS_CE_dS_m = mapa.HS_CE_dS_m,
                        HS_CIC_meq100g = mapa.HS_CIC_meq100g,
                        HS_EGRUESO_Porc = mapa.HS_EGRUESO_Porc,
                        HS_LIMO_Porc = mapa.HS_LIMO_Porc,
                        HS_MATORG_Porc = mapa.HS_LIMO_Porc,
                        HS_PH = mapa.HS_PH,
                        HS_PROF_cm = mapa.HS_PROF_cm,
                        HS_TEXTURA = mapa.HS_TEXTURA,
                        ID_INTER_PROSP_RELIEVE = mapa.ID_INTER_PROSP_RELIEVE,
                        Litologia = mapa.Litologia,
                        PROF_EFECTIVA_cm = mapa.PROF_EFECTIVA_cm,
                        Proy_GEODE = mapa.Proy_GEODE,
                        SC_ARCILLA_Porc = mapa.SC_ARCILLA_Porc,
                        SC_ARENA_Porc = mapa.SC_ARENA_Porc,
                        SC_CALIZAA_g_Kg = mapa.SC_CALIZAA_g_Kg,
                        SC_CARBONA_g_Kg = mapa.SC_CARBONA_g_Kg,
                        SC_CE_dS_m = mapa.SC_CE_dS_m,
                        SC_CIC_meq_100g = mapa.SC_CIC_meq_100g,
                        SC_EGRUESO_Porc = mapa.SC_EGRUESO_Porc,
                        SC_ESPESOR_cm = mapa.SC_ESPESOR_cm,
                        SC_LIMO_Porc = mapa.SC_LIMO_Porc,
                        SC_MATORG_Porc = mapa.SC_MATORG_Porc,
                        SC_PH = mapa.SC_PH,
                        SC_TEXTURA = mapa.SC_TEXTURA,
                        SUELO1 = mapa.SUELO1,
                        SUELO2 = mapa.SUELO2,
                        SUELO3 = mapa.SUELO3
                    };
                    dbOptiaqua.Insert(nMapa);
                    //var sql = $"update MapaBase set geom=geometry::STGeomFromText('{poligono}', 4326) where ID={mapa.ID} and IdVersion='{idVersion}';";
                    var sql = "update MapaBase SET Geom.STSrid = 4326; ";
                    dbOptiaqua.Execute(sql);
                }
                return "";
            } catch (Exception ex) {
                return ex.Message;
            }
        }

        public static string ImportarMapaCatastral(string fileName, string idVersion) {
            try {
                var dbOptiaqua = DB.ConexionOptiaqua;
                var dbMapa = DBMapa(fileName);
                var srsid = dbMapa.Single<int?>("SELECT srs_id FROM gpkg_contents");
                if (srsid == null || srsid != 4326) {
                    return "El mapa no parece estar en la proyección 4326";
                }
                var lMapa = dbMapa.Fetch<MapaCatastralSqlitePoco>(@" SELECT * FROM [Mapa Catastral_Proyeccion_WGS84]");
                var reader = new GeoPackageGeoReader();
                var lIns = new List<MapaCatastralSqlserverPoco>();                
                foreach (var mapa in lMapa) {
                    var geom2 = reader.Read(mapa.GEOM);
                    var poligono = geom2.ToString();

                    var nMapa = new MapaCatastralSqlserverPoco {
                        ID = mapa.fid,
                        IdVersion = idVersion,
                        GEOM = poligono,
                        Latitud = mapa.Latitud,
                        Longitud = mapa.Longitud,
                        FECHA_MUESTRA = mapa.FECHA_MUESTRA,
                        HS_ARENA_Porc = mapa.HS_ARENA_Porc,
                        HS_EGRUESO_Porc = mapa.HS_EGRUESO_Porc,
                        HS_ARCILLA_Porc = mapa.HS_ARCILLA_Porc,
                        HS_LIMO_Porc = mapa.HS_LIMO_Porc,
                        HS_MATORG_Porc = mapa.HS_MATORG_Porc,
                        HS_PH = mapa.HS_PH,
                        HS_PROF_cm = mapa.HS_PROF_cm,
                        HS_TEXTURA = mapa.HS_TEXTURA,
                        ID_MUESTRA = mapa.ID_MUESTRA,
                        MUNICIPIO = mapa.MUNICIPIO,
                        OBSERVACIONES = mapa.OBSERVACIONES,
                        ORIGEN_MUESTRA = mapa.ORIGEN_MUESTRA,
                        PARCELA = mapa.PARCELA,
                        POLIGONO = mapa.POLIGONO,
                        PROF_EFECTIVA_cm = mapa.PROF_EFECTIVA_cm,
                        PROVINCIA = mapa.PROVINCIA,
                        REF_CATAST = mapa.REF_CATAST
                    };
                    lIns.Add(nMapa);
                }
                dbOptiaqua.InsertBatch<MapaCatastralSqlserverPoco>(lIns);
                var sql = "update MapaBase SET Geom.STSrid = 4326; ";
                dbOptiaqua.Execute(sql);
                return "";
            } catch (Exception ex) {
                return ex.Message;
            }
        }

    }
}
