using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Bson.Serialization.Attributes;

namespace HGIS
{
    public partial class StatsMaster
    {

        public class StatsBase
        {
            public StatsBase() { }

            /// <summary>
            /// Object Id
            /// </summary>
            [BsonRepresentation(BsonType.String)] 
            public ObjectId Id { get; set; }


            /// <summary>
            /// Name of a collection an object is saved to
            /// </summary>
            protected string CollectionName { get; set; }

            /// <summary>
            /// Amount of hits for the particular referrer
            /// </summary>
            public long Hits { get; set; }

            /// <summary>
            /// Amount of bytes sent out for the particular referrer
            /// </summary>
            public long Bytes { get; set; }

            /// <summary>
            /// Properties that should get increased on each update; other properties will only be set on insert
            /// </summary>
            protected static string[] PropsIncreasedOnUpdate = new string[] { "Hits", "Bytes" };

            /// <summary>
            /// Properties that should be ignored when querrying an object
            /// </summary>
            protected static string[] PropsIgnoredOnQuery = new string[] {"Id", "Hits", "Bytes", "Lon", "Lat" };

            /// <summary>
            /// Properties that should get ignored on read query
            /// </summary>
            protected static string[] PropsIgnoredOnReadQuery = new string[] { "Referrer", "Ip", "CountryIso", "Country", "City" };

            /// <summary>
            /// Bytes in a GigaByte; A Gigabyte is 1,073,741,824 (2^30) bytes. 1,024 Megabytes, or 1,048,576 Kilobytes.
            /// </summary>
            protected static int BytesInGigaByte = (int)Math.Pow(2, 30);

            /// <summary>
            /// Return total transferred size in Gb
            /// </summary>
            /// <returns></returns>
            public double GetGbs()
            {
                return (double)this.Bytes / BytesInGigaByte;
            }

            public string GetCollectionName()
            {
                return this.CollectionName;
            }

            /// <summary>
            /// Returns an update builder used in the mongodb's update procedure;
            /// prepares a builder that sets some properties on insert and increases some other properties on update
            /// </summary>
            /// <returns></returns>
            public IMongoUpdate GetUpdateBuilder()
            {
                MongoDB.Driver.Builders.UpdateBuilder SetOnInsertBuilder = new MongoDB.Driver.Builders.UpdateBuilder();
                MongoDB.Driver.Builders.UpdateBuilder IncBuilder = new MongoDB.Driver.Builders.UpdateBuilder();

                //object's own props
                var props = this.GetType().GetProperties();

                //build updates
                foreach (var p in props)
                {
                    if (!PropsIncreasedOnUpdate.Contains(p.Name))
                    {
                        //use dynamic here so it can be then converted to BsonValue by the mongodb driver
                        dynamic pValue = p.GetValue(this);

                        if (pValue == null)
                        {
                            SetOnInsertBuilder.SetOnInsert(p.Name, BsonNull.Value); 
                        }
                        else
                        {
                            SetOnInsertBuilder.SetOnInsert(p.Name, pValue);
                        }
                    }
                    else
                    {
                        IncBuilder.Inc(p.Name, p.GetValue(this) as dynamic);
                    }
                }

                //output combined update builders
                return MongoDB.Driver.Builders.Update.Combine(SetOnInsertBuilder, IncBuilder);
            }

            /// <summary>
            /// Returns a query builder used to grab an object during the update procedure
            /// </summary>
            /// <returns></returns>
            public IMongoQuery GetQueryBuilder()
            {
                //List of querries needed to locate a document
                var querries = new List<IMongoQuery>();

                //object's own props
                var props = this.GetType().GetProperties();

                //build querries
                foreach (var p in props)
                {
                    if (!PropsIgnoredOnQuery.Contains(p.Name))
                    {
                        querries.Add(
                            MongoDB.Driver.Builders.Query.EQ(p.Name, p.GetValue(this) as dynamic)
                        );
                    }
                }

                //output combined querries
                return MongoDB.Driver.Builders.Query.And(querries);
            }

            /// <summary>
            /// Returns a query builder used to read the data
            /// </summary>
            /// <returns></returns>
            public IMongoQuery GetReadQueryBuilder()
            {
                //List of querries needed to locate a document
                var querries = new List<IMongoQuery>();

                //object's own props
                var props = this.GetType().GetProperties();

                //build querries
                foreach (var p in props)
                {
                    if (!PropsIgnoredOnQuery.Contains(p.Name) && !PropsIgnoredOnReadQuery.Contains(p.Name))
                    {
                        querries.Add(
                            MongoDB.Driver.Builders.Query.EQ(p.Name, p.GetValue(this) as dynamic)
                        );
                    }
                }

                //output combined querries
                if (querries.Count > 0)
                {
                    return MongoDB.Driver.Builders.Query.And(querries);
                }
                else
                {
                    return MongoDB.Driver.Builders.Query.Null;
                }
            }


            /// <summary>
            /// Outputs the object as the specified type; clones the properties internaly
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <returns></returns>
            public T OutputAs<T>() where T : StatsBase
            {
                //create output object
                T output = Activator.CreateInstance<T>();

                //get the properties of the output type
                var props = output.GetType().GetProperties();

                var myType = this.GetType();

                foreach (var p in props)
                {
                    p.SetValue(output, myType.GetProperty(p.Name).GetValue(this));
                }

                return output;
            }
        }


        /// <summary>
        /// Total referrer stats representation
        /// </summary>
        public class ReferrerStatsTotal : StatsBase
        {
            public ReferrerStatsTotal()
            {
                this.CollectionName = "referrer_total";
            }

            /// <summary>
            /// Referrer
            /// </summary>
            public string Referrer { get; set; }

            public ReferrerStatsTotal ToTotal()
            {
                return OutputAs<ReferrerStatsTotal>();
            }
        }

        /// <summary>
        /// Yearly referrer stats representation
        /// </summary>
        public class ReferrerStatsYearly : ReferrerStatsTotal
        {
            public ReferrerStatsYearly()
            {
                this.CollectionName = "referrer_yearly";
            }

            /// <summary>
            /// Year
            /// </summary>
            public int? Year { get; set; }
        }

        /// <summary>
        /// Monthly referrer stats representation
        /// </summary>
        public class ReferrerStatsMonthly : ReferrerStatsYearly
        {
            public ReferrerStatsMonthly()
            {
                this.CollectionName = "referrer_monthly";
            }

            /// <summary>
            /// Month of the year
            /// </summary>
            public int? Month { get; set; }
        }

        /// <summary>
        /// Weekly referrer stats representation
        /// </summary>
        public class ReferrerStatsWeekly : ReferrerStatsYearly
        {

            public ReferrerStatsWeekly()
            {
                this.CollectionName = "referrer_weekly";
            }

            /// <summary>
            /// Week of the year
            /// </summary>
            public int? Week { get; set; }
        }

        /// <summary>
        /// Daily referrer stats representation
        /// </summary>
        public class ReferrerStatsDaily : ReferrerStatsMonthly
        {
            public ReferrerStatsDaily()
            {
                this.CollectionName = "referrer_daily";
            }

            /// <summary>
            /// Day of the month
            /// </summary>
            public int? Day { get; set; }
        }


        /// <summary>
        /// Total ip stats representation
        /// </summary>
        public class IpStatsTotal : StatsBase
        {
            public IpStatsTotal()
            {
                this.CollectionName = "ip_total";
            }

            /// <summary>
            /// Ip address
            /// </summary>
            public string Ip { get; set; }

            /// <summary>
            /// Longitude
            /// </summary>
            public double? Lon { get; set; }

            /// <summary>
            /// Latitude
            /// </summary>
            public double? Lat { get; set; }

            /// <summary>
            /// Request IP Country ISO code
            /// </summary>
            public string CountryIso { get; set; }

            /// <summary>
            /// Request IP country
            /// </summary>
            public string Country { get; set; }

            /// <summary>
            /// Request IP city
            /// </summary>
            public string City { get; set; }

            public IpStatsTotal ToTotal()
            {
                return OutputAs<IpStatsTotal>();
            }
        }

        /// <summary>
        /// Yearly ip stats representation
        /// </summary>
        public class IpStatsYearly : IpStatsTotal
        {
            public IpStatsYearly()
            {
                this.CollectionName = "ip_yearly";
            }

            /// <summary>
            /// Year
            /// </summary>
            public int? Year { get; set; }
        }

        /// <summary>
        /// Monthly ip stats representation
        /// </summary>
        public class IpStatsMonthly : IpStatsYearly
        {
            public IpStatsMonthly()
            {
                this.CollectionName = "ip_monthly";
            }

            /// <summary>
            /// Month of the year
            /// </summary>
            public int? Month { get; set; }
        }

        /// <summary>
        /// Weekly ip stats representation
        /// </summary>
        public class IpStatsWeekly : IpStatsYearly
        {
            public IpStatsWeekly()
            {
                this.CollectionName = "ip_weekly";
            }

            /// <summary>
            /// Week of the year
            /// </summary>
            public int? Week { get; set; }
        }

        /// <summary>
        /// Daily ip stats representation
        /// </summary>
        public class IpStatsDaily : IpStatsMonthly
        {
            public IpStatsDaily()
            {
                this.CollectionName = "ip_daily";
            }
            
            /// <summary>
            /// Day of the month
            /// </summary>
            public int? Day { get; set; }
        }


        /// <summary>
        /// Complete request stats representation
        /// </summary>
        public class RequestStatsComplete: StatsBase
        {
            public RequestStatsComplete(string referrer, string ip, long byteSize)
            {
                this.Referrer = referrer;

                this.Ip = ip;

                this.Bytes = byteSize;
                this.Hits = 1;

                //date stuff
                var now = DateTime.Now;

                this.Year = now.Year;
                this.Month = now.Month;
                this.Day = now.Day;
                this.Week = now.DayOfYear / 7;
            }

            /// <summary>
            /// Referrer
            /// </summary>
            public string Referrer { get; set; }

            /// <summary>
            /// Ip address
            /// </summary>
            public string Ip { get; set; }

            /// <summary>
            /// Year
            /// </summary>
            public int? Year { get; set; }


            /// <summary>
            /// Month of a year
            /// </summary>
            public int? Month { get; set; }

            /// <summary>
            /// Week of a year
            /// </summary>
            public int? Week { get; set; }

            /// <summary>
            /// Day of a month
            /// </summary>
            public int? Day { get; set; }


            /// <summary>
            /// Request IP Country ISO code
            /// </summary>
            public string CountryIso { get; set; }

            /// <summary>
            /// Request IP country
            /// </summary>
            public string Country { get; set; }

            /// <summary>
            /// Request IP city
            /// </summary>
            public string City { get; set; }


            /// <summary>
            /// Request IP longitude
            /// </summary>
            public double? Lon { get; set; }

            /// <summary>
            /// Request IP latitude
            /// </summary>
            public double? Lat { get; set; }


            /// <summary>
            /// Sets the geo ip related props
            /// </summary>
            /// <param name="geoIp"></param>
            public void SetGeoIp(MaxMind.GeoIP2.Responses.CityResponse geoIp)
            {
                if (geoIp == null) return;

                this.CountryIso = geoIp.Country.IsoCode;
                this.Country = geoIp.Country.Name;
                this.City = geoIp.City.Name;
                this.Lon = geoIp.Location.Longitude;
                this.Lat = geoIp.Location.Latitude;
            }
        }
    }
}
