<%@ WebHandler Language="C#" Class="Wms" %>

using System;
using System.Web;
using System.Web.SessionState;
using System.Configuration;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

/// <summary>
/// HGIS.cartomatic.pl internal WMS service handler.
/// Meant to be a lower performance endpoint serving the data for the public unauthenticated users;
/// in general should not be visible to the public and should be asynchronously proxied in order to enable
/// the public endpoint to act as a high performance service for the authenticated users as well as the low performance service for the public users at the same time
/// </summary>
public class Wms : IHttpHandler
{
    /// <summary>
    /// Setings object - used for easy deserialisation
    /// </summary>
    private class WmsSettings
    {
        public WmsSettings() { }

        /// <summary>
        /// url of the public facing endpoint; used to generate a valid Caps doc when WMS is proxied
        /// </summary>
        public string PublicAccessUrl { get; set; }

        /// <summary>
        /// data folder for the particular wms engine
        /// </summary>
        public string WmsDataFolder { get; set; }

        /// <summary>
        /// Watermark file path
        /// </summary>
        public string Watermark { get; set; }
    }
    
    /// <summary>
    /// Whether or not the WMS service has already been prepared
    /// </summary>
    private static bool ServicePrepared = false;
    
    /// <summary>
    /// WMS service description
    /// </summary>
    private static Cartomatic.Wms.WmsDriver.WmsServiceDescription ServiceDescription;

    /// <summary>
    /// Wms ServiceDescription
    /// </summary>
    private static WmsSettings Settings;

    /// <summary>
    /// Tile cache ServiceDescription
    /// </summary>
    private static Cartomatic.MapUtils.TileCache.Settings Tcs;

    /// <summary>
    /// configured tile schemes 
    /// </summary>
    private static List<Cartomatic.MapUtils.Tiling.TileScheme> TileSchemes;

    /// <summary>
    /// Watermark bitmap; will not be applied if could not be read
    /// </summary>
    private static Bitmap Watermark;
    
    
    public void ProcessRequest (HttpContext context) {

        //Prepare the service if needed
        if (!ServicePrepared)
        {
            PrepareService();
            ServicePrepared = true;
        }


        //Let the tilecache process the Request
        var tcout = Cartomatic.MapUtils.TileCache.WmsRequestProcessor.ProcessRequest(
            Tcs,
            GetTileScheme(context),
            context.Request.Url.AbsoluteUri,
            GetWmsdriver(context)
        );


        //Note:
        //If there is a need to separate cache for different urls, wmses, ect it should be done by adjusting the
        //CacheFolder property of tile cache ServiceDescription object
        //by default tilecache extracts map & source params and creates a folder based on them (source_map) so cache separation may
        //alternatively be achieved by adding those 2 params (if possible of course as they may be meanigful, for example with the manifold wms driver)

        //transfer the tile cache response to the response object
        context.Response.ContentType = tcout.ResponseContentType;

        //if the output contains binary data, write binary
        if (tcout.HasFile)
        {
            //let the watermark stamper decide how to return the data
            ApplyWatermark(tcout.FilePath, tcout.ResponseContentType, context);
        }
        else if (tcout.HasData)
        {
            //let the watermark stamper decide how to return the data
            ApplyWatermark(tcout.ResponseBinary, tcout.ResponseContentType, context);
        }
        else //otherwise write returned text
        {
            context.Response.Write(tcout.ResponseText);
        }
       
        context.Response.End();       
    }

    
    /// <summary>
    /// Prepares the service
    /// </summary>
    private void PrepareService()
    {
        //extract the wms ServiceDescription
        Settings = Cartomatic.Utils.Serialisation.DeserializeFromJson<WmsSettings>(
            ReadFileTxt(ConfigurationManager.AppSettings["wms_settings"])
        );
        //and fix the paths so they can be expected to be absolute
        Settings.WmsDataFolder = Cartomatic.Utils.Path.SolvePath(Settings.WmsDataFolder);
        Settings.Watermark = Cartomatic.Utils.Path.SolvePath(Settings.Watermark);
                
        
        //Describe the wms service
        ServiceDescription = Cartomatic.Wms.WmsDriver.WmsServiceDescription.FromJson(
            ReadFileTxt(ConfigurationManager.AppSettings["wms_service_description"])
        );

        //Note:
        //if a desired wms PublicAccessURL is not provided, the url is worked out based on the current Request url;
        //since this is a service behind accessible through a proxy, PublicAccessURL must be adjusted appropriately
        ServiceDescription.PublicAccessURL = Settings.PublicAccessUrl;

        //read tile cache ServiceDescription
        Tcs = Cartomatic.MapUtils.TileCache.Settings.FromJson(
            ReadFileTxt(ConfigurationManager.AppSettings["tile_cache_settings"])
        );
        
        //read tile schemes
        TileSchemes = Cartomatic.Utils.Serialisation.DeserializeFromJson <List<Cartomatic.MapUtils.Tiling.TileScheme>>(
            ReadFileTxt(ConfigurationManager.AppSettings["tile_schemes"])
        );
        
        //finally read in the watermark image
        try
        {
            Watermark = new Bitmap(Settings.Watermark);
        }
        catch { } //silently fail, no watermark will be painted
    }

    
    /// <summary>
    /// Applies the 
    /// </summary>
    /// <param name="b"></param>
    /// <returns></returns>
    private void ApplyWatermark(byte[] b, string contentType, HttpContext context)
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

    private void ApplyWatermark(string b, string contentType, HttpContext context)
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
            g.DrawImage(
                Watermark,
                new Rectangle(0, 0, Watermark.Width, Watermark.Height), //source
                new Rectangle(0, 0, b.Width, b.Height), //destination
                GraphicsUnit.Pixel
            );
        }
        
        //when ready convert the image to byte arr
        using (var ms = new MemoryStream())
        {
            b.Save(ms, Cartomatic.Wms.WmsDriver.Base.GetEncoderInfo(contentType), null);
            output = ms.ToArray();
        }

        return output;
    }
    
    
    /// <summary>
    /// Reads the txt file from the specified path - either absolute or relative
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    private string ReadFileTxt(string path)
    {
        var output = string.Empty;

        try
        {
            output = System.IO.File.ReadAllText(
                Cartomatic.Utils.Path.SolvePath(path)
            );
        }
        catch { }

        return output;
    }

    /// <summary>
    /// Gets the safe (cloned) tile scheme approprite for the given Request
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    private Cartomatic.MapUtils.Tiling.TileScheme GetTileScheme(HttpContext context)
    {
        Cartomatic.MapUtils.Tiling.TileScheme output = null;
        
        //tile scheme depends on the requested data - so far it depends on the EPSG
        var epsg = GetEpsg(context);
        try
        {
            output = TileSchemes.Find(ts => ts.Epsg == epsg);
        }
        catch { } //silently fail
        
        
        //return a safe copy of the tile scheme, so it does not get screwed when being used for calculations by different threads
        if (output != null)
        {
            return output.Clone();
        }
        else
        {
            return output;
        }
    }

    /// <summary>
    /// extracts the epsg off the request
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    private string GetEpsg(HttpContext context)
    {
        //start with the >= 1.3 CRS param
        var epsg = context.Request.Params["CRS"];
        if (string.IsNullOrEmpty(epsg))
        {
            //do a < 1.3 fallback
            epsg = context.Request.Params["SRS"];
        }
        
        return epsg;
    }
    
    /// <summary>
    /// Gets a wms driver instance that will be used to handle the Request
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    private Cartomatic.Manifold.WmsDriver GetWmsdriver(HttpContext context)
    {
        //first get the map file and map component
        var source = context.Request.Params["source"];
        var map = context.Request.Params["map"];

        //fail if the map file has not been supplied
        if (string.IsNullOrEmpty(source))
        {
            Fail(context);
        }

        return new Cartomatic.Manifold.WmsDriver(
            GetMapFileLocation(context),
            GetMapComponent(context),
            ServiceDescription
        );   
    }
    
    /// <summary>
    /// Returns a full path to a map file that should be loaded by mapserver
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    private string GetMapFileLocation(HttpContext context)
    {
        var source = context.Request.Params["source"];
        
        //fail if the map file has not been supplied
        if(string.IsNullOrEmpty(source)){
            Fail(context);
        }
        
        return Settings.WmsDataFolder + source + ".map";
    }

    /// <summary>
    /// Gets map component that will be used by the mapserver to render maps
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    private string GetMapComponent(HttpContext context)
    {
        //try to extract map component to be used with this Request
        string mapComp = context.Request.Params["map"];
        
        //if the param has not been supplied default to 'Map'
        if (string.IsNullOrEmpty(mapComp))
        {
            mapComp = "Map";
        }
        return mapComp;
    }
    
    /// <summary>
    /// reuse the handler's instance
    /// </summary>
    public bool IsReusable {
        get {
            return true;
        }
    }
    
    /// <summary>
    /// Returns 404
    /// </summary>
    private void Fail(HttpContext context)
    {
        context.Response.Clear();
        context.Response.Status = "404 Not Found";
        context.Response.StatusCode = 404;
        context.Response.End();
    }

}