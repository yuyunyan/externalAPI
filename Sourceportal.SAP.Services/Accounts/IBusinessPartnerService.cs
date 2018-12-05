using Sourceportal.SAP.Domain.Models.Requests.Accounts;
using Sourceportal.SAP.Domain.Models.Responses.Accounts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sourceportal.SAP.Services.Accounts
{
    public interface IBusinessPartnerService
    {
        //BusinessPartnerQueryResponse GetBusinessPartner(string externalId);
        BusinessPartnerIncomingResponse GetBusinessPartnerForIncoming(string externalId);

        BusinessPartnerIncomingResponse MapBusinessPartnerResponse(BusinessPartnerResponse bpr);
        BusinessPartnerResponse ProcessBusinessPartnerRequest(BusinessPartnerRequest businessPartner);
    }
}
