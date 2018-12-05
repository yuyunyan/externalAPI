using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Sourceportal.SAP.Domain.Models.DB.Triggers
{
    [DataContract]
    public class TriggerDb
    {
        public ObjectId Id { get; set; }

        [BsonElement("trigger")]
        public string Trigger { get; set; }

        [BsonElement("created")]
        public BsonDateTime Created { get; set; }

        [BsonElement("objectType")]
        public string ObjectType { get; set; }

        [BsonElement("host")]
        public string Host { get; set; }

    }
}
