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
        /// Setings object - used for easy deserialisation of common (shared) wms settings
        /// </summary>
        private class WmsSettings
        {
            public WmsSettings() { }

            /// <summary>
            /// url of the public facing endpoint; used to generate a valid Caps doc when WMS is proxied
            /// </summary>
            public string PublicAccessUrl { get; set; }

            /// <summary>
            /// url of the backend service
            /// </summary>
            public string BackendServiceUrl { get; set; }

            /// <summary>
            /// data folder for the particular wms engine
            /// </summary>
            public string WmsDataFolder { get; set; }

            /// <summary>
            /// Name of the map component used to render the data
            /// </summary>
            public string MapComponent { get; set; }

            /// <summary>
            /// Watermark file path
            /// </summary>
            public string Watermark { get; set; }
        }

        /// <summary>
        /// Wms ServiceDescription
        /// </summary>
        private WmsSettings Settings { get; set; }


        /// <summary>
        /// Whether or not the WMS service has already been prepared
        /// </summary>
        private bool ServicePrepared { get; set; }

        /// <summary>
        /// WMS service description
        /// </summary>
        private Cartomatic.Wms.WmsDriver.WmsServiceDescription ServiceDescription { get; set; }

        /// <summary>
        /// Tile cache ServiceDescription
        /// </summary>
        private Cartomatic.MapUtils.TileCache.Settings Tcs { get; set; }

        /// <summary>
        /// configured tile schemes 
        /// </summary>
        private List<Cartomatic.MapUtils.Tiling.TileScheme> TileSchemes { get; set; }

        /// <summary>
        /// Watermark bitmap data; will not be applied if could not be read
        /// Using memory stream not a bitmap instance, so when painting a watermark no clashes occur
        /// </summary>
        private System.IO.MemoryStream Watermark { get; set; }

        /// <summary>
        /// Allowed data sources; basically a list of map file names (epsg codes) that provide the data
        /// So far assuming 2180, will be accessed the most often
        /// </summary>
        private static string[] AllowedSources = new string[] { "2180", "3857", "4326" };


    }
}