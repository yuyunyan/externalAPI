using Sourceportal.SAP.Domain.Models.Middleware;
using Sourceportal.SAP.Services.ApiService;
using Sourceportal.SAP.Services.ErrorManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sourceportal.SAP.Services.Middleware
{
    public class MiddlewareService : IMiddlewareService
    {
        private readonly IRestClient _restClient;

        public MiddlewareService(IRestClient restClient)
        {
            _restClient = restClient;
        }

        public string Sync<T>(MiddlewareSyncRequest<T> rqeuest) where T : MiddlewareSyncBase
        {
            var middlewareSyncResponse = _restClient.Post<MiddlewareSyncRequest<T>, MiddlewareSyncResponse>("transactions/add", rqeuest);

            if (!string.IsNullOrEmpty(middlewareSyncResponse.ErrorMessage))
            {
                var errorMessage = string.Format("Middleware error occured: {0}", middlewareSyncResponse.ErrorMessage);
                throw new GlobalApiException(errorMessage, "Middleware");
            }
            return middlewareSyncResponse.TransactionId;
        }
    }
}
