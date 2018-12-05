using System.Net.Http;
using System.Web.Http;
using Sourceportal.SAP.Domain.Models.Responses.Accounts;
using Sourceportal.SAP.Domain.Models.Requests.Accounts;
using Sourceportal.SAP.Services.Accounts;
using Sourceportal.SAP.Services.Utils;
using Sourceportal.SAP.Services.SAP;
using Sourceportal.SAP.Services.Middleware;
using Sourceportal.SAP.Domain.Models.Shared;
using System.Web;

namespace Sourceportal.SAP.API.Controllers
{
    public class AccountsController : ApiController
    {
        private readonly IBusinessPartnerService _businessPartnerService;
        private readonly IBusinessPartnerSyncRequestCreator _businessPartnerSyncRequestCreator;
        private readonly IMiddlewareService _middlewareService;

        public AccountsController(IBusinessPartnerService businessPartnerService, 
            IBusinessPartnerSyncRequestCreator businessPartnerSyncRequestCreator, IMiddlewareService middlewareService)
        {
            _businessPartnerService = businessPartnerService;
            _businessPartnerSyncRequestCreator = businessPartnerSyncRequestCreator;
            _middlewareService = middlewareService;
        }

        [HttpPost]
        [Route("businesspartner/set")]
        public BusinessPartnerIncomingResponse SetBusinessPartner(BusinessPartnerRequest bp)
        {
            return _businessPartnerService.MapBusinessPartnerResponse(_businessPartnerService.ProcessBusinessPartnerRequest(bp));
        }


        [HttpPost]
        [Route("sap/businesspartner/set")] 
        public void SapUpdateBusinessPartner(Trigger request)
        {
            string host = HttpContext.Current.Request.Url.Host;
            SapXmlExtractor.LogTrigger(request, ObjectTypeEnums.BusinessPartner.ToString(), host);
            var businessPartner = _businessPartnerService.GetBusinessPartnerForIncoming(request.ID);
            var syncRequest = _businessPartnerSyncRequestCreator.Create(businessPartner);
            _middlewareService.Sync(syncRequest);
        }

        [HttpPost]
        [Route("sap/accounthierarchy/set")] 
        public void SapUpdateAccountHierarchy(Trigger request)
        {
            string host = HttpContext.Current.Request.Url.Host;
            SapXmlExtractor.LogTrigger(request, ObjectTypeEnums.AccountHierarchy.ToString(), host);
        }

    }
}