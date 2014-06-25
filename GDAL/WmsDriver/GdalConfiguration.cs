using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Xml;
using System.Collections.Generic;
using System.Text;

using OSGeo.GDAL;

namespace HGIS.GDAL
{
    public partial class WmsDriver
    {
        /// <summary>
        /// whether or not gdal has been configured
        /// </summary>
        private static bool GdalConfigured;

        /// <summary>
        /// path to the gdal stuff
        /// </summary>
        private static string GdalPath;

        /// <summary>
        /// Configures gdal;
        /// expects gdal stuff to come from http://www.gisinternals.com/sdk/ and maintain the folder structure
        /// created by the msi installers
        /// </summary>
        public static void ConfigureGdal()
        {
            if (GdalConfigured) return;


            // Prepend native path to environment path, to ensure the
            // right libs are being used.
            var path = Environment.GetEnvironmentVariable("PATH");
            path = GdalPath + ";" + Path.Combine(GdalPath, "gdalplugins") + ";" + path;
            Environment.SetEnvironmentVariable("PATH", path);

            // Set the additional GDAL environment variables.
            var gdalData = Path.Combine(GdalPath, "gdal-data");
            Environment.SetEnvironmentVariable("GDAL_DATA", gdalData);
            Gdal.SetConfigOption("GDAL_DATA", gdalData);

            var driverPath = Path.Combine(GdalPath, "gdalplugins");
            Environment.SetEnvironmentVariable("GDAL_DRIVER_PATH", driverPath);
            Gdal.SetConfigOption("GDAL_DRIVER_PATH", driverPath);

            Environment.SetEnvironmentVariable("GEOTIFF_CSV", gdalData);
            Gdal.SetConfigOption("GEOTIFF_CSV", gdalData);

            var projSharePath = Path.Combine(GdalPath, "projlib");
            Environment.SetEnvironmentVariable("PROJ_LIB", projSharePath);
            Gdal.SetConfigOption("PROJ_LIB", projSharePath);


            // Register drivers
            Gdal.AllRegister();
            GdalConfigured = true;

        }
    }
}
