<%@ WebHandler Language="C#" Class="HGIS.Wms" %>

using System;
using System.Web;
using System.Configuration;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace HGIS
{
    /// <summary>
    /// HGIS.cartomatic.pl internal WMS service handler. Uses manifold gis as the raster handling engine.
    /// 
    /// Meant to be a lower performance endpoint serving the data for the public unauthenticated users;
    /// in general should not be visible to the public and should be asynchronously proxied in order to enable
    /// the public endpoint to act as a higher performance service for the authenticated users as well as the lower performance service for the public users at the same time
    /// </summary>
    public class Wms : IHttpHandler
    {
        /// <summary>
        /// WMS utils
        /// </summary>
        private static WmsUtils wms = new WmsUtils(WmsUtils.WmsDriverType.Manifold);

        /// <summary>
        /// Stats master
        /// </summary>
        private static StatsMaster sm = StatsMaster.FromFile(ConfigurationManager.AppSettings["stats_master_settings"]);
        
        public void ProcessRequest(HttpContext context)
        {
            //get the wms driver
            //and make sure it was possible to create it (wms driver will not be created if source does not meet the 'allowed sources' criteria
            //in such case just fail
            var wmsdrv = wms.GetWmsDriver(context);
            if (wmsdrv == null)
            {
                wms.Fail(context);
                return;
            }
            
            //Let the tilecache process the Request
            var tcout = Cartomatic.MapUtils.TileCache.WmsRequestProcessor.ProcessRequest(
                wms.GetTileCacheSettings(),
                wms.GetTileScheme(context),
                context.Request.Url.AbsoluteUri,
                wmsdrv
            );

            //transfer the tile cache response to the response object
            context.Response.ContentType = tcout.ResponseContentType;

            if (tcout.HasFile)
            {
                //let the watermark stamper decide how to return the data
                wms.ApplyWatermark(tcout.FilePath, tcout.ResponseContentType, context);

                //log the stats - skip the non image responses though
                if (tcout.ResponseContentType.IndexOf("image", StringComparison.InvariantCultureIgnoreCase) > -1)
                {
                    sm.SaveStats(context.Request, true, tcout.FilePath);
                }
            }
            else if (tcout.HasData)
            {
                //let the watermark stamper decide how to return the data
                wms.ApplyWatermark(tcout.ResponseBinary, tcout.ResponseContentType, context);
                
                //log the stats - skip the non image responses though
                if (tcout.ResponseContentType.IndexOf("image", StringComparison.InvariantCultureIgnoreCase) > -1)
                {
                    sm.SaveStats(context.Request, false, tcout.ResponseBinary.Length);
                }
            }
            else //otherwise write returned text
            {
                context.Response.Write(tcout.ResponseText);
            }

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