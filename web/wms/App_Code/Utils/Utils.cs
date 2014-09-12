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
        /// Gets the epsg of the served content, for example 2180, 3857, etc.
        /// used for working out the path of the datasource file
        /// rewritten into 'epsg' param from url/type/epsg/source
        /// 
        /// Note: extracts the epsg off the path - since the wms endpoint does not do the reprojections, it will fail if the epsg specified through the crs param is different
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private string GetEpsg(HttpContext context)
        {
            return context.Request.Params["epsg"];
        }

        /// <summary>
        /// Gets the source of the served content, for example wig100k, kdr, etc.
        /// used for working out the path of the datasource file
        /// rewritten into 'source' param from url/type/epsg/source
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private string GetSource(HttpContext context)
        {
            return context.Request.Params["source"]; ;
        }

        /// <summary>
        /// Gets the type of the served content, for example topo, city, etc.
        /// used for working out the path of the datasource file
        /// rewritten into 'type' param from url/type/epsg/source
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private string GetType(HttpContext context)
        {
            return context.Request.Params["type"]; ;
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
            //Note: backend service does not use rewrite, so params rewritten from the public service path can be just glued in
            return Settings.BackendServiceUrl + "?" + string.Join("&", context.Request.QueryString);
        }

        /// <summary>
        /// Gets the wms service description appropriate for the current request
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private Cartomatic.Wms.WmsDriver.WmsServiceDescription GetWmsServiceDescription(HttpContext context)
        {
            //get the path elements
            var epsg = GetEpsg(context);
            var source = GetSource(context);
            var type = GetType(context);

            return GetWmsServiceDescription(type, epsg, source);
        }

        /// <summary>
        /// Gets the wms service description appropriate for the requested type, epsg, source 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="epsg"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        private Cartomatic.Wms.WmsDriver.WmsServiceDescription GetWmsServiceDescription(string type, string epsg, string source)
        {
            var key = type + "_" + epsg + "_" + source;

            Cartomatic.Wms.WmsDriver.WmsServiceDescription sd = null;
            if (!RuntimeServiceDescriptions.ContainsKey(key))
            {
                //extract service descriptions and create its safe copy
                sd = BaseServiceDescriptions["main"].Clone();;
                
                //if there is an additional service description available
                //extract it an merge with the main object
                if (BaseServiceDescriptions.ContainsKey(source))
                {
                    var sd_source = BaseServiceDescriptions[source];
                    sd.Merge(sd_source, new string[] { "Abstract" }); //merge the abstracts so it actually is made of the main and source abstract strings!
                }


                //adjust the base service url so it is paramless and valid for both - public facing and internal services
                sd.PublicAccessURL = GetPublicServiceUrl(type, epsg, source);

                //and save for further referencce
                RuntimeServiceDescriptions[key] = sd;
            }
            else
            {
                sd = RuntimeServiceDescriptions[key];
            }

            return sd;
        }

        /// <summary>
        /// Gets a public service address
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public string GetPublicServiceUrl(HttpContext context)
        {
            //get the path elements
            var epsg = GetEpsg(context);
            var source = GetSource(context);
            var type = GetType(context);

            return GetPublicServiceUrl(type, epsg, source);
        }

        /// <summary>
        /// Gets a public service address
        /// </summary>
        /// <param name="type"></param>
        /// <param name="epsg"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public string GetPublicServiceUrl(string type, string epsg, string source)
        {
            //Note:
            //in order to unify the wms access paths between different potential endpoints (manifold, gdal, etc)
            //the request to the public facing wms is always in a form of
            //base_url/type/epsg/source, for example base_url/topo/2180/wig100k
            //and gets rewritten to
            //base_url/?type=type&epsg=epsg&source=source, for example base_url/?type=topo&epsg=2180&source=wig100k
            //
            //This is so some clients (hello geoportal 2... ;) that seem to not like the params in the GetCapabilities can consume the service properly

            return Settings.PublicAccessUrl + type + "/" + epsg + "/" + source;
        }

    }
}