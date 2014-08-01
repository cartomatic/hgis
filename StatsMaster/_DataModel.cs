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
        public class Settings
        {
            public Settings() :
                this("localhost", 28017)
            { }

            public Settings(string host, int? port)
            {
                this.Host = host;
                this.Port = port;
            }

            public string GetConnectionString()
            {
                return "mongodb://" + this.GetHost() + ":" + this.GetPort();
            }

            /// <summary>
            /// returns host and port needed fpr connection
            /// </summary>
            /// <returns></returns>
            public string GetHost()
            {
                return string.IsNullOrEmpty(this.Host) ? "localhost" : this.Host;
            }

            public int GetPort()
            {
                return this.Port.HasValue && this.Port > 0 ? (int)this.Port : 28017;
            }

            /// <summary>
            /// Gets the log folder
            /// </summary>
            /// <returns></returns>
            public string GetLogFolder()
            {
                return string.IsNullOrEmpty(this.LogFolder) ? "stats_master" : this.LogFolder;
            }

            /// <summary>
            /// Returns configured db name; if db name is not configured, defaults to hgis
            /// </summary>
            /// <returns></returns>
            public string GetDbName()
            {
                return string.IsNullOrEmpty(this.DbName) ? "hgis" : this.DbName;
            }

            /// <summary>
            /// Db host
            /// </summary>
            public string Host { get; set; }

            /// <summary>
            /// Db port
            /// </summary>
            public int? Port { get; set; }


            /// <summary>
            /// Log folder used by the exeption logger to dump the log file; defaults to stats_master if not set
            /// </summary>
            public string LogFolder { get; set; }


            /// <summary>
            /// Name; defaults to hgis
            /// </summary>
            public string DbName { get; set; }

            /// <summary>
            /// Whether or not to log yearly stats; by default only total stats are logged
            /// </summary>
            public bool LogYearlyStats { get; set; }

            /// <summary>
            /// Whether or not to log monthly  stats; by default only total stats are logged
            /// </summary>
            public bool LogMonthlyStats { get; set; }

            /// <summary>
            /// Whether or not to log weekly stats; by default only total stats are logged
            /// </summary>
            public bool LogWeeklyStats { get; set; }

            /// <summary>
            /// Whether or not to log daily stats; by default only total stats are logged
            /// </summary>
            public bool LogDailyStats { get; set; }

            /// <summary>
            /// Path to the MaxMind's lite city db
            /// </summary>
            public string CityDbPath { get; set; }

            /// <summary>
            /// Email account to report the problems to
            /// </summary>
            public Cartomatic.Utils.Email.EmailAccount Email { get; set; }
        }


        /// <summary>
        /// stats master settings
        /// </summary>
        public Settings settings { get; set; }


        /// <summary>
        /// client instance
        /// </summary>
        private MongoClient mongo { get; set; }

        /// <summary>
        /// Mongo db used to save the data
        /// </summary>
        private MongoDatabase mongodb { get; set; }


        /// <summary>
        /// Number of mongodb related exceptions catched; If larger than the cutoff specifed through the settings object, an email to the admin is sent
        /// </summary>
        private int mongodbExceptionCount { get; set; }


        /// <summary>
        /// Mongo collections used to save the data
        /// </summary>
        private Dictionary<string, MongoCollection> mongocollections { get; set; }

        /// <summary>
        /// MaxMind DatabaseReader to retrieve geolocation based on ip
        /// </summary>
        private MaxMind.GeoIP2.DatabaseReader geoIpReader { get; set; }

        /// <summary>
        /// Runtime GeoIp data cache
        /// </summary>
        private Dictionary<string, MaxMind.GeoIP2.Responses.CityResponse> geoIpCache { get; set; }

        /// <summary>
        /// Number of the geoIp related exceptions catched; If larger than the cutoff specifed through the settings object, an email to the admin is sent
        /// </summary>
        private int geoIpExceptionCount { get; set; }
    }
}
