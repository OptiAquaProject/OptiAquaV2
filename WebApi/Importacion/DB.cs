namespace DatosOptiaqua {
    using NPoco;
    using System.Collections.Generic;
    using System.Linq;

    public static partial class DB {

        internal static List<ItemMapaVersion> DatosMapasVersion() {
            Database db = ConexionOptiaqua;

            var ret = new List<ItemMapaVersion>();
            var lBase=db.Fetch<ItemMapaVersion>("SELECT IdVersion , COUNT(ID) AS nRegBase FROM dbo.MapaBase GROUP BY IdVersion");
            var lCatastral = db.Fetch<ItemMapaVersion>("SELECT IdVersion , COUNT(ID) AS nRegCatastral FROM dbo.MapaCatastral GROUP BY IdVersion");
            foreach( var item in lBase ) {
                var cat=lCatastral.FirstOrDefault(x=>x.IdVersion==item.IdVersion);
                if( cat!=null ) {
                    item.nRegCatastral=cat.nRegCatastral; 
                }
                ret.Add(item);
            }
            foreach (var item in lBase) {
                var bas = lBase.FirstOrDefault(x => x.IdVersion == item.IdVersion);
                if (bas == null) {                    
                    ret.Add(item);
                }
            }
            return ret;
        }

        public static void EliminarMapas(string idVersion) {
            Database db = ConexionOptiaqua;
            db.Execute($"delete from MapaBase where IdVersion='{idVersion}'");
            db.Execute($"delete from MapaCatastral where IdVersion='{idVersion}'");
        }
    }
}

    