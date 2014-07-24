using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using System.Drawing;
using System.IO;

namespace HGIS
{

    /// <summary>
    /// WMS utils
    /// </summary>
    public partial class WmsUtils
    {
        /// <summary>
        /// Creates a new instance of the utils 
        /// </summary>
        /// <param name="d">Type of the WMS driver to be used</param>
        public WmsUtils(WmsDriverType d = WmsDriverType.Undefined)
        {
            if (d == WmsDriverType.Undefined) throw new Exception("WMS Driver type must be properly specified mate!");
            this.DriverType = d;
            PrepareService();
        }
    }
}