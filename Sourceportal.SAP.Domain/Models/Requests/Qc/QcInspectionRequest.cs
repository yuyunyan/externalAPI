using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Sourceportal.SAP.Domain.Models.Requests.Qc
{
    [DataContract]
    public class QcInspectionRequest
    {
        [DataMember(Name = "externalId")]
        public string ExternalId { get; set; }

        [DataMember(Name = "inspectionStatusExternalId")]
        public string InspectionStatusExternalId { get; set; }

        [DataMember(Name = "inspectionQty")]
        public int InspectionQty { get; set; }

        [DataMember(Name = "qtyFailed")]
        public int QtyFailed { get; set; }

        [DataMember(Name = "completedByExternalId")]
        public string CompletedByExternalId { get; set; }

        [DataMember(Name = "acceptanceExternalId")]
        public string AcceptanceExternalId { get; set; }

        [DataMember(Name = "resultExternalId")]
        public string ResultExternalId { get; set; }

        [DataMember(Name = "documentExternalId")]
        public string DocumentExternalId { get; set; }

        [DataMember(Name = "documentName")]
        public string documentName { get; set; }

        [DataMember(Name = "documentUrl")]
        public string documentUrl { get; set; }
    }
}
