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

                //extract the data source name
                this.DataSourceName = System.IO.Path.GetFileNameWithoutExtension(this.DataSource);

                //open gdal dataset
                this.GdalDataset = Gdal.Open(DataSource, Access.GA_ReadOnly);


                //work out the raster bounds
                double[] adfGeoTransform = new double[6];
                this.GdalDataset.GetGeoTransform(adfGeoTransform);

                double minx_pix = 0;
                double miny_pix = this.GdalDataset.RasterYSize;
                double maxx_pix = this.GdalDataset.RasterXSize;
                double maxy_pix = 0;

                this.MinX = adfGeoTransform[0] + adfGeoTransform[1] * minx_pix + adfGeoTransform[2] * miny_pix;
                this.MinY = adfGeoTransform[3] + adfGeoTransform[4] * minx_pix + adfGeoTransform[5] * miny_pix;

                this.MaxX = adfGeoTransform[0] + adfGeoTransform[1] * maxx_pix + adfGeoTransform[2] * maxy_pix;
                this.MaxY = adfGeoTransform[3] + adfGeoTransform[4] * maxx_pix + adfGeoTransform[5] * maxy_pix;

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
