using Sourceportal.SAP.Domain.Models.Responses.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Sourceportal.SAP.Domain.Models.Responses.QC
{
    [DataContract]
    public class QcInspectionResponse : BaseResponse
    {
        [DataMember(Name = "inventoryExternalId")]
        public string InventoryId { get; set; }

        [DataMember(Name = "inspectionQty")]
        public decimal InspectionQty { get; set; }

        [DataMember(Name = "documentExternalId")]
        public string DocumentExternalId { get; set; }

    }
}
