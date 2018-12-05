using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Sourceportal.SAP.Domain.Models.DB.Organizations
{
    [DataContract]
    public class OrganizationDb
    {
        public ObjectId Id { get; set; }

        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("type")]
        public BsonArray Types { get; set; }

        [BsonElement("primarySales")]
        public bool PrimarySales { get; set; }
    }
}
