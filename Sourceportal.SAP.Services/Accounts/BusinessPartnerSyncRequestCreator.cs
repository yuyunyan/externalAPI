using Sourceportal.SAP.Domain.Models.Middleware;
using Sourceportal.SAP.Domain.Models.Middleware.Accounts;
using Sourceportal.SAP.Domain.Models.Middleware.Enums;
using Sourceportal.SAP.Domain.Models.Responses.Accounts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sourceportal.SAP.Services.SAP
{
    public class BusinessPartnerSyncRequestCreator : IBusinessPartnerSyncRequestCreator
    {


        public MiddlewareSyncRequest<BusinessPartnerSync> Create(BusinessPartnerIncomingResponse businessPartner)
        {
            var syncRequest = new MiddlewareSyncRequest<BusinessPartnerSync>(0, MiddlewareObjectTypes.Account.ToString());
            var bpSync = BusinessPartnerSync(businessPartner);
            syncRequest.Data = bpSync;
            return syncRequest;
        }

        private BusinessPartnerSync BusinessPartnerSync(BusinessPartnerIncomingResponse businessPartner)
        {
            var sync = new BusinessPartnerSync(businessPartner.AccountDetails.ExternalID);

            sync.AccountDetails = businessPartner.AccountDetails;
            sync.Contacts = businessPartner.Contacts;
            sync.Locations = businessPartner.Locations;

            return sync;
        }

    }
}
