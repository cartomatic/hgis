using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using StackExchange.Redis;

namespace HGIS
{
    public partial class TokenMaster
    {

        public TokenMaster(Settings settings)
        {
            this.settings = settings;
            ConnectDb();
        }

        public TokenMaster(string host, string port, int? secondsTicketValid)
            : this(new Settings(host, port, secondsTicketValid))
        { }

        public TokenMaster(string host, string port)
            : this(host, port, null)
        { }

        /// <summary>
        /// returns a new instance of TokenMaster
        /// </summary>
        /// <param name="filePath">Path to the json serialised TokenMaster.Settings file</param>
        /// <returns></returns>
        public static TokenMaster FromFile(string filePath)
        {
            TokenMaster output;
            
            //work out the absolute path
            filePath = Cartomatic.Utils.Path.SolvePath(filePath);

            if (!System.IO.File.Exists(filePath))
            {
                throw new ArgumentException("Specified TokenMaster.Settings config file does not exist");
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
        /// returns a new instance of TokenMaster
        /// </summary>
        /// <param name="json">Json serialised TokenMaster.Settings object</param>
        /// <returns></returns>
        public static TokenMaster FromJson(string json)
        {
            //deserialise the settings object
            var settings = Cartomatic.Utils.Serialisation.DeserializeFromJson<Settings>(json);
            if (settings == null)
            {
                throw new ArgumentException("It was not possible to deserialise the TokenMaster.Settings json string");
            }

            return new TokenMaster(settings);
        }

        /// <summary>
        /// creates a connection
        /// </summary>
        private void ConnectDb()
        {
            try
            {
                this.redis = ConnectionMultiplexer.Connect(this.settings.GetHostAndPort());
                db = redis.GetDatabase(asyncState: asyncState);
            }
            catch (Exception ex)
            {
                this.LogException(ex);
            }
        }
        
    }
}
