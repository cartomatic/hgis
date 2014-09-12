using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Cartomatic.Ogc.Schemas;
using Cartomatic.Ogc.Schemas.Wms.Wms_1302;

namespace HGIS.GDAL
{
    public partial class WmsDriver
    {
        /// <summary>
        /// converts the map's/datasource's bbox to geographic bbox
        /// </summary>
        /// <param name="bb"></param>
        /// <returns></returns>
        protected EX_GeographicBoundingBox ConvertToGeographicBoundingBox(BoundingBox bb)
        {
            //output object
            EX_GeographicBoundingBox bbox = new EX_GeographicBoundingBox();


            //coordinate projection transformation
            DotSpatial.Projections.ProjectionInfo pTo = DotSpatial.Projections.KnownCoordinateSystems.Geographic.World.WGS1984;
            DotSpatial.Projections.ProjectionInfo pFrom = null;

            pFrom = DotSpatial.Projections.ProjectionInfo.FromEpsgCode(this.SRID);

            double minx, miny, maxx, maxy;

            if (pFrom == null)
            {
                //from projection not known so just assume world coords
                minx = -180;
                miny = -90;
                maxx = 180;
                maxy = 90;
            }
            else
            {
                //projecton seems to be ok, so reproject
                //coordinate pairs
                double[] points = new double[4];

                points[0] = bb.minx;
                points[1] = bb.miny;

                points[2] = bb.maxx;
                points[3] = bb.maxy;

                double[] z = { 1 };

                DotSpatial.Projections.Reproject.ReprojectPoints(points, z, pFrom, pTo, 0, 2);

                minx = points[0];
                miny = points[1];
                maxx = points[2];
                maxy = points[3];
            }


            //transfer values
            bbox.westBoundLongitude = minx;
            bbox.southBoundLatitude = miny;
            bbox.eastBoundLongitude = maxx;
            bbox.northBoundLatitude = maxy;

            //and return the object
            return bbox;
        }
      
        /// <summary>
        /// Gets the bbox for the map / data source
        /// </summary>
        /// <returns></returns>
        protected BoundingBox GetMapBbox()
        {
            BoundingBox bb = new BoundingBox();

            bb.CRS = "EPSG:" + this.SRID;

            bb.minx = this.RasterMinX;
            bb.miny = this.RasterMinY;
            bb.maxx = this.RasterMaxX;
            bb.maxy = this.RasterMaxY;

            return bb;
        }

        /// <summary>
        /// Gets a bounding box of the data exposed by the wms service
        /// bbox can be then used to decide whether or not cache should be dumped and such
        /// </summary>
        /// <returns></returns>
        public override Cartomatic.Wms.WmsDriver.WmsBoundingBox GetBoundingBox()
        {
            //prepares the data set for further usage
            this.PrepareDriver();

            //the GDAL wms driver on init reads some raster info, so bbox is easily accessible here
            return new Cartomatic.Wms.WmsDriver.WmsBoundingBox(
                this.RasterMinX,
                this.RasterMinY,
                this.RasterMaxX,
                this.RasterMaxY
            );
        }
    }
}

