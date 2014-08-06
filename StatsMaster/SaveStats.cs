using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Web;

using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

namespace HGIS
{
    public partial class StatsMaster
    {

        private delegate void DelegateSaveStats(string referrer, string hostAddress, long bytes);

        public void SaveStats(HttpRequest request, string fPath)
        {
            //file should exist, as taken from tilecache
            SaveStats(
                request,
                new System.IO.FileInfo(fPath).Length
            );
        }


        /// <summary>
        /// Saves the stats based on the request
        /// </summary>
        public void SaveStats(HttpRequest request, long dataSize)
        {
            //only save stats for the actual raster data sent out, ignore the caps / get info requests

            //extract some needed info
            string referrer = null;
            if (request.UrlReferrer != null)
            {
                referrer = request.UrlReferrer.ToString();

                //reset the localhost referrer if needed
                if (!this.settings.FilterLocalhostRequests && (referrer.StartsWith("http://localhost") || referrer.StartsWith("http://127.0.0.1")))
                {
                    referrer = null;
                }
            }
            var ip = request.UserHostAddress;

            DelegateSaveStats save = new DelegateSaveStats(SaveStatsInternal);
            save.BeginInvoke(
                referrer,
                ip,
                dataSize,
                null,
                null
            );
        }

        /// <summary>
        /// saves the stats
        /// </summary>
        private void SaveStatsInternal(string referrer, string hostAddress, long bytes)
        {
            //make sure mongo client is available, otherwise will not be able to do the saving
            if (this.mongo == null) return;

            //if referrer is unknown, just dump the referrer stats as unknown
            if (string.IsNullOrEmpty(referrer)) referrer = "unknown";

            //prepare the request stats object
            var rs = new RequestStatsComplete(referrer, hostAddress, bytes);

            //get the ip related geo data
            try
            {
                if (this.geoIpCache.ContainsKey(hostAddress))
                {
                    rs.SetGeoIp(this.geoIpCache[hostAddress]);
                }
                else
                {
                    if (this.geoIpReader != null)
                    {
                        var gip = this.geoIpReader.City(hostAddress);
                        this.geoIpCache[hostAddress] = gip;
                        rs.SetGeoIp(gip);
                    }
                }
            }
            catch (MaxMind.GeoIP2.Exceptions.AddressNotFoundException)
            {
                //address not in the db so screw it
                //db rec will not have geo ip related data
            }
            catch (Exception ex)
            {
                HandleGeoIpException(ex);
            }


            List<StatsBase> stats = new List<StatsBase>();

            //always save the totalstats
            stats.Add(rs.OutputAs<ReferrerStatsTotal>());
            stats.Add(rs.OutputAs<IpStatsTotal>());

            //yearly stats
            if (this.settings.LogYearlyStats)
            {
                stats.Add(rs.OutputAs<ReferrerStatsYearly>());
                stats.Add(rs.OutputAs<IpStatsYearly>());
            }

            //monthly stats
            if (this.settings.LogMonthlyStats)
            {
                stats.Add(rs.OutputAs<ReferrerStatsMonthly>());
                stats.Add(rs.OutputAs<IpStatsMonthly>());
            }

            //weekly stats
            if (this.settings.LogWeeklyStats)
            {
                stats.Add(rs.OutputAs<ReferrerStatsWeekly>());
                stats.Add(rs.OutputAs<IpStatsWeekly>());
            }

            //daily stats
            if (this.settings.LogDailyStats)
            {
                stats.Add(rs.OutputAs<ReferrerStatsDaily>());
                stats.Add(rs.OutputAs<IpStatsDaily>());
            }

            //finally save whatever should be saved
            try
            {
                foreach (var s in stats)
                {
                    this.mongocollections[s.GetCollectionName()].Update(
                        s.GetQueryBuilder(),
                        s.GetUpdateBuilder(),
                        MongoDB.Driver.UpdateFlags.Upsert
                    );
                }
            }
            catch (Exception ex)
            {
                HandleMongoDbException(ex);
            }
        }
    }
}
