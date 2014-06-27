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
        public Cartomatic.Manifold.WmsDriver GetManifoldWmsdriver(HttpContext context)
        {
            //get the path elements
            var epsg = GetEpsg(context);
            var source = GetSource(context);
            var type = GetType(context);

            //fail if the map file does not 'fit'
            if (!Array.Exists(AllowedCs, cs => cs == epsg))
            {
                return null;
            }


            //TODO
            //work out the manifold map file location based on the epsg, source and type
            //this requires some changes in the manifold map files organisation
            //See the notes in the Utils/GetPublicServiceUrl

            throw new Exception("work in progress - need to rework the map files organisation.");
            

            return new Cartomatic.Manifold.WmsDriver(
                System.IO.Path.Combine(Settings.WmsDataFolder, source + ".map"),
                Settings.MapComponent,
                GetWmsServiceDescription(type, epsg, source)
            );
        }
    }
}