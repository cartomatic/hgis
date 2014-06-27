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
        /// WMS utils
        /// </summary>
        private static WmsUtils wms = new WmsUtils(gdal: true);

        /// <summary>
        /// Token master
        /// </summary>
        private static TokenMaster tm = TokenMaster.FromFile(ConfigurationManager.AppSettings["token_master_settings"]);

        public async void ProcessRequest(HttpContext context)
        {
            //check the auth token
            bool tokenValid = await tm.CheckIfTokenValidAsync(context.Request);

            if (tokenValid)
            {
                //Token valid so talking directly to the tilecache

                //get the wms driver
                HGIS.GDAL.WmsDriver wmsdrv = wms.GetGdalWmsdriver(context);

                if (wmsdrv == null)
                {
                    wms.Fail(context);
                    return;
                }
                
                //Let the tilecache process the Request and use the wmsdriver if no cache is found
                var tcout = Cartomatic.MapUtils.TileCache.WmsRequestProcessor.ProcessRequest(
                    wms.GetTileCacheSettings(),
                    wms.GetTileScheme(context),
                    context.Request.Url.AbsoluteUri,
                    wmsdrv
                );

                wmsdrv.Dispose();

                //transfer the tile cache response to the response object
                context.Response.ContentType = tcout.ResponseContentType;

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
            else
            {
                //Token invalid, so requesting data from a backend WMS

                //adjust the base url and call the backend asynchronously
                var response = await Cartomatic.Utils.Http.ExecuteWebRequestAsync(
                    wms.GetCompleteBackendServiceUrl(context)
                );

                //make sure response is available
                if (response == null)
                {
                    wms.Fail(context);

                    //make sure to return as code after Fail MIGHT execute
                    return;
                }

                //get the content type
                context.Response.ContentType = response.ContentType;

                //read/write the response 
                context.Response.BinaryWrite(Cartomatic.Utils.Stream.ReadStream(response.GetResponseStream()));

                //close the response
                response.Close();
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