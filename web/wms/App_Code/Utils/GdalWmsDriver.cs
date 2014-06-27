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
        /// Gets a wms driver instance that will be used to handle the Request
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public GDAL.WmsDriver GetGdalWmsdriver(HttpContext context)
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
            //the actual content may be made of real files or symbolic links
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
    }
}