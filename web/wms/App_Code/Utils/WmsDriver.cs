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
        /// <summary>
        /// Gets a wms driver instance that will be used to handle the Request
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public Cartomatic.Manifold.WmsDriver GetWmsdriver(HttpContext context)
        {
            return new Cartomatic.Manifold.WmsDriver(
                GetMapFileLocation(context),
                GetMapComponent(context),
                ServiceDescription
            );
        }

        /// <summary>
        /// Returns a full path to a map file that should be loaded by the mapserver
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private string GetMapFileLocation(HttpContext context)
        {
            //first get the map file and map component
            var source = context.Request.Params["source"];

            //fail if the map file does not 'fit'
            if (!Array.Exists(AllowedSources, src => src == source))
            {
                Fail(context);
            }

            //fail if the map file has not been supplied
            if (string.IsNullOrEmpty(source))
            {
                Fail(context);
            }

            return System.IO.Path.Combine(Settings.WmsDataFolder, source + ".map");
        }

        /// <summary>
        /// Gets map component that will be used by the mapserver to render maps
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private string GetMapComponent(HttpContext context)
        {
            //try to extract map component to be used with this Request
            //this is so, the map component can be overwritten if needed.
            string mapComp = context.Request.Params["map"];

            //if the param has not been supplied default to 'wms.hgis.cartomatic.pl'
            if (string.IsNullOrEmpty(mapComp))
            {
                mapComp = Settings.MapComponent;
            }
            return mapComp;
        }
    }
}