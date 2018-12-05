using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sourceportal.SAP.Services.QcInspectionSapService;
using Sourceportal.SAP.Domain.Models.Responses.QC;
using Sourceportal.SAP.Domain.Models.Responses.Shared;
using Sourceportal.SAP.Services.Utils;
using Sourceportal.SAP.Domain.Models.Requests.Qc;


namespace Sourceportal.SAP.Services.QC
{
    public class QcInspectionService : IQcInspectionService
    {
        private static readonly string SapUser = ConfigurationManager.AppSettings["SapUser"];
        private static readonly string SapPass = ConfigurationManager.AppSettings["SapPass"];
        private readonly Y6VORP7OY_MaterialInspectionClient _sapClient;



        public QcInspectionService()
        {
            _sapClient = new Y6VORP7OY_MaterialInspectionClient("binding_SOAP12");
            _sapClient.ClientCredentials.UserName.UserName = SapUser;
            _sapClient.ClientCredentials.UserName.Password = SapPass;
        }

        public QcInspectionResponse QueryQcInspection(string externalId)
        {
            var inspectionQuery = new MaterialInspectionReadByIDQueryMessage_sync();
            inspectionQuery.MaterialInspection = new MaterialInspectionReadByIDQuery();
            inspectionQuery.MaterialInspection.ID = new BusinessTransactionDocumentID();
            inspectionQuery.MaterialInspection.ID.Value = externalId;

            MaterialInspectionReadByIDResponseMessage_sync sapResponse = new MaterialInspectionReadByIDResponseMessage_sync();

            try
            {
                sapResponse = _sapClient.Read(inspectionQuery);
            }
            catch (Exception e)
            {
                return new QcInspectionResponse { Errors = new List<string> { "Failure to read Qc Inspection: " + e.Message } };
            }

            return ParseSapQcInspectionRead(sapResponse);
        }

        public QcInspectionResponse ParseSapQcInspectionRead(MaterialInspectionReadByIDResponseMessage_sync sapResponse)
        {
            QcInspectionResponse response = new QcInspectionResponse();
            BaseResponse tempRes = SapLogParser.ParseSapResponseLog(sapResponse.Log);
            response.Errors = tempRes.Errors;
            response.Warnings = tempRes.Warnings;

            try
            {
                if (sapResponse.MaterialInspection != null)
                {
                    response.ExternalId = sapResponse.MaterialInspection.ID.Value;

                    if (sapResponse.MaterialInspection.IdentifiedStockKey != null)
                        response.InventoryId = sapResponse.MaterialInspection.IdentifiedStockKey.ID.Value;

                    if (sapResponse.MaterialInspection.InspectionQuantity != null)
                        response.InspectionQty = sapResponse.MaterialInspection.InspectionQuantity.Value;

                    if (sapResponse.MaterialInspection.Decision != null && sapResponse.MaterialInspection.Decision.AttachmentFolder != null
                        && sapResponse.MaterialInspection.Decision.AttachmentFolder.Document != null)
                    {
                        var documents = sapResponse.MaterialInspection.Decision.AttachmentFolder.Document;
                        var pdfDocument = documents.FirstOrDefault(x => x.CategoryCode == "3" && x.TypeCode != null && x.TypeCode.Value == "10001");
                        response.DocumentExternalId = (pdfDocument != null && pdfDocument.UUID != null) ? pdfDocument.UUID.Value : null;
                    }
                }
            }
            catch (Exception e)
            {
                response.Errors.Add("Failed to parse Qc Inspection Read Response from Sap: " + e.Message);
                return response;
            }

            return response;
        }

        public QcInspectionResponse UpdateQcInspection(QcInspectionRequest qc)
        {
            var inspection = new MaterialInspectionUpdateRequestMessage_sync();
            inspection.MaterialInspection = new MaterialInspectionUpdateRequest();
            inspection.MaterialInspection.ID = new BusinessTransactionDocumentID();
            inspection.MaterialInspection.ID.Value = qc.ExternalId;

            inspection.MaterialInspection.Decision = new MaterialInspectionUpdateRequestDecision();
            inspection.MaterialInspection.Decision.ActualInspectedQuantity = new NONNEGATIVE_Quantity();
            inspection.MaterialInspection.Decision.ActualInspectedQuantity.Value = qc.InspectionQty;

            inspection.MaterialInspection.Decision.NonconformingUnitsNumberValueSpecified = true;
            inspection.MaterialInspection.Decision.NonconformingUnitsNumberValue = qc.QtyFailed;

            if (!string.IsNullOrEmpty(qc.AcceptanceExternalId))
            {
                inspection.MaterialInspection.Decision.ProposedAcceptanceStatusCodeSpecified = true;
                inspection.MaterialInspection.Decision.ProposedAcceptanceStatusCode = (AcceptanceStatusCode)Enum.Parse(typeof(AcceptanceStatusCode), qc.AcceptanceExternalId);
            }        

            inspection.MaterialInspection.Decision.DecisionMakerEmployeeID = new EmployeeID();
            inspection.MaterialInspection.Decision.DecisionMakerEmployeeID.Value = qc.CompletedByExternalId;

            inspection.MaterialInspection.Decision.Code = new InspectionDecisionCode();
            inspection.MaterialInspection.Decision.Code.Value = qc.ResultExternalId;

            inspection.MaterialInspection.Decision.AttachmentFolder = new MaintenanceAttachmentFolder();
            inspection.MaterialInspection.Decision.AttachmentFolder.DocumentListCompleteTransmissionIndicatorSpecified = true;
            inspection.MaterialInspection.Decision.AttachmentFolder.DocumentListCompleteTransmissionIndicator = true;
            inspection.MaterialInspection.Decision.AttachmentFolder.Document = new MaintenanceAttachmentFolderDocument[1];
            var document = new MaintenanceAttachmentFolderDocument();
            document.VisibleIndicatorSpecified = true;
            document.VisibleIndicator = true;
            document.CategoryCode = "3";
            document.TypeCode = new DocumentTypeCode();
            document.TypeCode.Value = "10001";
            document.Name = qc.documentName;
            document.ExternalLinkWebURI = qc.documentUrl;

            if (!string.IsNullOrEmpty(qc.DocumentExternalId))
            {
                document.UUID = new UUID();
                document.UUID.Value = qc.DocumentExternalId;
                document.ActionCode = ActionCode.Item04;
                document.ActionCodeSpecified = true;
            }

            inspection.MaterialInspection.Decision.AttachmentFolder.Document[0] = document;

            try
            {
                _sapClient.Update(inspection);
            }
            catch (Exception e)
            {
                return new QcInspectionResponse { Errors = new List<string> { "Failure to update Qc Inspection: " + e.Message } };
            }



            return QueryQcInspection(qc.ExternalId);
        }

    }
}
