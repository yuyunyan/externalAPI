using Sourceportal.SAP.Domain.Models.Requests.Materials;
using Sourceportal.SAP.Domain.Models.Responses.Materials;
using Sourceportal.SAP.Services.Materials;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace Sourceportal.SAP.API.Controllers
{
    public class MaterialsController : ApiController
    {

        private readonly IMaterialService _materialService;

        public MaterialsController(IMaterialService materialService)
        {
            _materialService = materialService;
        }

        [HttpGet]
        [Route("material/get")]
        public MaterialResponse GetMaterial(string externalId)
        {
            return _materialService.QueryMaterial(externalId);
        }

        [HttpPost]
        [Route("material/set")]
        public MaterialResponse SetMaterial(MaterialRequest material)
        {
            return _materialService.SetMaterial(material);
        }


    }
}