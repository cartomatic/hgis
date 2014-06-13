using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HGIS
{
    public partial class TokenMaster
    {
        /// <summary>
        /// saves the passed token to the db;
        /// Note: does not check if token exists, so will overwrite it if already in the db!
        /// </summary>
        /// <param name="token"></param>
        public void CreateToken(string token)
        {
            //just save the token
            db.StringSet(token, "x", expiry: TimeSpan.FromSeconds(this.settings.GetSecondsTicketValid()));
        }

        /// <summary>
        /// Generates a new token and saves it to a redis db
        /// </summary>
        /// <returns></returns>
        public string GimmeToken()
        {
            //generate new token
            string newToken = System.Guid.NewGuid().ToString();

            CreateToken(newToken);

            return newToken;
        }


        /// <summary>
        /// Checks whether the token is valid
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public bool CheckIfTokenValid(string ticket)
        {
            bool ticketValid = false;

            try
            {
                string dbticket = db.StringGet(ticket);

                if (dbticket == "x")
                {
                    ticketValid = true;
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
        /// <param name="ticket"></param>
        /// <returns></returns>
        public async Task<bool> CheckIfTokenValidAsync(string ticket)
        {
            bool ticketValid = false;

            try
            {
                string dbticket = await db.StringGetAsync(ticket);

                if (dbticket == "x")
                {
                    ticketValid = true;
                }
            }
            catch (Exception ex)
            {
                LogException(ex);
            }

            return ticketValid;
        }

    }
}
