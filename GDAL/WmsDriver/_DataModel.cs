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
        /// A flag indicating that the driver is ready - gdal dataset has been properly instantiated, data has been loaded and the driver is ready to do the work
        /// </summary>
        protected bool DriverReady { get; set; }

        /// <summary>
        /// gdal related exception Msg if something fails
        /// </summary>
        protected string DriverExceptionMsg { get; set; }

        /// <summary>
        /// declared srid of the data source
        /// </summary>
        protected int SRID { get; set; }

        /// <summary>
        /// Path to the data source
        /// </summary>
        public string DataSource { get; set; }

        /// <summary>
        /// Name of the file being served - also the name of the layer
        /// </summary>
        protected string DataSourceName { get; set; }


        /// <summary>
        /// Name of the root layer
        /// </summary>
        protected static string RootLayerName = "wms.hgis.cartomatic.pl";

        /// <summary>
        /// Raster MinX in the cs units
        /// </summary>
        protected double RasterMinX { get; set; }

        /// <summary>
        /// Raster MinY in the cs units
        /// </summary>
        protected double RasterMinY { get; set; }

        /// <summary>
        /// Raster MaxX in the cs units
        /// </summary>
        protected double RasterMaxX { get; set; }

        /// <summary>
        /// Raster MaxY in the cs units
        /// </summary>
        protected double RasterMaxY { get; set; }

        /// <summary>
        /// Raster width in map units
        /// </summary>
        protected double RasterWidth { get; set; }

        /// <summary>
        /// raster height in map units
        /// </summary>
        protected double RasterHeight { get; set; }

        /// <summary>
        /// List of raster overviews resolutions
        /// </summary>
        protected List<double> RasterResolutions { get; set; }

        /// <summary>
        /// GDAL data set
        /// </summary>
        protected Dataset GdalDataset { get; set; }

    }
}
