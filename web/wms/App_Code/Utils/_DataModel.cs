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
        /// Settings object - used for easy deserialisation of common (shared) wms settings
        /// </summary>
        private class WmsSettings
        {
            public WmsSettings() { }

            /// <summary>
            /// url of the public facing endpoint; used to generate a valid Caps doc when WMS is proxied
            /// </summary>
            public string PublicAccessUrl { get; set; }

            /// <summary>
            /// url of the backend service - backend service will usually be used to handle the lower importance public users
            /// </summary>
            public string BackendServiceUrl { get; set; }

            /// <summary>
            /// data folder for the particular wms engine
            /// </summary>
            public string WmsDataFolder { get; set; }

            /// <summary>
            /// Name of the manifold map component used to render the data
            /// </summary>
            public string MapComponent { get; set; }

            /// <summary>
            /// string pattern used to work out the actual file name used as the data source
            /// the usual replacable parts would be epsg, mapname and such. see the appropriate file name composition code for details
            /// applies to both manifold and gdal drivers
            /// </summary>
            public string FileNamePattern { get; set; }

            /// <summary>
            /// Location of the used gdal sdk
            /// </summary>
            public string GdalSdk { get; set; }

            /// <summary>
            /// Watermark file path
            /// </summary>
            public string Watermark { get; set; }
        }

        /// <summary>
        /// Types of the drivers that can be used with the utils
        /// </summary>
        public enum WmsDriverType
        {
            /// <summary>
            /// Undefined
            /// </summary>
            Undefined,
            
            /// <summary>
            /// Manifold WMS Driver
            /// </summary>
            Manifold,

            /// <summary>
            /// Gdal WMS Driver
            /// </summary>
            Gdal
        }

        /// <summary>
        /// Type of the driver to be used configured for the utils
        /// </summary>
        private WmsDriverType DriverType { get; set; }


        /// <summary>
        /// Whether or not the WMS service has already been prepared
        /// </summary>
        private bool ServicePrepared { get; set; }
        
        
        /// <summary>
        /// Wms service settings
        /// </summary>
        private WmsSettings Settings { get; set; }

        /// <summary>
        /// WMS service descriptions in a form of a dictionary so multiple service descriptions may be read at once.
        /// different wms drivers may use this object differently
        /// </summary>
        private Dictionary<string, Cartomatic.Wms.WmsDriver.WmsServiceDescription> BaseServiceDescriptions { get; set; }

        /// <summary>
        /// WMS service descriptions used during the runtime - each data source has a couple of possible service urls differentiated by the
        /// output coordinate system - therefore each such endpoint needs a customised PublicAccessUrl
        /// </summary>
        private Dictionary<string, Cartomatic.Wms.WmsDriver.WmsServiceDescription> RuntimeServiceDescriptions { get; set; }

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
        /// Allowed coordinate systems - if user requests something else he will be 404-oed
        /// At the time being this setting is not configurable
        /// </summary>
        private static string[] AllowedCs = new string[] { "2180", "3857", "4326" };

    }
}