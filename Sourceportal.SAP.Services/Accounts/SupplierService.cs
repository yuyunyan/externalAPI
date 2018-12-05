using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sourceportal.SAP.Services.SupplierManageService;
using Sourceportal.SAP.Services.SupplierQueryService;
using System.Configuration;
using Sourceportal.SAP.Domain.Models.Responses.Accounts;
using Sourceportal.SAP.Domain.Models.Responses.Shared;
using Sourceportal.SAP.Services.Utils;
using Sourceportal.SAP.Services.SAP;
using Sourceportal.SAP.Domain.Models.Shared;

namespace Sourceportal.SAP.Services.Accounts
{
    public class SupplierService : ISupplierService
    {
        private ManageSupplierInClient _manageClient;
        private QuerySupplierInClient _queryClient;
        private SapOrganizations _sapOrgs;
        private static readonly string SapUser = ConfigurationManager.AppSettings["SapUser"];
        private static readonly string SapPass = ConfigurationManager.AppSettings["SapPass"];
        private ISapDbHelper _sapService;

        public SupplierService(ISapDbHelper sapService)
        {
            _sapService = sapService;
            _manageClient = new ManageSupplierInClient("binding_SOAP12");
            _manageClient.ClientCredentials.UserName.UserName = SapUser;
            _manageClient.ClientCredentials.UserName.Password = SapPass;
            _queryClient = new QuerySupplierInClient("binding_SOAP12");
            _queryClient.ClientCredentials.UserName.UserName = SapUser;
            _queryClient.ClientCredentials.UserName.Password = SapPass;
        }

        public BusinessPartnerResponse SetSupplierFromBusinessPartner(string externalId, string billingCountry)
        {
            var query = new SupplierBundleMaintainRequestMessage_sync_V1();
            SupplierMaintainRequestBundleSupplier_V1 supplier = new SupplierMaintainRequestBundleSupplier_V1();

            supplier.InternalID = externalId;
            supplier.CreateFromBusinessPartnerIndicator = true;
            supplier.CreateFromBusinessPartnerIndicatorSpecified = true;
            supplier.actionCode = ActionCode.Item02;
            supplier.actionCodeSpecified = true;

            _sapOrgs = _sapService.SetOrgsFromDb();
            //supplier.paymentDataListCompleteTransmissionIndicator = true;
            //supplier.paymentDataListCompleteTransmissionIndicatorSpecified = true;
            //supplier.PaymentData = new SupplierMaintainRequestBundlePaymentData[_sapOrgs.Sales.Count];

            foreach(var company in _sapOrgs.Companies)
            {
                //int index = _sapOrgs.Companies.IndexOf(company);
                //var paymentData = new SupplierMaintainRequestBundlePaymentData();
                //paymentData.CompanyID = company.Company;

                //paymentData.AccountDeterminationCreditorGroupCode = new SupplierManageService.AccountDeterminationCreditorGroupCode();
               // paymentData.AccountDeterminationCreditorGroupCode.Value = company.CountryCode == billingCountry ? "4010" : "4020"; ;

                //paymentData.PaymentForm = new SupplierMaintainRequestBundlePaymentForm[1];
                //paymentData.PaymentForm[0] = new SupplierMaintainRequestBundlePaymentForm();
                //paymentData.PaymentForm[0].PaymentFormCode = "06";

                //supplier.PaymentData[index] = paymentData;
            }

            supplier.generalProductTaxExemptionListCompleteTransmissionIndicator = true;
            supplier.generalProductTaxExemptionListCompleteTransmissionIndicatorSpecified = true;
            supplier.GeneralProductTaxExemption = new SupplierMaintainRequestBundleGeneralProductTaxExemption[1];
            supplier.GeneralProductTaxExemption[0] = new SupplierMaintainRequestBundleGeneralProductTaxExemption();
            supplier.GeneralProductTaxExemption[0].CountryCode = "US";
            supplier.GeneralProductTaxExemption[0].TaxTypeCode = new SupplierManageService.TaxTypeCode { listID = "US", Value = "1"};
            supplier.GeneralProductTaxExemption[0].ReasonCode = new SupplierManageService.TaxExemptionReasonCode { listID = "US", Value = "O" };

            supplier.communicationArrangementListCompleteTransmissionIndicator = true;
            supplier.communicationArrangementListCompleteTransmissionIndicatorSpecified = true;
            supplier.CommunicationArrangement = new SupplierMaintainRequestBundleCommunicationArrangement[1];
            supplier.CommunicationArrangement[0] = new SupplierMaintainRequestBundleCommunicationArrangement();
            supplier.CommunicationArrangement[0].CommunicationMediumTypeCode = new SupplierManageService.CommunicationMediumTypeCode();
            supplier.CommunicationArrangement[0].CommunicationMediumTypeCode.Value = "PRT";
            supplier.CommunicationArrangement[0].OutputCopyNumberValueSpecified = true;
            supplier.CommunicationArrangement[0].OutputCopyNumberValue = 1;

            SupplierMaintainRequestBundleSupplier_V1[] supplierArray = new SupplierMaintainRequestBundleSupplier_V1[1];
            supplierArray[0] = supplier;
            query.Supplier = supplierArray;

            SupplierBundleMaintainConfirmationMessage_sync_V1 sapResponse = _manageClient.MaintainBundle_V1(query);
            return CreateSetSupplierResponse(sapResponse);
        }


        private BusinessPartnerResponse CreateSetSupplierResponse(SupplierBundleMaintainConfirmationMessage_sync_V1 sapResponse)
        {
            BusinessPartnerResponse response = new BusinessPartnerResponse();
            BaseResponse tempRes = SapLogParser.ParseSapResponseLog(sapResponse.Log);
            response.Errors = tempRes.Errors;
            response.Warnings = tempRes.Warnings;

            if (sapResponse.Supplier != null)
            {
                var getAllDetails = CreateSupplierQuery(sapResponse.Supplier[0].InternalID);

                if (getAllDetails.Supplier != null)
                {
                    response.BpExternalId = getAllDetails.Supplier[0].InternalID;

                    if (getAllDetails.Supplier[0].PurchasingData != null)
                    {
                        response.SupplierDetails = new AccountTypeDetails();
                        response.SupplierDetails.CurrencyID = getAllDetails.Supplier[0].PurchasingData.PurchaseOrderCurrencyCode;
                        response.SupplierDetails.PaymentTermID = getAllDetails.Supplier[0].PurchasingData.CashDiscountTermsCode != null ?
                            getAllDetails.Supplier[0].PurchasingData.CashDiscountTermsCode.Value : null;
                    }

                }

            }

            return response;
        }

        public BusinessPartnerResponse GetSupplierDetails(string externalId)
        {
            var queryResponse = CreateSupplierQuery(externalId);

            return ParseSupplierQuery(queryResponse);
        }

        private BusinessPartnerResponse ParseSupplierQuery(SupplierByElementsResponseMessage sapResponse)
        {
            BusinessPartnerResponse response = new BusinessPartnerResponse();
            BaseResponse tempRes = SapLogParser.ParseSapResponseLog(sapResponse.Log);
            response.Errors = tempRes.Errors;
            response.Warnings = tempRes.Warnings;

            if (sapResponse.Supplier != null)
            {
                response.BpExternalId = sapResponse.Supplier[0].InternalID;
                response.SupplierDetails = new AccountTypeDetails();

                if (sapResponse.Supplier[0].PurchasingData != null)
                {
                    response.SupplierDetails.CurrencyID = sapResponse.Supplier[0].PurchasingData.PurchaseOrderCurrencyCode;
                    response.SupplierDetails.PaymentTermID = sapResponse.Supplier[0].PurchasingData.CashDiscountTermsCode != null ?
                        sapResponse.Supplier[0].PurchasingData.CashDiscountTermsCode.Value : null;
                }

            }

            return response;
        }

        private SupplierByElementsResponseMessage CreateSupplierQuery(string externalId)
        {
            var query = new SupplierByElementsQueryMessage_sync();
            SelectionByIdentifier id = new SelectionByIdentifier();
            id.InclusionExclusionCode = "I";
            id.IntervalBoundaryTypeCode = "1";
            id.LowerBoundaryIdentifier = externalId;
            //id.UpperBoundaryIdentifier = externalId;
            SelectionByIdentifier[] ids = new SelectionByIdentifier[1];
            ids[0] = id;
            SupplierByElementsQuerySelectionByElements selectById = new SupplierByElementsQuerySelectionByElements();
            selectById.SelectionByInternalID = ids;
            query.SupplierSelectionByElements = selectById;

            return _queryClient.FindByElements(query);
        }
    }
}
