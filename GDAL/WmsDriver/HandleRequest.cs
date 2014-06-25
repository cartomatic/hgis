using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Cartomatic.Wms.WmsDriver;

namespace HGIS.GDAL
{
    public partial class WmsDriver
    {

        /// <summary>
        /// Need to override the base wms driver handle method in order to initiate the mapserver with correct ServiceDescription
        /// </summary>
        /// <param name="Request"></param>
        /// <returns></returns>
        public override WmsDriverResponse Handle(System.Net.HttpWebRequest request)
        {
            //store the Request object internally 
            this.Request = request;

            //prepares the data set for further usage
            this.PrepareDriver();
            

            //finally call the proper handle on base
            return base.Handle(request);
        }
    }
}
