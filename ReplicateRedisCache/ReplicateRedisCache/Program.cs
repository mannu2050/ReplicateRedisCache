using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReplicateRedisCache
{
    class Program
    {
        static void Main(string[] args)
        {
            ConnectionMultiplexer sourceConnection = ConnectionMultiplexer.Connect(ConfigurationManager.AppSettings["SourceConnection"].ToString());
            ConnectionMultiplexer destinationConnection = ConnectionMultiplexer.Connect(ConfigurationManager.AppSettings["DestinationConnection"].ToString());
            IDatabase destinationDB = destinationConnection.GetDatabase();
            IDatabase sourceDB = sourceConnection.GetDatabase();
            

            var endPoints = sourceConnection.GetEndPoints();
            var db = sourceConnection.GetServer(endPoints.First());
            int keysAdded=0, keysUpdated = 0;
            while (true)
            {
                keysUpdated = 0;
                    keysAdded = 0;
                foreach (var key in db.Keys())
                {
                    if (!destinationDB.KeyExists(key))
                    {
                        keysAdded++;
                        destinationDB.SetAdd(key, sourceDB.StringGet(key));
                        Console.Write(key.ToString() + " & Value=" + sourceDB.StringGet(key));
                    }
                    else if (destinationDB.StringGet(key) != sourceDB.StringGet(key))
                    {
                        keysUpdated++;
                        destinationDB.StringSet(key, sourceDB.StringGet(key));
                    }
                }
                if (keysUpdated == 0 || keysAdded == 0)
                    break;
            }
        }

        // Redis Connection string info
        private static Lazy<ConnectionMultiplexer> lazyConnection = new Lazy<ConnectionMultiplexer>(() =>
        {
            string cacheConnection =System.Configuration.ConfigurationManager.AppSettings["CacheConnection"].ToString();
            return ConnectionMultiplexer.Connect(cacheConnection);
        });

        public static ConnectionMultiplexer Connection
        {
            get
            {
                return lazyConnection.Value;
            }
        }
    }
}
