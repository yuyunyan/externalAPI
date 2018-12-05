using Sourceportal.SAP.Domain.Models.Shared;
using Sourceportal.SAP.Services.Accounts;
using Sourceportal.SAP.Services.SAP;
using Sourceportal.SAP.Services.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Xml;
using System.Xml.Serialization;

namespace Sourceportal.SAP.API.Controllers
{
    public class SapController : ApiController
    {
        private ISapService _sapService;

        public SapController(ISapService sapService)
        {
            _sapService = sapService;
        }

        [HttpPost]
        [Route("api/sap/test")]  //sap/update
        public string SapUpdate(Trigger request)
        {
            //depricated
            string host = HttpContext.Current.Request.Url.Host;
            SapXmlExtractor.LogTrigger(request, request.TriggerObject, host);
            return _sapService.ProcessTrigger(request);
        }
    }
}