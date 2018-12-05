using Sourceportal.SAP.Domain.Models.Requests.Materials;
using Sourceportal.SAP.Domain.Models.Responses.Shared;
using Sourceportal.SAP.Domain.Models.Shared;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Sourceportal.SAP.Domain.Models.Requests.PurchaseOrders
{
    [DataContract]
    public class PurchaseOrderRequest : BaseResponse
    {
        [DataMember(Name = "id")]
        public int Id { get; set; }

        [DataMember(Name = "externalId")]
        public string ExternalId { get; set; }

        [DataMember(Name = "accountExternalId")]
        public string AccountExternalId { get; set; }

        [DataMember(Name = "incoTermExternalId")]
        public string IncotermID { get; set; }

        [DataMember(Name = "paymentTermExternalId")]
        public string PaymentTermID { get; set; }

        [DataMember(Name = "currencyExternalId")]
        public string CurrencyID { get; set; }

        [DataMember(Name = "orgExternalId")]
        public string OrganizationID { get; set; }

        [DataMember(Name = "orderDate")]
        public string OrderDate { get; set; }

        [DataMember(Name = "toLocationExternalId")]
        public string ToLocationExternalId { get; set; }

        [DataMember(Name = "ownership")]
        public Ownership Ownership { get; set; }

        [DataMember(Name = "lines")]
        public List<PurchaseOrderLine> Lines { get; set; }

    }

    [DataContract]
    public class PurchaseOrderLine
    {
        [DataMember(Name = "lineNum")]
        public int LineNum { get; set; }

        [DataMember(Name = "itemDetails")]
        public MaterialRequest ItemDetails { get; set; }

        [DataMember(Name = "quantity")]
        public int Qty { get; set; }

        [DataMember(Name = "cost")]
        public decimal Cost { get; set; }

        [DataMember(Name = "packagingTypeExternalId")]
        public string PackagingTypeExternalId { get; set; }

        [DataMember(Name = "promisedDate")]
        public string PromisedDate { get; set; }

        [DataMember(Name = "dueDate")]
        public string DueDate { get; set; }

        [DataMember(Name = "isSpecBuy")]
        public bool IsSpecBuy { get; set; }

        [DataMember(Name = "specBuyForUser")]
        public string SpecBuyForUser { get; set; }

        [DataMember(Name = "specBuyForAccount")]
        public string SpecBuyForAccount { get; set; }

        [DataMember(Name = "specBuyReason")]
        public string SpecBuyReason { get; set; }

    }

}
