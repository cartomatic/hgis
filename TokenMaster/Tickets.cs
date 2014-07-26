using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Web;

namespace HGIS
{
    public partial class TokenMaster
    {
        /// <summary>
        /// saves the passed token and its value in the db;
        /// Note: does not check if token exists, so will overwrite it if already in the db!
        /// </summary>
        /// <param name="token"></param>
        public void CreateToken(string token, string tokenValue)
        {
            //make sure the db object is available
            if (db == null) return;

            //just save the token
            db.StringSet(token, tokenValue, expiry: TimeSpan.FromSeconds(this.settings.GetSecondsTicketValid()));
        }

        /// <summary>
        /// Generates a new token and saves it to a redis db
        /// </summary>
        /// <returns></returns>
        public string GimmeToken(HttpRequest request)
        {
            //generate new token
            string newToken = System.Guid.NewGuid().ToString();
            string tokenValue = Cartomatic.Utils.Http.GetCallerFingerPrint(request);

            CreateToken(newToken, tokenValue);

            return newToken;
        }


        /// <summary>
        /// Checks whether the token is valid
        /// </summary>
        /// <returns></returns>
        public bool CheckIfTokenValid(HttpRequest request)
        {
            bool ticketValid = false;

            //make sure the db object is available
            if (db == null) return false;

            try
            {
                var token = request.QueryString[this.settings.GetTokenParam()];
                if (!string.IsNullOrEmpty(token))
                {
                    string dbticket = db.StringGet(token);

                    if (dbticket == Cartomatic.Utils.Http.GetCallerFingerPrint(request))
                    {
                        ticketValid = true;
                    }
                }
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
            
            return ticketValid;
        }

        /// <summary>
        /// Asynchronously checks whether the token is valid
        /// </summary>
        /// <returns></returns>
        public async Task<bool> CheckIfTokenValidAsync(HttpRequest request)
        {
            bool ticketValid = false;

            //make sure the db object is available
            if (db == null) return false;

            try
            {
                var token = request.QueryString[this.settings.GetTokenParam()];

                if (!string.IsNullOrEmpty(token))
                {
                    string dbticket = await db.StringGetAsync(token);

                    if (dbticket == Cartomatic.Utils.Http.GetCallerFingerPrint(request))
                    {
                        ticketValid = true;
                    }
                }               
            }
            catch (Exception ex)
            {
                LogException(ex);
            }

            return ticketValid;
        }


        /// <summary>
        /// gets the configured token param
        /// </summary>
        /// <returns></returns>
        public string GetTokenParam()
        {
            return this.settings.GetTokenParam();
        }

    }
}
