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
        /// paints a watermark over the incoming bitmap and writes it to the response
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        public void ApplyWatermark(byte[] b, string contentType, HttpContext context)
        {
            //make sure to only apply the watermark if the watermark bitmap is available
            if (Watermark != null)
            {
                context.Response.BinaryWrite(
                    ApplyWatermark(Cartomatic.Utils.Drawing.BitmapFromByteArr(b), contentType)
                );
            }
            else
            {
                //Watermark bitmap path has not been defined, is invalid, or the bitmap could not be read
                context.Response.BinaryWrite(b);
            }
        }

        /// <summary>
        /// paints a watermark over the incoming bitmap and writes it to the response
        /// </summary>
        /// <param name="b"></param>
        /// <param name="contentType"></param>
        /// <param name="context"></param>
        public void ApplyWatermark(string b, string contentType, HttpContext context)
        {
            //make sure to only apply the watermark if the watermark bitmap is available
            if (Watermark != null)
            {
                context.Response.BinaryWrite(
                    ApplyWatermark(new Bitmap(b), contentType)
                );
            }
            else
            {
                //Watermark bitmap path has not been defined, is invalid, or the bitmap could not be read
                context.Response.WriteFile(b, false); //this should be same as transmit file
            }
        }

        /// <summary>
        /// paints a watermark over the incoming bitmap
        /// </summary>
        /// <param name="b"></param>
        /// <param name="contentType"></param>
        /// <returns></returns>
        private byte[] ApplyWatermark(Bitmap b, string contentType)
        {
            byte[] output = null;

            //first paint the watermark
            using (var g = Graphics.FromImage(b))
            {
                var w = new Bitmap(Watermark);
                g.DrawImage(
                    w,
                    new Rectangle(0, 0, w.Width, w.Height), //source
                    new Rectangle(0, 0, b.Width, b.Height), //destination
                    GraphicsUnit.Pixel
                );
                w.Dispose();
            }

            //when ready convert the image to byte arr
            using (var ms = new MemoryStream())
            {
                b.Save(ms, Cartomatic.Wms.WmsDriver.Base.GetEncoderInfo(contentType), null);
                output = ms.ToArray();
            }

            return output;
        }
    }
}