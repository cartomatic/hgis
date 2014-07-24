using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using System.Drawing;
using System.IO;

using System.Configuration;

namespace HGIS
{
    public partial class WmsUtils
    {

        /// <summary>
        /// Gets a driver instance that will be used to handle the request
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public Cartomatic.Wms.WmsDriver.Base GetWmsDriver(HttpContext context)
        {
            Cartomatic.Wms.WmsDriver.Base drv = null;

            switch (DriverType)
            {
                case WmsDriverType.Gdal:
                    drv = GetGdalWmsDriver(context);
                    break;

                case WmsDriverType.Manifold:
                    drv = GetManifoldWmsDriver(context);
                    break;
            }

            return drv;
        }

        /// <summary>
        /// Gets a wms driver instance that will be used to handle the Request
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public GDAL.WmsDriver GetGdalWmsDriver(HttpContext context)
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


            //Note:
            //The assumed storage hierarchy is:
            //Settings.WmsDataFolder
            //  \type
            //    \epsg
            //      \source
            //
            //the actual content may be made of real files or symbolic links (mklink)
            //
            //while the source param is the actual file name, it does not contain the extension
            //extension is provided through the FileNamePattern setting, for example FileNamePattern: '{source}_here_is_some_other_file_identification.jp2

            //work out the file path
            var source_file_path = System.IO.Path.Combine(
                Settings.WmsDataFolder,
                type + "\\" + epsg + "\\" + Settings.FileNamePattern.Replace("{source}", source) //this should make it for example topo\2180\wig100k.jp2
            );

            //make sure the file exists, otherwise just fail
            if (!System.IO.File.Exists(source_file_path))
            {
                return null;
            }

            return new GDAL.WmsDriver(
                Settings.GdalSdk,
                source_file_path,
                source,
                epsg,
                GetWmsServiceDescription(type, epsg, source)
            );
        }


        /// <summary>
        /// Gets a wms driver instance that will be used to handle the Request
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public Cartomatic.Manifold.WmsDriver GetManifoldWmsDriver(HttpContext context)
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