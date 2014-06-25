using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Drawing;
using System.Drawing.Imaging;

using System.IO;

using OSGeo.GDAL;

using Cartomatic.Wms.WmsDriver;

namespace HGIS.GDAL
{
    public partial class WmsDriver
    {
        protected byte[] Render(WmsBoundingBox bbox, int width, int height, ImageCodecInfo imageEncoder, Color backColor)
        {
            byte[] output = null;


            //prepare the output bitmap
            var outB = new Bitmap(width, height, PixelFormat.Format32bppRgb);


            //do not render the map if the bbox is outside of the map's bbox
            //if any of the below tests yield true, bbox does not intersect with the raster bounds
            //if (
            //    !(
            //        bbox.MaxX < MinX ||
            //        bbox.MinX > MaxX ||
            //        bbox.MinY > MaxY ||
            //        bbox.MaxX < MinY
            //    )
            //)
            //{
                //since got here, bbox intersects with the raster bounds so need to read the data

                //first work out which overview should be used - based on the requested resolution


                //next map the bbox to pixels of the data source




                //dev - paint the 0 band for initial tests!

                BitmapData bitmapData = outB.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format32bppRgb);

                try
                {
                    int stride = bitmapData.Stride;
                    IntPtr buf = bitmapData.Scan0;

                    Band redBand = GdalDataset.GetRasterBand(1);

                    //TODO - properly handle grayscale / palletted images
                    //so far expecting a (a)rgb image!

                    Band greenBand = GdalDataset.GetRasterBand(2);
                    Band blueBand = GdalDataset.GetRasterBand(3);


                    //TODO
                    //use GetOverview(iOverview);
                    //in order to read data properly mapped - see the coord bitmap mapping above

                    //DEV
                    blueBand.ReadRaster(0, 0, width, height, buf, width, height, DataType.GDT_Byte, 4, stride);
                    greenBand.ReadRaster(0, 0, width, height, new IntPtr(buf.ToInt64() + 1), width, height, DataType.GDT_Byte, 4, stride);
                    redBand.ReadRaster(0, 0, width, height, new IntPtr(buf.ToInt64() + 2), width, height, DataType.GDT_Byte, 4, stride);

                    //TODO
                    //could read alpha too potentially???


                }
                finally
                {
                    outB.UnlockBits(bitmapData);
                }


                //TODO
                //paint transferred data over the output image!
                //
                //The read image may have different size / offset if source only partially intersectss with the bbox!!!


            //}
           

            //NOTe: this can become extremaly heavy on the cpu...
            //so app pool cpu throttling may have to be enabled...


            //if transparent, just use the out bitmap as it is already transparent
            //if not transparent, but back color set, then paint the pitmap with that color
            if (backColor != Color.Transparent)
            {
                //TODO
                var backB = new Bitmap(width, height, PixelFormat.Format24bppRgb); //use 24 bit bitmap as it is not transparent

                //paint the base image with the required back colour
                using (Graphics g = Graphics.FromImage(backB))
                {
                    //first paint the background color
                    g.FillRectangle(
                        new SolidBrush(backColor),
                        0,
                        0,
                        backB.Width,
                        backB.Height
                    );

                    //next the rendered bitmap
                    g.DrawImage(
                        outB,
                        new Rectangle(0, 0, backB.Width, backB.Height), //dest rect
                        new Rectangle(0, 0, outB.Width, outB.Height), //source rect
                        GraphicsUnit.Pixel
                    );
                }

                outB = backB;
            }


            using (var ms = new MemoryStream())
            {

                outB.Save(ms, imageEncoder, null);
                output = ms.ToArray();
            }


            return output;
        }
        
    }
}
