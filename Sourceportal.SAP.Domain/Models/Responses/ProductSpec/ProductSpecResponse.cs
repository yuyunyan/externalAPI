using Sourceportal.SAP.Domain.Models.Responses.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Sourceportal.SAP.Domain.Models.Responses.ProductSpec
{
    [DataContract]
    public class ProductSpecResponse : BaseResponse
    {
        [DataMember(Name = "productSpec")]
        public List<ProductSpecObject> ProductSpec { get; set; }

    }

    [DataContract]
    public class ProductSpecObject
    {
        [DataMember(Name = "productSpecId")]
        public string ProductSpecId { get; set; }
        [DataMember(Name = "versionUUID")]
        public string VersionUUID { get; set; }
        [DataMember(Name = "materialId")]
        public string MaterialId { get; set; }
    }
}
