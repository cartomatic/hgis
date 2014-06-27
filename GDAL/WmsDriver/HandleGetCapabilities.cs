using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Web;


using Cartomatic.Wms.WmsDriver;

using Cartomatic.Ogc.Schemas;
using Cartomatic.Ogc.Schemas.Wms.Wms_1302;

namespace HGIS.GDAL
{
    public partial class WmsDriver
    {
        /// <summary>
        /// Specialised override to provide SharpMap specific Layer section of the Capability node handling
        /// </summary>
        /// <param name="c"></param>
        protected override void GenerateLayerSection(Capability c)
        {
            
            //the layer root
            var rootL = new Layer();

            //Root layer name
            rootL.Name = ServiceDescription.Title;

            //Root layer crs
            var rootCrs = new List<string>();
            rootCrs.Add("EPSG:" + this.SRID);
            rootL.CRS = rootCrs.ToArray();

            //Get the map's / dataset's bounding box
            var bbox = GetMapBbox();


            //convert it to geographic and add to output
            rootL.EX_GeographicBoundingBox = ConvertToGeographicBoundingBox(bbox);


            //bounding box for crs
            var bboxes = new List<BoundingBox>();
            bboxes.Add(bbox);
            rootL.BoundingBox = bboxes.ToArray();


            //extract layers layers
            var layers = new List<Layer>();


            //so far this driver is designed to handle only one raster at a time - so there can only be one layer
            layers.Add(GenerateWmsLayerDescription());


            //add layers to root layer
            rootL.Layer1 = layers.ToArray();

            //and root layer to output
            c.Layer = rootL;
        }

        
        /// <summary>
        /// Generates a WmsLayer description for the map/dataset
        /// </summary>
        /// <param name="l"></param>
        /// <returns></returns>
        protected Layer GenerateWmsLayerDescription()
        {
            Layer L = new Layer();


            L.Name = this.DataSourceName;
            L.Title = this.DataSourceName;

            //No styles for manifold layers so far

            //layer bounding box
            var lbbox = GetMapBbox();
            L.BoundingBox = new BoundingBox[] { lbbox };
            L.EX_GeographicBoundingBox = ConvertToGeographicBoundingBox(lbbox);

            //queryable
            L.queryable = GetQueryable();

            return L;
        }

        /// <summary>
        /// Returns true if the datasource can be querried
        /// </summary>
        /// <returns></returns>
        private bool GetQueryable()
        {
            return false;
        }

    }
}
