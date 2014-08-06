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
        /// Performs the referrer tables cleanup
        /// </summary>
        public void DoLocalhostReferrerDataCleanup()
        {
            //anything to be done here????
            //Q: should we leave the different localhosts so some localhost traffic (likely dev traffic) is logged,
            //or ignore it completely and just make it all unknown???
        }


        /// <summary>
        /// Performs ip related data cleanup by reading all the records and ensuring the available ip related data gets updated (for example after the ip database has been updated)
        /// </summary>
        public void DoIpDataCleanup()
        {
            //the ip data cache
            var geoIpCache = new Dictionary<string, MaxMind.GeoIP2.Responses.CityResponse>();

            //init the stats objects
            var stats = new List<IpStatsTotal>()
            {
                new IpStatsTotal(),
                new IpStatsYearly(),
                new IpStatsMonthly(),
                new IpStatsWeekly(),
                new IpStatsDaily()
            };


            foreach (var s in stats)
            {
                //get the collection
                var col = this.mongocollections[s.GetCollectionName()];

                //get the cursor to grab all the recs
                var cursor = col.FindAllAs(s.GetType());

                //and iterate through them to do the fixing
                foreach (IpStatsTotal rec in cursor)
                {
                    //get the ip record
                    try
                    {
                        if (geoIpCache.ContainsKey(rec.Ip))
                        {
                            rec.UpdateGeoIp(geoIpCache[rec.Ip]);
                        }
                        else
                        {
                            if (this.geoIpReader != null)
                            {
                                var gip = this.geoIpReader.City(rec.Ip);
                                geoIpCache[rec.Ip] = gip;
                                rec.UpdateGeoIp(gip);
                            }
                        }

                        //finally save the updated record
                        col.Save(rec);
                    }
                    catch (Exception ex)
                    {
                        //silently fail but log the exception
                        LogException(ex);
                    }
                }
            }
        }

    }
}
