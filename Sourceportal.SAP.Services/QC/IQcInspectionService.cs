using Sourceportal.SAP.Domain.Models.Requests.Qc;
using Sourceportal.SAP.Domain.Models.Responses.QC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sourceportal.SAP.Services.QC
{
    public interface IQcInspectionService
    {
        QcInspectionResponse QueryQcInspection(string externalId);
        QcInspectionResponse UpdateQcInspection(QcInspectionRequest qc);
    }
}
