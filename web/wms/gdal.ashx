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
    /// HGIS.cartomatic.pl public WMS service handler. Uses manifold gis as the raster handling engine
    /// </summary>
    public class Wms : IHttpHandler
    {
        /// <summary>
        /// WMS utils
        /// </summary>
        private static WmsUtils wms = new WmsUtils();


        /// <summary>
        /// Token master
        /// </summary>
        private static TokenMaster tm = TokenMaster.FromFile(ConfigurationManager.AppSettings["token_master_settings"]);
        
        
        public async void ProcessRequest(HttpContext context)
        {

            var wmsdrv = new HGIS.GDAL.WmsDriver(
                @"D:\GDAL\1.11\x64",
                //@"F:\hgisdata\rasters\wig500k_google.jp2",
                @"F:\hgisdata\rasters\wig500k_google.ecw",
                3857,
                null
            );
            
            //Let the tilecache process the Request
            var tcout = Cartomatic.MapUtils.TileCache.WmsRequestProcessor.ProcessRequest(
                wms.GetTileCacheSettings(),
                wms.GetTileScheme(context),
                context.Request.Url.AbsoluteUri,
                wmsdrv
            );

            //transfer the tile cache response to the response object
            context.Response.ContentType = tcout.ResponseContentType;

            
            //check the auth token
            bool tokenValid = await tm.CheckIfTokenValidAsync(context.Request, "t");

            if (!tokenValid)
            {
                if (tcout.HasFile)
                {
                    //let the watermark stamper decide how to return the data
                    wms.ApplyWatermark(tcout.FilePath, tcout.ResponseContentType, context);
                }
                else if (tcout.HasData)
                {
                    //let the watermark stamper decide how to return the data
                    wms.ApplyWatermark(tcout.ResponseBinary, tcout.ResponseContentType, context);
                }
                else //otherwise write returned text
                {
                    context.Response.Write(tcout.ResponseText);
                }
            }
            else
            {

                if (tcout.HasFile)
                {
                    context.Response.WriteFile(tcout.FilePath, false);
                }
                else if (tcout.HasData)
                {
                    context.Response.BinaryWrite(tcout.ResponseBinary);
                }
                else //otherwise write returned text
                {
                    context.Response.Write(tcout.ResponseText);
                }
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