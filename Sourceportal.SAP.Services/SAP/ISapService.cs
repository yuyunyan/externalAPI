using Sourceportal.SAP.Domain.Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sourceportal.SAP.Services.SAP
{
    public interface ISapService
    {
        string ProcessTrigger(Trigger sapTrigger);
        void HandleInboundSapRequest(string sapRequest);


    }
}
