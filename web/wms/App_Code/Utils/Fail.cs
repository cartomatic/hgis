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
        /// Returns 404
        /// </summary>
        public void Fail(HttpContext context)
        {
            context.Response.Clear();
            context.Response.Status = "404 Not Found";
            context.Response.StatusCode = 404;
            //context.Response.End();
            HttpContext.Current.Response.Flush();
            HttpContext.Current.Response.SuppressContent = true;
            HttpContext.Current.ApplicationInstance.CompleteRequest();
        }
    }
}