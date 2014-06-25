using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Web;

using Cartomatic.Wms.WmsDriver;

namespace HGIS.GDAL
{
    public partial class WmsDriver
    {
        /// <summary>
        /// Performs driver specific internal checkup
        /// </summary>
        /// <returns></returns>
        protected override WmsDriverValidationResponse ValidateDriver()
        {
            var output = new WmsDriverValidationResponse();

            output.Success = this.DriverReady;
            output.Msg = this.DriverExceptionMsg;

            return output;
        }
    }
}
