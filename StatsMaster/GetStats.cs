using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace HGIS
{
    public partial class StatsMaster
    {

        public List<T> GetStats<T>() where T : StatsBase
        {
            var output = new List<T>();

            if (this.mongo == null)
            {
                throw new Exception("Stats module failure");
            }

            //use a 'complete' object so year gets assigned
            T s = new RequestStatsComplete(null, null).OutputAs<T>();

            if (string.IsNullOrEmpty(s.GetCollectionName()))
            {
                throw new ArgumentException("Invalid stats type");
            }

            //since got here it should be ok to proceed

            //this will pull all the shite off the specified collection 
            var cursor = this.mongocollections[s.GetCollectionName()].FindAs<T>(s.GetReadQueryBuilder());
            cursor.SetSortOrder(MongoDB.Driver.Builders.SortBy.Descending("Bytes"));
            cursor.SetLimit(100);
            
            return cursor.ToList<T>();
        }
    }
}
