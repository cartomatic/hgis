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
        /// <param name="gdal">Whether the utils should prepare gdal driver related settings</param>
        /// <param name="manifold">Whether the utils should prepare manifold related settings</param>
        public WmsUtils(bool gdal = false, bool manifold = false)
        {
            if ((!gdal && !manifold) || (gdal && manifold)) throw new Exception("WMS Driver type must be properly specified mate!");

            PrepareService(gdal, manifold);
        }
    }
}