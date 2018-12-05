using Sourceportal.SAP.Domain.Models.Requests.Materials;
using Sourceportal.SAP.Domain.Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Sourceportal.SAP.Domain.Models.Requests.SalesOrders
{
    [DataContract]
    public class SalesOrderDetailsRequest
    {
        [DataMember(Name = "id")]
        public int SalesOrderId { get; set; }

        [DataMember(Name = "externalId")]
        public string ExternalId { get; set; }
        
        [DataMember(Name = "accountExternalId")]
        public string AccountExternalId { get; set; }

        [DataMember(Name = "contactExternalId")]
        public string ContactExternalId { get; set; }

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

        [DataMember(Name = "customerPo")]
        public string CustomerPo { get; set; }

        [DataMember(Name = "ownership")]
        public Ownership Ownership { get; set; }

        [DataMember(Name = "ultDestinationId")]
        public string UltDestinationId { get; set; }

        [DataMember(Name = "freightAccount")]
        public string FreightAccount { get; set; }

        [DataMember(Name = "incotermLocation")]
        public string IncotermLocation { get; set; }

        [DataMember(Name = "lines")]
        public List<SalesOrderLineDetail> SOLines { get; set; }

    }


    [DataContract]
    public class SalesOrderLineDetail 
    {
        [DataMember(Name = "lineNum")]
        public int LineNum { get; set; }

        [DataMember(Name = "itemDetails")]
        public MaterialRequest ItemDetails { get; set; }

        [DataMember(Name = "customerLine")]
        public int CustomerLine { get; set; }

        [DataMember(Name = "quantity")]
        public int Qty { get; set; }

        [DataMember(Name = "price")]
        public decimal Price { get; set; }

        [DataMember(Name = "dueDate")]
        public string DueDate { get; set; }

        [DataMember(Name = "customerPartNum")]
        public string CustomerPartNum { get; set; }

        [DataMember(Name = "shipDate")]
        public string ShipDate { get; set; }

        [DataMember(Name = "cost")]
        public decimal Cost { get; set; }

        [DataMember(Name = "dateCode")]
        public string DateCode { get; set; }

        [DataMember(Name = "packagingId")]
        public string PackagingId { get; set; }

        [DataMember(Name = "packageConditionalId")]
        public string PackageConditionalId { get; set; }

        [DataMember(Name = "productSpec")]
        public string ProductSpec { get; set; }

    }

}
