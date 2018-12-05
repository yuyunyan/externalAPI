using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Sourceportal.SAP.Domain.Models.Shared
{
    [DataContract]
    public class SalesArrangementResponse
    {
        [DataMember(Name = "salesOrganisationId")]
        public string SalesOrganisationId { get; set; }
        [DataMember(Name = "distributionChannelCode")]
        public string DistributionChannelCode { get; set; }
        [DataMember(Name = "classificationCode")]
        public string ClassificationCode { get; set; }
        [DataMember(Name = "transferLocationName")]
        public string TransferLocationName { get; set; }
        [DataMember(Name = "deliveryPriorityCode")]
        public string DeliveryPriorityCode { get; set; }
        [DataMember(Name = "completeDeliveryRequestedIndicator")]
        public bool CompleteDeliveryRequestedIndicator { get; set; }
        [DataMember(Name = "currencyCode")]
        public string CurrencyCode { get; set; }
        [DataMember(Name = "customerGroupCode")]
        public string CustomerGroupCode { get; set; }
        [DataMember(Name = "cashDiscountTermsCode")]
        public string CashDiscountTermsCode { get; set; }
    }
}
