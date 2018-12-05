using Sourceportal.SAP.Domain.Models.Requests.Materials;
using Sourceportal.SAP.Domain.Models.Responses.Materials;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sourceportal.SAP.Services.Materials
{
    public interface IMaterialService
    {
        MaterialResponse QueryMaterial(string externalId);
        MaterialResponse SetMaterial(MaterialRequest material);
    }
}
