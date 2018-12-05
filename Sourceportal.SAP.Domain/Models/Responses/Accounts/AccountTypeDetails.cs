using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Sourceportal.SAP.Domain.Models.Responses.Accounts
{
    [DataContract]
    public class AccountTypeDetails
    {
        [DataMember(Name = "currencyID")]
        public string CurrencyID { get; set; }

        [DataMember(Name = "paymentTermID")]
        public string PaymentTermID { get; set; }

        [DataMember(Name = "creditLimit")]
        public decimal CreditLimit { get; set; }
    }
}
