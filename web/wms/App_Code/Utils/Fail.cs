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
        /// Returns 404;
        /// Note: uses HttpContext.Current.ApplicationInstance.CompleteRequest(), so the code after the Fail() method is executed!
        /// </summary>
        public void Fail(HttpContext context)
        {
            context.Response.Clear();
            context.Response.Status = "404 Not Found";
            context.Response.StatusCode = 404;
            
            //DO NOT USE
            //context.Response.End();

            //Note:
            //Do not use Response.End() as it throws ThreadAbortException exception and is likely to cause problems;
            //even though it does not always shows 'visible' symptoms (500 basically) it will show in the IIS log.
            //Response.Close will also not do ;)
            //
            //Instead HttpContext.Current.ApplicationInstance.CompleteRequest() / context.ApplicationInstance.CompleteRequest() should be used.
            //
            //for details see:
            //http://support.microsoft.com/kb/312629
            //http://msdn.microsoft.com/lv-lv/library/system.web.httpresponse.end(en-us).aspx
            //http://blogs.msdn.com/b/aspnetue/archive/2010/05/25/response-end-response-close-and-how-customer-feedback-helps-us-improve-msdn-documentation.aspx

            context.ApplicationInstance.CompleteRequest();
        }
    }
}