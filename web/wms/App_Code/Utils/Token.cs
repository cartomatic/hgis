using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using System.Drawing;
using System.IO;

using System.Threading.Tasks;

namespace HGIS
{
    public partial class WmsUtils
    {
        /// <summary>
        /// Checks if the token is valid (whether or not user has been authenticated)
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task<bool> CheckIfTokenValid(HttpContext context)
        {
            bool tokenValaid = false;

            //TODO
            //redis based token checkup utility

            //for now just a basic dev test
            if (context.Request.Params["t"] == "xxx")
            {
                tokenValaid = true;
            }

            return tokenValaid;
        }
    }
}