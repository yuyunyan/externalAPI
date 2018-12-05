using MongoDB.Driver;
using Sourceportal.SAP.Domain.Models.DB.Triggers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sourceportal.SAP.DB.Triggers
{
    public class TriggerRepository : ITriggerRepository
    {

        private static readonly string ConnectionString = ConfigurationManager.ConnectionStrings["MongoConnection"].ConnectionString;
        private readonly MongoClient _client;
        private static readonly string MongoDbName = ConfigurationManager.AppSettings["MongoSapDbName"];

        public TriggerRepository()
        {
            _client = new MongoClient(ConnectionString);
        }

        private IMongoCollection<TriggerDb> GetCollection()
        {
            var database = _client.GetDatabase(MongoDbName);
            return database.GetCollection<TriggerDb>("Triggers");
        }

        public TriggerDb AddTrigger(TriggerDb trigger)
        {
            GetCollection().InsertOne(trigger);
            return trigger;
        }
    }
}
