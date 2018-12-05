using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Sourceportal.SAP.Domain.Models.Requests.Materials
{
    [DataContract]
    public class MaterialRequest
    {
        [DataMember(Name = "id")]
        public int Id { get; set; }

        [DataMember(Name = "externalId")]
        public string ExternalId { get; set; }

        [DataMember(Name = "mfg")]
        public string Mfg { get; set; }

        [DataMember(Name = "commodityExternalId")]
        public string CommodityExternalID { get; set; }

        [DataMember(Name = "sourceDataId")]
        public int SourceDataID { get; set; }

        [DataMember(Name = "partNumber")]
        public string PartNumber { get; set; }

        [DataMember(Name = "partNumberStrip")]
        public string PartNumberStrip { get; set; }

        [DataMember(Name = "description")]
        public string Description { get; set; }

        [DataMember(Name = "eurohs")]
        public bool EURoHS { get; set; }

        [DataMember(Name = "eccn")]
        public string ECCN { get; set; }

        [DataMember(Name = "hts")]
        public string HTS { get; set; }

        [DataMember(Name = "msl")]
        public string MSL { get; set; }

        [DataMember(Name = "datasheetUrl")]
        public string DatasheetURL { get; set; }
    }
}
