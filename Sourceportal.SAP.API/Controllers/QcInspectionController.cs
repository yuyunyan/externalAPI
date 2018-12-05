using Sourceportal.SAP.Domain.Models.Requests.Qc;
using Sourceportal.SAP.Domain.Models.Responses.QC;
using Sourceportal.SAP.Domain.Models.Shared;
using Sourceportal.SAP.Services.Middleware;
using Sourceportal.SAP.Services.QC;
using Sourceportal.SAP.Services.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace Sourceportal.SAP.API.Controllers
{
    public class QcInspectionController : ApiController
    {
        private readonly IQcInspectionService _qcInspectionService;
        private readonly IMiddlewareService _middlewareService;
        private readonly IQcInspectionSyncRequestCreator _qcSync;

        public QcInspectionController(IQcInspectionService qcInspectionService, IMiddlewareService middlewareService, IQcInspectionSyncRequestCreator qcSync)
        {
            _qcInspectionService = qcInspectionService;
            _middlewareService = middlewareService;
            _qcSync = qcSync;
        }


        [HttpPost]
        [Route("sap/qcinspection/set")]
        public void SapUpdateQcInspection(Trigger request)
        {
            string host = HttpContext.Current.Request.Url.Host;
            SapXmlExtractor.LogTrigger(request, ObjectTypeEnums.QcInspection.ToString(), host);
            var qc = _qcInspectionService.QueryQcInspection(request.ID);
            var syncRequest = _qcSync.Create(qc);
            _middlewareService.Sync(syncRequest);
        }

        [HttpPost]
        [Route("qcinspection/set")]
        public QcInspectionResponse UpdateQcInspection(QcInspectionRequest qc)
        {
            return _qcInspectionService.UpdateQcInspection(qc);
        }
    }
}