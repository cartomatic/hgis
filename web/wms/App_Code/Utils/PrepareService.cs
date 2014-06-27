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
        private void PrepareService(bool gdal, bool manifold)
        {
            //make sure the work is to be done
            if (ServicePrepared) return;

            //prepare the runtime service descriptions
            RuntimeServiceDescriptions = new Dictionary<string, Cartomatic.Wms.WmsDriver.WmsServiceDescription>();

            //Describe the wms service
            BaseServiceDescriptions = Cartomatic.Utils.Serialisation.DeserializeFromJson<Dictionary<string, Cartomatic.Wms.WmsDriver.WmsServiceDescription>>(
                ReadFileTxt(ConfigurationManager.AppSettings["wms_service_description"])
            );


            //WMS driver related settings
            string settings_webconfig_key = "";
            if (manifold)
            {
                settings_webconfig_key = "manifold_wms_settings";
            }

            //GDAL WMS driver related stuff goes here!
            if (gdal)
            {
                settings_webconfig_key = "gdal_wms_settings";
            }

            //extract the wms Service settings
            Settings = Cartomatic.Utils.Serialisation.DeserializeFromJson<WmsSettings>(
                ReadFileTxt(ConfigurationManager.AppSettings[settings_webconfig_key])
            );
            //and fix the paths so they can be expected to be absolute
            Settings.WmsDataFolder = Cartomatic.Utils.Path.SolvePath(Settings.WmsDataFolder);
            Settings.Watermark = Cartomatic.Utils.Path.SolvePath(Settings.Watermark);



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