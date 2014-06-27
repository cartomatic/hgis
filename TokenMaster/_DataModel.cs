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
        /// <summary>
        /// Token generator settings
        /// </summary>
        public class Settings
        {
            public Settings() {}

            public Settings(string host, string port, int? secondsTicketValid)
            {
                if (string.IsNullOrEmpty(host))
                {
                    throw new ArgumentOutOfRangeException("host", "Param cannot be empty");
                }
                
                if (!secondsTicketValid.HasValue)
                {
                    secondsTicketValid = defaultSecondsTicketValid;
                }

                if (secondsTicketValid <= 0)
                {
                    throw new ArgumentOutOfRangeException("secondsTicketValid", "If provided, param value must be greater than 0");
                }

                this.Host = host;
                this.Port = port;
                this.SecondsTicketValid = secondsTicketValid;
            }

            /// <summary>
            /// returns host and port needed fpr connection
            /// </summary>
            /// <returns></returns>
            public string GetHostAndPort()
            {
                string handp = Host;
                if (!string.IsNullOrEmpty(Port))
                {
                    handp += ":" + Port;
                }
                return handp;
            }

            /// <summary>
            /// Gets the seconds ticket will be valid
            /// </summary>
            /// <returns></returns>
            public int GetSecondsTicketValid()
            {
                if (!this.SecondsTicketValid.HasValue)
                {
                    this.SecondsTicketValid = defaultSecondsTicketValid;
                }
                return (int)this.SecondsTicketValid;
            }

            /// <summary>
            /// Gets the log folder
            /// </summary>
            /// <returns></returns>
            public string GetLogFolder()
            {
                if (string.IsNullOrEmpty(this.LogFolder))
                {
                    this.LogFolder = defaultLogFolder;
                }
                return this.LogFolder;
            }


            /// <summary>
            /// Gets the token param used to extract the token off the request url
            /// </summary>
            /// <returns></returns>
            public string GetTokenParam()
            {
                if (string.IsNullOrEmpty(this.TokenParam))
                {
                    this.TokenParam = defaultTokenParam;
                }
                return this.TokenParam;
            }

            /// <summary>
            /// Default time the token is valid
            /// </summary>
            private static int defaultSecondsTicketValid = 10;

            /// <summary>
            /// the default folder used for logging
            /// </summary>
            private static string defaultLogFolder = "token_master";

            /// <summary>
            /// Default name of the param used to extract the token off the request url
            /// </summary>
            private static string defaultTokenParam = "t";

            /// <summary>
            /// Db host
            /// </summary>
            public string Host { get; set; }

            /// <summary>
            /// Db port
            /// </summary>
            public string Port { get; set; }

            /// <summary>
            /// Time the token will be valid for
            /// </summary>
            public int? SecondsTicketValid { get; set; }

            /// <summary>
            /// Log folder used by the exeption logger to dump the log file; defaults to ticket_master if not set
            /// </summary>
            public string LogFolder { get; set; }

            /// <summary>
            /// Name of the param that will be used to extract the token off the request url
            /// </summary>
            public string TokenParam { get; set; }

            /// <summary>
            /// Email account to report the problems to
            /// </summary>
            public Cartomatic.Utils.Email.EmailAccount Email { get; set; }
        }


        /// <summary>
        /// the connection multiplexer object
        /// </summary>
        private ConnectionMultiplexer redis { get; set; }

        /// <summary>
        /// db
        /// </summary>
        private IDatabase db { get; set; }

        /// <summary>
        /// state used for the async calls
        /// </summary>
        private object asyncState { get; set; }

        /// <summary>
        /// settings object
        /// </summary>
        private Settings settings { get; set; }
    }
}
