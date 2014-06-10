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
        /// Reads the txt file from the specified path - either absolute or relative
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private string ReadFileTxt(string path)
        {
            var output = string.Empty;

            try
            {
                output = System.IO.File.ReadAllText(
                    Cartomatic.Utils.Path.SolvePath(path)
                );
            }
            catch { }

            return output;
        }

        /// <summary>
        /// extracts the epsg off the request
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private string GetEpsg(HttpContext context)
        {
            //start with the >= 1.3 CRS param
            var epsg = context.Request.Params["CRS"];
            if (string.IsNullOrEmpty(epsg))
            {
                //do a < 1.3 fallback
                epsg = context.Request.Params["SRS"];
            }

            return epsg;
        }

        /// <summary>
        /// Returns the configured backend service base url
        /// </summary>
        /// <returns></returns>
        public string GetRawBackendServiceUrl()
        {
            return Settings.BackendServiceUrl;
        }


        /// <summary>
        /// returns a backend service url with the appropriate params glued in
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public string GetCompleteBackendServiceUrl(HttpContext context)
        {
            return Settings.BackendServiceUrl + "?" + string.Join("&", context.Request.QueryString);
        }

    }
}