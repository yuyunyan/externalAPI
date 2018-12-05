using Sourceportal.SAP.Domain.Models.Middleware;
using Sourceportal.SAP.Domain.Models.Responses.QC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sourceportal.SAP.Services.QC
{
    public interface IQcInspectionSyncRequestCreator
    {
        MiddlewareSyncRequest<QcInspectionSync> Create(QcInspectionResponse qc);
    }
}
