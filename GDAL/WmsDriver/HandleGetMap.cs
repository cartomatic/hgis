using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Collections.Generic;
using System.Text;

using Cartomatic.Wms.WmsDriver;

namespace HGIS.GDAL
{
    public partial class WmsDriver
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override WmsDriverResponse HandleGetMapDriverSpecific()
        {
            WmsDriverResponse output = new WmsDriverResponse();

            bool ignoreCase = GetIgnoreCase();

            //Parse map size - this should be ok here as the calling generic method shouold have checked it already
            int width = int.Parse(GetParam("WIDTH"));
            int height = int.Parse(GetParam("HEIGHT"));

            if (GetParam("CRS") != "EPSG:" + this.SRID)
            {
                output = HandleWmsException(WmsExceptionCode.InvalidCRS, "CRS not supported");
                return output;
            }



            //Set background color of map
            Color backColor = Color.White;
            if (IsTransparent())
            {
                backColor = Color.Transparent;
            }
            else if (GetParam("BGCOLOR") != null)
            {
                try
                {
                    backColor = ColorTranslator.FromHtml(GetParam("BGCOLOR"));
                }
                catch
                {
                    output = HandleWmsException("Invalid parameter BGCOLOR");
                    return output;
                }
            }
            else
            {
                backColor = Color.White;
            }


            //Get the image format requested
            //Note: this should have been checked in the base driver so it is assumed the format will be ok!
            ImageCodecInfo imageEncoder = GetEncoderInfo(GetParam("FORMAT"));

            

            var bbox = ParseBBOX(GetParam("bbox"), GetParam("version"), this.SRID);
            if (bbox == null)
            {
                output = HandleWmsException("Invalid parameter BBOX");
                return output;
            }


            //Note:
            //gdal WMS driver does not support styles so far as there is no such concept within gdal
            string[] styles = GetParam("styles").Split(',');

            //Some clients will Request an emtpy style per each layer
            foreach (var s in styles)
            {
                if (!string.IsNullOrEmpty(s))
                {
                    output = HandleWmsException("Invalid parameter STYLES: " + styles);
                    return output;
                }
            }


            //since got here, can start preparing for rendering

            //Extract layers
            string[] inLayers = GetParam("LAYERS").Split(',');

            //if too many layers were requested fail
            if (inLayers.Length > ServiceDescription.LayerLimit)
            {
                output = HandleWmsException(WmsExceptionCode.OperationNotSupported, "Too many layers requested");
                return output;
            }


            //first extract all the current map layer names and at the same time turn them off
            List<string> mapLayers = new List<string>();

            //also add the root layer name to the layers list
            //as the map (root layer) can also be requested
            mapLayers.Add(this.ServiceDescription.Title);


            //add the served file name as well - this is the main name of the layer
            mapLayers.Add(this.DataSourceName);

            //now make sure the requested layer is valid
            foreach (var l in inLayers)
            {
                if (!mapLayers.Exists(s => s == l))
                {
                    output = HandleWmsException(WmsExceptionCode.LayerNotDefined, "Unknown layer '" + l + "'");
                    return output;
                }
            }


            //render image
            output.ResponseContentType = imageEncoder.MimeType;
            output.ResponseBinary = Render(bbox, width, height, imageEncoder, backColor);
            output.HasData = true;

            return output;
        }

    }
}
