using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sourceportal.SAP.Domain.Models.Responses.Accounts;
using Sourceportal.SAP.Domain.Models.Requests.Accounts;

namespace Sourceportal.SAP.Services.Accounts
{
    public interface IAccountService
    {
        BusinessPartnerResponse GetCustomerDetails(string externalId);
        BusinessPartnerResponse SetCustomerFromBusinessPartner(BusinessPartnerRequest customer, string billingCountry, string externalId);
        void HandleInboundSapRequest(string sapRequest);
    }
}
