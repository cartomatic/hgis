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
        /// prepares gdal dataset
        /// </summary>
        protected void PrepareDriver()
        {
            //create mapserver object
            try
            {
                //make sure gdal is properly set up
                ConfigureGdal();

                //extract the data source name if not already set
                //the driver constructor has a 'layerName' param that can be used to explicitly specify how a layer should be named
                if (string.IsNullOrEmpty(this.DataSourceName))
                {
                    this.DataSourceName = System.IO.Path.GetFileNameWithoutExtension(this.DataSource);
                }

                //open gdal dataset
                this.GdalDataset = Gdal.Open(DataSource, Access.GA_ReadOnly);


                //work out the raster bounds
                double[] adfGeoTransform = new double[6];
                this.GdalDataset.GetGeoTransform(adfGeoTransform);

                double minx_pix = 0;
                double miny_pix = this.GdalDataset.RasterYSize;
                double maxx_pix = this.GdalDataset.RasterXSize;
                double maxy_pix = 0;

                this.RasterMinX = adfGeoTransform[0] + adfGeoTransform[1] * minx_pix + adfGeoTransform[2] * miny_pix;
                this.RasterMinY = adfGeoTransform[3] + adfGeoTransform[4] * minx_pix + adfGeoTransform[5] * miny_pix;

                this.RasterMaxX = adfGeoTransform[0] + adfGeoTransform[1] * maxx_pix + adfGeoTransform[2] * maxy_pix;
                this.RasterMaxY = adfGeoTransform[3] + adfGeoTransform[4] * maxx_pix + adfGeoTransform[5] * maxy_pix;

                this.RasterWidth = Math.Abs(this.RasterMaxX - this.RasterMinX);
                this.RasterHeight = Math.Abs(this.RasterMaxY - this.RasterMinY);

                //collect the roaster overview resolutions
                //assume the horisontal and vertical resolutions are the same (which of course may not be true in all cases, but will be ok with this ode usage) 
                Band band = GdalDataset.GetRasterBand(1);
                RasterResolutions = new List<double>();
                for (int i = 0; i < band.GetOverviewCount(); i++)
                {
                    var ov = band.GetOverview(i);

                    RasterResolutions.Add(this.RasterWidth / ov.XSize);
                }

                //driver seems to be ok so far
                this.DriverReady = true;
            }
            catch (Exception ex)
            {
                this.DriverReady = false;
                this.DriverExceptionMsg = ex.Message;
            }
        }
    }
}
