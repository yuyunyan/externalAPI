using Sourceportal.SAP.Domain.Models.Middleware;
using Sourceportal.SAP.Domain.Models.Middleware.Accounts;
using Sourceportal.SAP.Domain.Models.Responses.Accounts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sourceportal.SAP.Services.SAP
{
    public interface IBusinessPartnerSyncRequestCreator
    {
        MiddlewareSyncRequest<BusinessPartnerSync> Create(BusinessPartnerIncomingResponse businessPartner);
    }
}
