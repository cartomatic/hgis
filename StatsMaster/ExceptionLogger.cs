using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HGIS
{
    public partial class StatsMaster
    {
        /// <summary>
        /// Meant to log exceptions and notify the admin about them if the contact email is configured
        /// </summary>
        /// <param name="ex"></param>
        private void LogException(Exception ex)
        {
            Cartomatic.Utils.Exceptions.ExceptionLogger.logException(ex, this.settings.GetLogFolder());
        }

        private void HandleMongoDbException(Exception ex)
        {
            //for now just dump the exception
            LogException(ex);

            //TODO: disable stats logging routines when mongodb seems to be dead
        }

        private void HandleGeoIpException(Exception ex)
        {
            //for now just dump the exception
            LogException(ex);

            //TODO: disable GeoIp related routines when necessary
        }
    }
}
