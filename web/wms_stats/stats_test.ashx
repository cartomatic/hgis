<%@ WebHandler Language="C#" Class="HGIS.Wms" %>

using System;
using System.Web;
using System.Web.SessionState;
using System.Configuration;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

using System.Threading.Tasks;

namespace HGIS
{
    /// <summary>
    /// HGIS.cartomatic.pl public WMS service handler. Uses gdal the raster handling engine
    /// </summary>
    public class Wms : IHttpHandler
    {
        /// <summary>
        /// Token master
        /// </summary>
        private static StatsMaster sm = StatsMaster.FromFile(ConfigurationManager.AppSettings["stats_master_settings"]);

        
        /// <summary>
        /// Processes the request
        /// </summary>
        /// <param name="context"></param>
        public async void ProcessRequest(HttpContext context)
        {
            sm.SaveStats(context.Request, 10);
            
            //complete the request
            context.ApplicationInstance.CompleteRequest();
        }
        

        /// <summary>
        /// reuse the handler's instance
        /// </summary>
        public bool IsReusable
        {
            get
            {
                return true;
            }
        }
    }
}