using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Sourceportal.SAP.Domain.Models.Middleware
{
    [DataContract]
    public class QcInspectionSync : MiddlewareSyncBase
    {
        public QcInspectionSync(string externalId) : base(externalId)
        {
        }

        [DataMember(Name = "inventoryExternalId")]
        public string InventoryId { get; set; }

        [DataMember(Name = "inspectionQty")]
        public decimal InspectionQty { get; set; }
    }
}
