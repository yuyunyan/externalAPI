using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Sourceportal.SAP.Domain.Models.DB.Organizations
{
    [DataContract]
    public class CompanyDb
    {
        public ObjectId Id { get; set; }

        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("warehouses")]
        public BsonArray Warehouses { get; set; }

        [BsonElement("countryCode")]
        public string CountryCode { get; set; }

        [BsonElement("isPrimary")]
        public bool isPrimary { get; set; }
    }
}
