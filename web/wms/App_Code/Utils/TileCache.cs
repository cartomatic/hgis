using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using System.Drawing;
using System.IO;

namespace HGIS
{
    public partial class WmsUtils
    {
        public Cartomatic.MapUtils.TileCache.Settings GetTileCacheSettings()
        {
            return Tcs;
        }

        /// <summary>
        /// Gets the safe (cloned) tile scheme approprite for the given Request
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public Cartomatic.MapUtils.Tiling.TileScheme GetTileScheme(HttpContext context)
        {
            Cartomatic.MapUtils.Tiling.TileScheme output = null;

            //tile scheme depends on the requested data - so far it depends on the EPSG
            //epsg value is read off the wms service path url/type/epsg/source
            //
            //Note:
            //In the future it may sensible to allow specifying appropriate tile chache scheme
            //through a param
            var epsg = GetEpsg(context);
            try
            {
                output = TileSchemes.Find(ts => ts.Epsg == "EPSG:" + epsg);
            }
            catch { } //silently fail


            //return a safe copy of the tile scheme, so it does not get screwed when being used for calculations by different threads
            if (output != null)
            {
                return output.Clone();
            }
            else
            {
                return output;
            }
        }
    }
}