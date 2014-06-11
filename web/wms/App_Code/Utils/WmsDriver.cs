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
        /// <exception cref=""></exception>
        /// <returns></returns>
        public Cartomatic.Manifold.WmsDriver GetWmsdriver(HttpContext context)
        {
            //get the ma file location
            var source = context.Request.Params["source"];

            //fail if the map file does not 'fit'
            if (!Array.Exists(AllowedSources, src => src == source))
            {
                return null;
            }
            
            return new Cartomatic.Manifold.WmsDriver(
                System.IO.Path.Combine(Settings.WmsDataFolder, source + ".map"),
                Settings.MapComponent,
                ServiceDescription
            );
        }
    }
}