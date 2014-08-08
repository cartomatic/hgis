using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Driver.Builders;

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

                //ensure index
                EnsureIndex(s);


                //get the cursor to grab all the recs
                var cursor = col.FindAllAs(s.GetType());

                try
                {

                    //and iterate through them to do the fixing
                    foreach (IpStatsTotal rec in cursor)
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
                }
                catch (Exception ex)
                {
                    //silently fail but log the exception
                    LogException(ex);
                }
            }
        }

        /// <summary>
        /// Cleans up string ids - this is because of the wrong id serialisation set on the statsbase initially
        /// so when updating, duplicate objects with string ids (not the default ObjectId) were inserted instead of updating existing objects.
        /// this affects only ip stats as refererrer collections were not updated.
        /// well shit happens...
        /// </summary>
        public void DoStringIdCleanup()
        {

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

                //grab a cursor 
                var cursor = col.FindAllAs(s.GetType());

                //place holder for the read ids
                List<string> removeIds = new List<string>();

                foreach (StatsBase rec in cursor)
                {
                    if (!removeIds.Contains(rec.Id.ToString()))
                    {
                        removeIds.Add(rec.Id.ToString());
                    }
                }

                foreach (var id in removeIds)
                {
                    col.Remove(Query.EQ("_id", id));
                }
            }
        }

        /// <summary>
        /// Cleans up xtra properties that go added due to wrong id serialisation flag
        /// </summary>
        public void DoExtraIdPropertyCleanup()
        {
            //init the stats objects
            var stats = new List<StatsBase>()
            {
                new IpStatsTotal(),
                new IpStatsYearly(),
                new IpStatsMonthly(),
                new IpStatsWeekly(),
                new IpStatsDaily(),
                new ReferrerStatsTotal(),
                new ReferrerStatsYearly(),
                new ReferrerStatsMonthly(),
                new ReferrerStatsWeekly(),
                new ReferrerStatsDaily()
            };

            foreach (var s in stats)
            {
                //get the collection
                var col = this.mongocollections[s.GetCollectionName()];

                col.Update(
                    Query.Exists("Id"),
                    Update.Unset("Id"),
                    MongoDB.Driver.UpdateFlags.Multi
                );
            }
        }

        /// <summary>
        /// Ensures index exist for the collection that keeps the data for an object
        /// </summary>
        /// <param name="col"></param>
        private void EnsureIndex(StatsBase s)
        {
            //get collection
            var col = this.mongocollections[s.GetCollectionName()];
            col.CreateIndex(s.GetIndexedKeys(), MongoDB.Driver.Builders.IndexOptions.SetUnique(true).SetDropDups(true).SetBackground(true));
        }

        /// <summary>
        /// Ensures indexes exist for the collections that keep datafor the used objects
        /// </summary>
        /// <param name="stats"></param>
        private void EnsureIndex(List<StatsBase> stats){
            foreach (var s in stats)
            {
                EnsureIndex(s);
            }
        }
    }
}
