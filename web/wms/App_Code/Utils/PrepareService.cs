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
        /// Prepares the service
        /// </summary>
        private void PrepareService()
        {
            //make sure the work is to be done
            if (ServicePrepared) return;

            //extract the wms ServiceDescription
            Settings = Cartomatic.Utils.Serialisation.DeserializeFromJson<WmsSettings>(
                ReadFileTxt(ConfigurationManager.AppSettings["wms_settings"])
            );
            //and fix the paths so they can be expected to be absolute
            Settings.WmsDataFolder = Cartomatic.Utils.Path.SolvePath(Settings.WmsDataFolder);
            Settings.Watermark = Cartomatic.Utils.Path.SolvePath(Settings.Watermark);


            //Describe the wms service
            ServiceDescription = Cartomatic.Wms.WmsDriver.WmsServiceDescription.FromJson(
                ReadFileTxt(ConfigurationManager.AppSettings["wms_service_description"])
            );

            //Note:
            //if a desired wms PublicAccessURL is not provided, the url is worked out based on the current Request url;
            //since this is a service behind accessible through a proxy, PublicAccessURL must be adjusted appropriately
            ServiceDescription.PublicAccessURL = Settings.PublicAccessUrl;

            //read tile cache ServiceDescription
            Tcs = Cartomatic.MapUtils.TileCache.Settings.FromJson(
                ReadFileTxt(ConfigurationManager.AppSettings["tile_cache_settings"])
            );

            //read tile schemes
            TileSchemes = Cartomatic.Utils.Serialisation.DeserializeFromJson<List<Cartomatic.MapUtils.Tiling.TileScheme>>(
                ReadFileTxt(ConfigurationManager.AppSettings["tile_schemes"])
            );

            //finally read in the watermark image
            try
            {
                if (System.IO.File.Exists(Settings.Watermark))
                {
                    Watermark = new System.IO.MemoryStream(
                        System.IO.File.ReadAllBytes(Settings.Watermark)
                    );
                }
            }
            catch { } //silently fail, no watermark will be painted


            //flag the service as prepared
            ServicePrepared = true;
        }   
    }
}