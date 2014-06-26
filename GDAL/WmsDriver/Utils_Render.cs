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
        /// <summary>
        /// Renders the requested image
        /// NOTE: jp2 rendering using GDAL can become extremaly heavy on the cpu...
        /// so app pool cpu throttling may have to be enabled...
        /// in such case though of course all the stuff gonna be a bit rough
        /// </summary>
        /// <param name="bbox"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="imageEncoder"></param>
        /// <param name="backColor"></param>
        /// <returns></returns>
        protected byte[] Render(WmsBoundingBox bbox, int width, int height, ImageCodecInfo imageEncoder, Color backColor)
        {
            byte[] output = null;


            //prepare the output bitmap
            var outB = new Bitmap(width, height, PixelFormat.Format32bppRgb);
            outB.MakeTransparent();

            //do not render the map if the bbox is outside of the map's bbox
            //if any of the below tests yield true, bbox does not intersect with the raster bounds
            if (
                !(
                    bbox.MaxX < RasterMinX ||
                    bbox.MinX > RasterMaxX ||
                    bbox.MinY > RasterMaxY ||
                    bbox.MaxY < RasterMinY
                )
            )
            {
                //since got here, bbox intersects with the raster bounds so need to read the data



                //work out which overview should be used - based on the requested resolution
                //the array of resolutions is sorted as it has been collected off the raster
                var requestedRes = bbox.Width / width;
                var minDiff = Math.Abs(RasterResolutions[0] - requestedRes);

                var overviewIdx = 0;
                for (int i = 1; i < RasterResolutions.Count; i++)
                {
                    var currentDiff = Math.Abs(RasterResolutions[i] - requestedRes);

                    if (currentDiff > minDiff)
                    {
                        break;
                    }

                    overviewIdx = i;
                }

                //get the overview - need to go through a band
                var ov = GdalDataset.GetRasterBand(1).GetOverview(overviewIdx);

                //get the resolution of the overview
                var ovres = RasterResolutions[overviewIdx];
                



                //work out render area in map units and pixels
                double gdal_render_area_left = bbox.MinX < RasterMinX ? RasterMinX : bbox.MinX;
                double gdal_render_area_right = bbox.MaxX > RasterMaxX ? RasterMaxX : bbox.MaxX;
                double gdal_render_area_top = bbox.MaxY > RasterMaxY ? RasterMaxY : bbox.MaxY;
                double gdal_render_area_bottom = bbox.MinY < RasterMinY ? RasterMinY : bbox.MinY;
                
                int gdal_pix_left = (int)((gdal_render_area_left - RasterMinX) / ovres);
                int gdal_pix_top = (int)((RasterMaxY - gdal_render_area_top) / ovres);
                int gdal_pix_width = (int)((gdal_render_area_right - gdal_render_area_left) / ovres);
                int gdal_pix_heigh = (int)((gdal_render_area_top - gdal_render_area_bottom) / ovres);
                


                //bitmap to extract gdal data
                var gdalB = new Bitmap(
                    gdal_pix_width,
                    gdal_pix_heigh,
                    PixelFormat.Format32bppRgb
                );

                //lock the actual part of the bitmap that should be painted!!!!
                BitmapData bitmapData = gdalB.LockBits(
                    new Rectangle(0, 0, gdalB.Width, gdalB.Height),
                    ImageLockMode.ReadWrite,
                    PixelFormat.Format32bppRgb
                );

                try
                {
                    int stride = bitmapData.Stride;
                    IntPtr buf = bitmapData.Scan0;

                    //Notes
                    //so far expecting rgb or argb image!

                    //TODO
                    //properly handle grayscale / palletted images
                    
                    
                    
                    //int[] bandmap = null;
                    //if (GdalDataset.RasterCount == 4)
                    //{
                    //    //rgba -> bgra
                    //    bandmap = new int[] { 3, 2, 1, 4 };
                    //}
                    //else if (GdalDataset.RasterCount == 3)
                    //{
                    //    //rgb -> bgr
                    //    bandmap = new int[] { 3, 2, 1 };
                    //}
                    //else
                    //{
                    //    throw new Exception("So far hgis.Gdal wms driver so far does not support neither grayscale nor palleted rasters");
                    //}

                    ////How to read a particular overview?????
                    //GdalDataset.ReadRaster(
                    //    gdal_pix_left - gdal_margin_left,
                    //    gdal_pix_top - gdal_margin_top,
                    //    gdalB.Width,
                    //    gdalB.Height,
                    //    buf,
                    //    gdalB.Width,
                    //    gdalB.Height,
                    //    DataType.GDT_Byte,
                    //    GdalDataset.RasterCount,
                    //    bandmap,
                    //    4,
                    //    stride,
                    //    1
                    //);



                    Band redBand = GdalDataset.GetRasterBand(1).GetOverview(overviewIdx);
                    Band greenBand = GdalDataset.GetRasterBand(2).GetOverview(overviewIdx);
                    Band blueBand = GdalDataset.GetRasterBand(3).GetOverview(overviewIdx);



                    //read blue
                    blueBand.ReadRaster(
                        gdal_pix_left,
                        gdal_pix_top,
                        gdalB.Width,
                        gdalB.Height,
                        buf,
                        gdalB.Width,
                        gdalB.Height,
                        DataType.GDT_Byte,
                        4,
                        stride
                    );

                    //green
                    greenBand.ReadRaster(
                        gdal_pix_left,
                        gdal_pix_top,
                        gdalB.Width,
                        gdalB.Height,
                        new IntPtr(buf.ToInt64() + 1),
                        gdal_pix_width,
                        gdal_pix_heigh,
                        DataType.GDT_Byte,
                        4,
                        stride
                    );

                    //red
                    redBand.ReadRaster(
                        gdal_pix_left,
                        gdal_pix_top,
                        gdalB.Width,
                        gdalB.Height,
                        new IntPtr(buf.ToInt64() + 2),
                        gdalB.Width,
                        gdalB.Height,
                        DataType.GDT_Byte,
                        4,
                        stride
                    );


                    if (GdalDataset.RasterCount > 3)
                    {
                        Band alphaBand = GdalDataset.GetRasterBand(4).GetOverview(overviewIdx);

                        alphaBand.ReadRaster(
                            gdal_pix_left,
                            gdal_pix_top,
                            gdalB.Width,
                            gdalB.Height,
                            new IntPtr(buf.ToInt64() + 3),
                            gdalB.Width,
                            gdalB.Height,
                            DataType.GDT_Byte,
                            4,
                            stride
                        );
                    }

                }
                finally
                {
                    gdalB.UnlockBits(bitmapData);
                }

                //at this stage raster data should have been extracted, so now need to paint it over the output bitmap

                //work out the offset of the gdal raster

                double requestedResX = bbox.Width / width;
                double requestedResY = bbox.Height / height;

                //initially assume the image is completely within the raster bounds
                int pixl = 0, pixt = 0, pixw = width, pixh = height;

                //adjust the destination rect in a case bbox intersects with the raster bounds
                if (bbox.MinX < gdal_render_area_left)
                {
                    pixl = (int)(Math.Abs(gdal_render_area_left - bbox.MinX) / requestedResX);
                    pixw -= pixl;
                }

                if (bbox.MaxX > gdal_render_area_right)
                {
                    pixw -= (int)(Math.Abs(bbox.MaxX - gdal_render_area_right) / requestedResX);
                }

                if (bbox.MaxY > gdal_render_area_top)
                {
                    pixt = (int)(Math.Abs(bbox.MaxY - gdal_render_area_top) / requestedResY);
                    pixh -= pixt;
                }

                if (bbox.MinY < gdal_render_area_bottom)
                {
                    pixh -= (int)(Math.Abs(gdal_render_area_bottom - bbox.MinY) / requestedResY);
                }

                //paint the gdal raster on the output bitmap
                using (Graphics g = Graphics.FromImage(outB))
                {
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;

                    //make sure the edge pixels are not taken into account when interpolating
                    //for details check out http://www.codeproject.com/Articles/11143/Image-Resizing-outperform-GDI
                    using (ImageAttributes wrapMode = new ImageAttributes())
                    {
                        wrapMode.SetWrapMode(System.Drawing.Drawing2D.WrapMode.TileFlipXY);

                        //next the rendered bitmap
                        g.DrawImage(
                            gdalB,
                            new Rectangle(pixl, pixt, pixw, pixh), //dest rect
                            0, 0, gdalB.Width, gdalB.Height, //source rect
                            GraphicsUnit.Pixel,
                            wrapMode
                        );
                    }
                }

            }
           


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
