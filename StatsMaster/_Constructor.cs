using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MongoDB.Bson;
using MongoDB.Driver;

namespace HGIS
{
    public partial class StatsMaster
    {

        public StatsMaster(Settings settings)
        {
            this.settings = settings;
            ConnectDb();
            InitIpGeoDb();
        }

        /// <summary>
        /// returns a new instance of StatsMaster
        /// </summary>
        /// <param name="filePath">Path to the json serialised StatsMaster.Settings file</param>
        /// <returns></returns>
        public static StatsMaster FromFile(string filePath)
        {
            StatsMaster output;

            //work out the absolute path
            filePath = Cartomatic.Utils.Path.SolvePath(filePath);

            if (!System.IO.File.Exists(filePath))
            {
                throw new ArgumentException("Specified StatsMaster.Settings config file does not exist");
            }

            try
            {
                output = FromJson(System.IO.File.ReadAllText(filePath));
            }
            catch (Exception ex)
            {
                //just rethrow the exception
                throw ex;
            }

            return output;
        }

        /// <summary>
        /// returns a new instance of StatsMaster
        /// </summary>
        /// <param name="json">Json serialised StatsMaster.Settings object</param>
        /// <returns></returns>
        public static StatsMaster FromJson(string json)
        {
            //deserialise the settings object
            var settings = Cartomatic.Utils.Serialisation.DeserializeFromJson<Settings>(json);
            if (settings == null)
            {
                throw new ArgumentException("It was not possible to deserialise the StatsMaster.Settings json string");
            }

            return new StatsMaster(settings);
        }


        /// <summary>
        /// creates a connection
        /// </summary>
        private void ConnectDb()
        {
            try
            {
                this.mongo = new MongoClient(this.settings.GetConnectionString());
                this.mongodb = this.mongo.GetServer().GetDatabase(this.settings.GetDbName());
                
                //mongo collections
                this.mongocollections = new Dictionary<string, MongoCollection>();


                //always log the total stats

                //init the objects so can grab some internal data
                StatsBase rfr = new ReferrerStatsTotal();
                StatsBase ip = new IpStatsTotal();

                this.mongocollections.Add(rfr.GetCollectionName(), mongodb.GetCollection<ReferrerStatsTotal>(rfr.GetCollectionName()));
                this.mongocollections.Add(ip.GetCollectionName(), mongodb.GetCollection<IpStatsTotal>(ip.GetCollectionName()));

                //depending on the configured settings also prepare other collections
                if (this.settings.LogYearlyStats)
                {
                    //init the objects so can grab some internal data
                    rfr = new ReferrerStatsYearly();
                    ip = new IpStatsYearly();
                    this.mongocollections.Add(rfr.GetCollectionName(), mongodb.GetCollection<ReferrerStatsYearly>(rfr.GetCollectionName()));
                    this.mongocollections.Add(ip.GetCollectionName(), mongodb.GetCollection<IpStatsYearly>(ip.GetCollectionName()));
                }

                if (this.settings.LogMonthlyStats)
                {
                    //init the objects so can grab some internal data
                    rfr = new ReferrerStatsMonthly();
                    ip = new IpStatsMonthly();
                    this.mongocollections.Add(rfr.GetCollectionName(), mongodb.GetCollection<ReferrerStatsMonthly>(rfr.GetCollectionName()));
                    this.mongocollections.Add(ip.GetCollectionName(), mongodb.GetCollection<IpStatsMonthly>(ip.GetCollectionName()));
                }

                if (this.settings.LogWeeklyStats)
                {
                    //init the objects so can grab some internal data
                    rfr = new ReferrerStatsWeekly();
                    ip = new IpStatsWeekly();
                    this.mongocollections.Add(rfr.GetCollectionName(), mongodb.GetCollection<ReferrerStatsWeekly>(rfr.GetCollectionName()));
                    this.mongocollections.Add(ip.GetCollectionName(), mongodb.GetCollection<IpStatsWeekly>(ip.GetCollectionName()));
                }

                if (this.settings.LogDailyStats)
                {
                    //init the objects so can grab some internal data
                    rfr = new ReferrerStatsDaily();
                    ip = new IpStatsDaily();
                    this.mongocollections.Add(rfr.GetCollectionName(), mongodb.GetCollection<ReferrerStatsDaily>(rfr.GetCollectionName()));
                    this.mongocollections.Add(ip.GetCollectionName(), mongodb.GetCollection<IpStatsDaily>(ip.GetCollectionName()));
                }
            }
            catch (Exception ex)
            {
                this.LogException(ex);
            }
        }

        /// <summary>
        /// Inits the MaxMind GeoIp db reader
        /// </summary>
        private void InitIpGeoDb()
        {
            //get the path to the geoip db
            string dbpath = Cartomatic.Utils.Path.SolvePath(this.settings.CityDbPath);

            //and try to init the reader
            try
            {
                this.geoIpReader = new MaxMind.GeoIP2.DatabaseReader(dbpath, MaxMind.Db.FileAccessMode.MemoryMapped);
                this.geoIpCache = new Dictionary<string, MaxMind.GeoIP2.Responses.CityResponse>();
            }
            catch (Exception ex)
            {
                this.LogException(ex);
            }
        }
    }
}
