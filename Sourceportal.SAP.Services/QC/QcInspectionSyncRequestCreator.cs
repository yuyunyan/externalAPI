using Sourceportal.SAP.Domain.Models.Middleware;
using Sourceportal.SAP.Domain.Models.Middleware.Enums;
using Sourceportal.SAP.Domain.Models.Responses.QC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sourceportal.SAP.Services.QC
{
    public class QcInspectionSyncRequestCreator : IQcInspectionSyncRequestCreator
    {

        public MiddlewareSyncRequest<QcInspectionSync> Create(QcInspectionResponse qc)
        {
            var syncRequest = new MiddlewareSyncRequest<QcInspectionSync>(0, MiddlewareObjectTypes.QcInspection.ToString());
            var qcSync = QcSync(qc);
            syncRequest.Data = qcSync;
            return syncRequest;
        }

        private QcInspectionSync QcSync(QcInspectionResponse qc)
        {
            var sync = new QcInspectionSync(qc.ExternalId);

            sync.InventoryId = qc.InventoryId;
            sync.InspectionQty = qc.InspectionQty;

            return sync;
        }


    }
}
