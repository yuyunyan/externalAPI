using System;
using System.Configuration;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Sourceportal.SAP.Domain.Models.Responses.Accounts;
using Sourceportal.SAP.Domain.Models.Requests.Accounts;
using Sourceportal.SAP.Services.AccountCustomerSapService;
using Sourceportal.SAP.Services.AccountManageCustomerService;
using PartyLifeCycleStatusCode = Sourceportal.SAP.Services.AccountManageCustomerService.PartyLifeCycleStatusCode;
using Sourceportal.SAP.Services.ApiService;
using Sourceportal.SAP.Domain.Models.WebApi.Responses;
using Sourceportal.SAP.Domain.Models.WebApi.Responses.Accounts;
using Sourceportal.SAP.Domain.Models.WebApi.Requests.Accounts;
using Sourceportal.SAP.Domain.Models.WebApi.Requests.Ownership;
using Sourceportal.SAP.Domain.Models.Responses.Shared;
using Sourceportal.SAP.Services.Utils;
using Sourceportal.SAP.Services.SAP;
using Sourceportal.SAP.Domain.Models.Shared;

namespace Sourceportal.SAP.Services.Accounts
{
    public class AccountService : IAccountService
    {
        private readonly IRestClient _restClient;
        private QueryCustomerInClient _queryClient;
        private ManageCustomerInClient _manageClient;
        private SapOrganizations _sapOrgs;
        private static readonly string SapUser = ConfigurationManager.AppSettings["SapUser"];
        private static readonly string SapPass = ConfigurationManager.AppSettings["SapPass"];
        private ISapDbHelper _sapService;

        public AccountService(IRestClient restClient, ISapDbHelper sapService)
        {
            _restClient = restClient;
            _sapService = sapService;
            _queryClient = new QueryCustomerInClient("binding_SOAP12");
            _manageClient = new ManageCustomerInClient("binding_SOAP12");
            _queryClient.ClientCredentials.UserName.UserName = SapUser;
            _queryClient.ClientCredentials.UserName.Password = SapPass;
            _manageClient.ClientCredentials.UserName.UserName = SapUser;
            _manageClient.ClientCredentials.UserName.Password = SapPass;

        }

        public BusinessPartnerResponse GetCustomerDetails(string externalId)
        {
            var queryResponse = CreateCustomerQuery(externalId);

            return ParseCustomerQuery(queryResponse);
        }

        private BusinessPartnerResponse ParseCustomerQuery(CustomerResponseMessage_sync sapResponse)
        {
            BusinessPartnerResponse response = new BusinessPartnerResponse();
            BaseResponse tempRes = SapLogParser.ParseSapResponseLog(sapResponse.Log);
            response.Errors = tempRes.Errors;
            response.Warnings = tempRes.Warnings;

            if (sapResponse.Customer != null)
            {
                response.BpExternalId = sapResponse.Customer[0].InternalID;
                response.CustomerDetails = new AccountTypeDetails();

                if (sapResponse.Customer[0].SalesArrangement != null)
                {                    
                    response.CustomerDetails.CurrencyID = sapResponse.Customer[0].SalesArrangement[0].CurrencyCode;
                    response.CustomerDetails.PaymentTermID = sapResponse.Customer[0].SalesArrangement[0].CashDiscountTermsCode != null ?
                        sapResponse.Customer[0].SalesArrangement[0].CashDiscountTermsCode.Value : null;

                    if (sapResponse.Customer[0].PaymentData != null && sapResponse.Customer[0].PaymentData[0].CreditLimitAmount != null)
                        response.CustomerDetails.CreditLimit = sapResponse.Customer[0].PaymentData[0].CreditLimitAmount.Value;
                }

            }

            return response;
        }

        private CustomerResponseMessage_sync CreateCustomerQuery(string externalId)
        {
            AccountDetailsResponse response = new AccountDetailsResponse();

            var query = new CustomerByElementsQueryMessage_sync();
            CustomerSelectionByInternalID id = new CustomerSelectionByInternalID();
            id.InclusionExclusionCode = "I";
            id.IntervalBoundaryTypeCode = "1";
            id.LowerBoundaryInternalID = externalId;
            CustomerSelectionByInternalID[] ids = new CustomerSelectionByInternalID[10];
            ids[0] = id;
            CustomerSelectionByElements selectById = new CustomerSelectionByElements();
            selectById.SelectionByInternalID = ids;
            query.CustomerSelectionByElements = selectById;

            return _queryClient.FindByElements(query);
        }

        public BusinessPartnerResponse SetCustomerFromBusinessPartner(BusinessPartnerRequest customer, string billingCountry, string externalId)
        {
            var query = new CustomerBundleMaintainRequestMessage_sync_V1();
            CustomerMaintainRequestBundleCustomer_V1 sapCustomer = new CustomerMaintainRequestBundleCustomer_V1();

            sapCustomer.InternalID = externalId;
            sapCustomer.CreateFromBusinessPartnerIndicator = true;
            sapCustomer.CreateFromBusinessPartnerIndicatorSpecified = true;
            sapCustomer.actionCode = ActionCode.Item02;
            sapCustomer.actionCodeSpecified = true;

            sapCustomer.directResponsibilityListCompleteTransmissionIndicator = true;
            sapCustomer.directResponsibilityListCompleteTransmissionIndicatorSpecified = true;
            sapCustomer.DirectResponsibility = new CustomerMaintainRequestBundleDirectResponsibility[1];
            sapCustomer.DirectResponsibility[0] = new CustomerMaintainRequestBundleDirectResponsibility();
            sapCustomer.DirectResponsibility[0].PartyRoleCode = "142";
            sapCustomer.DirectResponsibility[0].EmployeeID = new AccountManageCustomerService.EmployeeID();
            sapCustomer.DirectResponsibility[0].EmployeeID.Value = customer.BpDetails.Ownership.LeadOwner.ExternalId;

            _sapOrgs = _sapService.SetOrgsFromDb();
            sapCustomer.salesArrangementListCompleteTransmissionIndicatorSpecified = true;
            sapCustomer.salesArrangementListCompleteTransmissionIndicator = true;
            sapCustomer.SalesArrangement = new CustomerMaintainRequestBundleSalesArrangement[_sapOrgs.Sales.Count];
            //sapCustomer.PaymentData = new CustomerMaintainRequestBundlePaymentData[_sapOrgs.Sales.Count];

            foreach (var sales in _sapOrgs.Sales)
            {
                int index = _sapOrgs.Sales.IndexOf(sales);

                //sales arrangement
                var salesArrangement = new CustomerMaintainRequestBundleSalesArrangement();
                salesArrangement.SalesOrganisationID = sales;
                salesArrangement.DistributionChannelCode = new AccountManageCustomerService.DistributionChannelCode();
                salesArrangement.DistributionChannelCode.Value = "01";
                salesArrangement.CompleteDeliveryRequestedIndicator = false;
                salesArrangement.CompleteDeliveryRequestedIndicatorSpecified = true;

                //payment data
                //var paymentData = new CustomerMaintainRequestBundlePaymentData();
                //paymentData.CompanyID = company.Company;
                //paymentData.AccountDeterminationDebtorGroupCode = new AccountManageCustomerService.AccountDeterminationDebtorGroupCode();
                //paymentData.AccountDeterminationDebtorGroupCode.Value = company.CountryCode == billingCountry ? "4010" : "4020";
                //paymentData.PaymentForm = new CustomerMaintainRequestBundlePaymentDataPaymentForm[1];
                //paymentData.PaymentForm[0] = new CustomerMaintainRequestBundlePaymentDataPaymentForm();
                //paymentData.PaymentForm[0].PaymentFormCode = "06";

                //sapCustomer.PaymentData[index] = paymentData;
                sapCustomer.SalesArrangement[index] = salesArrangement;
            }

            sapCustomer.generalProductTaxExemptionListCompleteTransmissionIndicator = true;
            sapCustomer.generalProductTaxExemptionListCompleteTransmissionIndicatorSpecified = true;
            sapCustomer.GeneralProductTaxExemption = new CustomerMaintainRequestBundleGeneralProductTaxExemption[1];
            sapCustomer.GeneralProductTaxExemption[0] = new CustomerMaintainRequestBundleGeneralProductTaxExemption();
            sapCustomer.GeneralProductTaxExemption[0].CountryCode = "US";
            sapCustomer.GeneralProductTaxExemption[0].TaxTypeCode = new AccountManageCustomerService.TaxTypeCode { listID = "US", Value = "1" }; 
            sapCustomer.GeneralProductTaxExemption[0].ReasonCode = new AccountManageCustomerService.TaxExemptionReasonCode { listID = "US", Value = "O" };

            //shipping instructions
            //sapCustomer.Text = new CustomerMaintainRequestBundleText[1];
            //sapCustomer.Text[0] = new CustomerMaintainRequestBundleText();
            //sapCustomer.textListCompleteTransmissionIndicator = true;
            //sapCustomer.textListCompleteTransmissionIndicatorSpecified = true;
            //sapCustomer.Text[0].ContentText = customer.BpDetails.ShippingInstructions;

            CustomerMaintainRequestBundleCustomer_V1[] customerArray = new CustomerMaintainRequestBundleCustomer_V1[1];
            customerArray[0] = sapCustomer;
            query.Customer = customerArray;

            CustomerBundleMaintainConfirmationMessage_sync_V1 sapResponse = _manageClient.MaintainBundle_V1(query);
            return CreateSetCustomerResponse(sapResponse);
        }


        private BusinessPartnerResponse CreateSetCustomerResponse(CustomerBundleMaintainConfirmationMessage_sync_V1 sapResponse)
        {
            BusinessPartnerResponse response = new BusinessPartnerResponse();
            BaseResponse tempRes = SapLogParser.ParseSapResponseLog(sapResponse.Log);
            response.Errors = tempRes.Errors;
            response.Warnings = tempRes.Warnings;

            if (sapResponse.Customer != null)
            {
                var getAllDetails = CreateCustomerQuery(sapResponse.Customer[0].InternalID);

                if (getAllDetails.Customer != null)
                {
                    response.BpExternalId = getAllDetails.Customer[0].InternalID;

                    if (getAllDetails.Customer[0].SalesArrangement != null)
                    {
                        response.CustomerDetails = new AccountTypeDetails();
                        response.CustomerDetails.CurrencyID = getAllDetails.Customer[0].SalesArrangement[0].CurrencyCode;
                        response.CustomerDetails.PaymentTermID = getAllDetails.Customer[0].SalesArrangement[0].CashDiscountTermsCode != null ?
                            getAllDetails.Customer[0].SalesArrangement[0].CashDiscountTermsCode.Value : null;

                        if (getAllDetails.Customer[0].PaymentData != null && getAllDetails.Customer[0].PaymentData[0].CreditLimitAmount != null)
                            response.CustomerDetails.CreditLimit = getAllDetails.Customer[0].PaymentData[0].CreditLimitAmount.Value;
                    }

                }

            }
            
            return response;
        }

        public void SetLocationAndContactIds(AccountDetailsResponse returnResponse, CustomerResponseMessage_sync queryResponse)
        {
            if (queryResponse.Customer[0].AddressInformation != null)
            {
                returnResponse.LocationExternalIds = new List<string>();
                int locationCount = queryResponse.Customer[0].AddressInformation.Length;
                for (int i = 0; i < locationCount; i++)
                {                    
                    returnResponse.LocationExternalIds.Add(queryResponse.Customer[0].AddressInformation[i].UUID.Value);
                }
            }

            if (queryResponse.Customer[0].ContactPerson != null)
            {
                returnResponse.ContactExternalIds = new List<string>();
                int contactCount = queryResponse.Customer[0].ContactPerson.Length;
                for (int i = 0; i < contactCount; i++)
                {
                    returnResponse.ContactExternalIds.Add(queryResponse.Customer[0].ContactPerson[i].BusinessPartnerContactInternalID);
                }
            }
        }        

        public void HandleInboundSapRequest(string sapRequest)
        {
            string ConnectionString = ConfigurationManager.ConnectionStrings["SourcePortalConnection"].ConnectionString;

            var con = new SqlConnection(ConnectionString);

            String query = "INSERT INTO dbo.tempSapRequests (Request) VALUES (@Request)";

            using (SqlCommand command = new SqlCommand(query, con))
            {
                command.Parameters.Add("@Request", sapRequest);

                con.Open();
                int result = command.ExecuteNonQuery();

                if(result < 0)
                    Console.WriteLine("Error Inserting SAP Request into Database!");
            }
        }

    }
}
