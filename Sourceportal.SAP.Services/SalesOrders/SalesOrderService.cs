using Sourceportal.SAP.Domain.Models.Requests.SalesOrders;
using Sourceportal.SAP.Domain.Models.Responses.Accounts;
using Sourceportal.SAP.Domain.Models.Responses.ProductSpec;
using Sourceportal.SAP.Domain.Models.Responses.SalesOrders;
using Sourceportal.SAP.Domain.Models.Responses.Shared;
using Sourceportal.SAP.Domain.Models.Shared;
using Sourceportal.SAP.Services.ApiService;
using Sourceportal.SAP.Services.Materials;
using Sourceportal.SAP.Services.ProductSpec;
using Sourceportal.SAP.Services.SalesOrderManageService;
using Sourceportal.SAP.Services.SalesOrderQueryService;
using Sourceportal.SAP.Services.Utils;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sourceportal.SAP.Services.SalesOrders
{
    public class SalesOrderService : ISalesOrderService
    {
        private readonly IProductSpecService _productSpecService;
        private ManageSalesOrderInClient _manageClient;
        private QuerySalesOrderInClient _queryClient;
        private static readonly string SapUser = ConfigurationManager.AppSettings["SapUser"];
        private static readonly string SapPass = ConfigurationManager.AppSettings["SapPass"];
        private readonly IMaterialService _materialService;

        public SalesOrderService(IProductSpecService productSpecService, IMaterialService materialService)
        {
            _productSpecService = productSpecService;
            _queryClient = new QuerySalesOrderInClient("binding_SOAP12");
            _manageClient = new ManageSalesOrderInClient("binding_SOAP12");
            _queryClient.ClientCredentials.UserName.UserName = SapUser;
            _queryClient.ClientCredentials.UserName.Password = SapPass;
            _manageClient.ClientCredentials.UserName.UserName = SapUser;
            _manageClient.ClientCredentials.UserName.Password = SapPass;
            _materialService = materialService;
        }

        public SalesOrderDetailsResponse GetSalesOrder(string soExternalId)
        {
            var query = new SalesOrderByElementsQueryMessage_sync();
            var queryByElements = new SalesOrderByElementsQuerySelectionByElements();
            var queryElementsArray = new SalesOrderByElementsQuerySelectionByID[1];
            var queryElements = new SalesOrderByElementsQuerySelectionByID();
            var ID = new SalesOrderQueryService.BusinessTransactionDocumentID();

            ID.Value = soExternalId;
            queryElements.LowerBoundaryID = ID;
            queryElements.InclusionExclusionCode = "I";
            queryElements.IntervalBoundaryTypeCode = "1";

            queryElementsArray[0] = queryElements;
            queryByElements.SelectionByID = queryElementsArray;
            query.SalesOrderSelectionByElements = queryByElements;

            var sapResponse = _queryClient.FindByElements(query);

            return ParseSapQueryResponse(sapResponse);
        } 

        public SalesOrderDetailsResponse ParseSapQueryResponse(SalesOrderByElementsResponseMessage_sync sapResponse)
        {
            SalesOrderDetailsResponse response = new SalesOrderDetailsResponse();
            BaseResponse tempRes = SapLogParser.ParseSapResponseLog(sapResponse.Log);
            response.Errors = tempRes.Errors;
            response.Warnings = tempRes.Warnings;

            if (sapResponse.SalesOrder != null)
            {
                response.SalesOrderExternalId = sapResponse.SalesOrder[0].ID.Value;
            }            

            return response;
        }

        public SalesOrderDetailsResponse SetSalesOrder(SalesOrderDetailsRequest salesOrder)
        {
            //SalesOrderDetailsResponse response = new SalesOrderDetailsResponse();

            var request = new SalesOrderMaintainRequestBundleMessage_sync();
            request.SalesOrder = new SalesOrderMaintainRequest[1];
            var sapSalesOrder = new SalesOrderMaintainRequest();

            if (salesOrder.ExternalId == null)
            {
                sapSalesOrder.actionCode = ActionCode.Item01;
                sapSalesOrder.actionCodeSpecified = true;
            }
            else
            {
                sapSalesOrder.ID = new SalesOrderManageService.BusinessTransactionDocumentID();
                sapSalesOrder.ID.Value = salesOrder.ExternalId;
            }

            sapSalesOrder.itemListCompleteTransmissionIndicator = true;
            sapSalesOrder.itemListCompleteTransmissionIndicatorSpecified = true;
            sapSalesOrder.ReleaseAllItemsToExecution = true;
            sapSalesOrder.ReleaseAllItemsToExecutionSpecified = true;
            sapSalesOrder.ReleaseCustomerRequest = true;
            sapSalesOrder.ReleaseCustomerRequestSpecified = true;

            sapSalesOrder.UltimateDestination = salesOrder.UltDestinationId;
            sapSalesOrder.FreightAccount = salesOrder.FreightAccount;

            sapSalesOrder.SalesAndServiceBusinessArea = new SalesOrderMaintainRequestSalesAndServiceBusinessArea();
            sapSalesOrder.SalesAndServiceBusinessArea.DistributionChannelCode = new SalesOrderManageService.DistributionChannelCode();
            sapSalesOrder.SalesAndServiceBusinessArea.DistributionChannelCode.Value = "01";

            if (!string.IsNullOrEmpty(salesOrder.AccountExternalId))
            {
                sapSalesOrder.AccountParty = new SalesOrderMaintainRequestPartyParty();
                sapSalesOrder.AccountParty.PartyID = new SalesOrderManageService.PartyID();
                sapSalesOrder.AccountParty.PartyID.Value = salesOrder.AccountExternalId;  //set account external id
            }
            if (!string.IsNullOrEmpty(salesOrder.ContactExternalId))
            {
                sapSalesOrder.AccountParty.ContactParty = new SalesOrderMaintainRequestPartyContactParty[1];
                sapSalesOrder.AccountParty.ContactParty[0] = new SalesOrderMaintainRequestPartyContactParty();
                sapSalesOrder.AccountParty.ContactParty[0].PartyID = new SalesOrderManageService.PartyID();
                sapSalesOrder.AccountParty.ContactParty[0].PartyID.Value = salesOrder.ContactExternalId; //contaact external id
            }

            if (!string.IsNullOrEmpty(salesOrder.OrganizationID))
            {
                sapSalesOrder.SalesUnitParty = new SalesOrderMaintainRequestPartyIDParty();
                sapSalesOrder.SalesUnitParty.PartyID = new SalesOrderManageService.PartyID();
                sapSalesOrder.SalesUnitParty.PartyID.Value = salesOrder.OrganizationID; //mapped to external id from middleware request --S5200
            }

            if (salesOrder.Ownership != null && salesOrder.Ownership.LeadOwner != null && !string.IsNullOrEmpty(salesOrder.Ownership.LeadOwner.ExternalId))
            {
                sapSalesOrder.EmployeeResponsibleParty = new SalesOrderMaintainRequestPartyIDParty();
                sapSalesOrder.EmployeeResponsibleParty.PartyID = new SalesOrderManageService.PartyID();
                sapSalesOrder.EmployeeResponsibleParty.PartyID.Value = salesOrder.Ownership.LeadOwner.ExternalId; //owner external id.  example is --7000022
            }

            if (!string.IsNullOrEmpty(salesOrder.CustomerPo))
            {
                sapSalesOrder.BuyerID = new SalesOrderManageService.BusinessTransactionDocumentID();
                sapSalesOrder.BuyerID.Value = salesOrder.CustomerPo;  //set customer po
            }

            sapSalesOrder.DeliveryTerms = new SalesOrderMaintainRequestDeliveryTerms();
            sapSalesOrder.DeliveryTerms.Incoterms = new SalesOrderManageService.Incoterms();
            sapSalesOrder.DeliveryTerms.Incoterms.ClassificationCode = salesOrder.IncotermID; //mapped to external id from middleware request  --DDU
            if(string.IsNullOrEmpty(salesOrder.IncotermLocation))
                sapSalesOrder.DeliveryTerms.Incoterms.TransferLocationName = salesOrder.IncotermLocation;

            sapSalesOrder.CashDiscountTerms = new SalesOrderMaintainRequestCashDiscountTerms();
            sapSalesOrder.CashDiscountTerms.Code = new SalesOrderManageService.CashDiscountTermsCode();
            sapSalesOrder.CashDiscountTerms.Code.Value = salesOrder.PaymentTermID; //mapped to external id from middleware request --1003

            if (!string.IsNullOrEmpty(salesOrder.CurrencyID))
            {
                sapSalesOrder.PricingTerms = new SalesOrderMaintainRequestPricingTerms();
                sapSalesOrder.PricingTerms.CurrencyCode = salesOrder.CurrencyID; //to do, use external id --USD
            }

            sapSalesOrder.PostingDate = new DateTime();
            sapSalesOrder.PostingDate = Convert.ToDateTime(salesOrder.OrderDate);//setting posting date and converting orderdate, probably ok

            if(salesOrder.SOLines != null)
            {
                try
                {
                    sapSalesOrder.Item = SetSalesOrderLines(salesOrder.SOLines, salesOrder.CurrencyID);
                }
                catch (Exception ex)
                {
                    var error = new SalesOrderDetailsResponse();
                    error.Errors = new List<string>();
                    error.Errors.Add(ex.Message);
                    return error;
                }
                //error handling if product specs fail to generate
                if (salesOrder.SOLines.Count != sapSalesOrder.Item.Count())
                {
                    var response = new SalesOrderDetailsResponse();
                    response.Errors = new List<string>();
                    response.Errors.Add("product specs failed to generate");
                    return response;
                }
                    
            }

            request.SalesOrder[0] = sapSalesOrder;
            var sapResponse = _manageClient.MaintainBundle(request);
            var parsedSapResponse = ParseSapManageResponse(sapResponse, salesOrder);         

            if (parsedSapResponse.Errors.Count == 0)
            {
                if (salesOrder.SOLines != null)
                {
                    //make another call to set price now with generated ID
                    request.SalesOrder[0].ID = new SalesOrderManageService.BusinessTransactionDocumentID();
                    request.SalesOrder[0].ID.Value = parsedSapResponse.SalesOrderExternalId;
                    request.SalesOrder[0].actionCodeSpecified = false;
                    parsedSapResponse = ParseSapManageResponse(_manageClient.MaintainBundle(request), salesOrder);
                }
            }

            return parsedSapResponse;
        }

        public SalesOrderMaintainRequestItem[] SetSalesOrderLines(List<SalesOrderLineDetail> lines, string currency)
        {      
            SalesOrderMaintainRequestItem[] items = new SalesOrderMaintainRequestItem[lines.Count()];
            
            //create materials for items that have not been synced
            foreach (var line in lines)
            {
                if (string.IsNullOrEmpty(line.ItemDetails.ExternalId))
                {
                    var res = _materialService.SetMaterial(line.ItemDetails);

                    if (string.IsNullOrEmpty(res.ExternalId))
                    {
                        throw new Exception("Error creating material for " + line.ItemDetails.PartNumber + ": " + res.Errors);
                    }
                    else
                    {
                        line.ItemDetails.ExternalId = res.ExternalId;
                    }
                }
            }

            //get list of unique product ids to generate specs
            var hash = new HashSet<string>();
            foreach (var line in lines)
            {
                if (string.IsNullOrEmpty(line.ProductSpec))
                {
                    //check if there is a matching item in the list that already has a generated product spec
                    foreach(var line2 in lines)
                    {
                        if(line2.ItemDetails.ExternalId == line.ItemDetails.ExternalId && !string.IsNullOrEmpty(line2.ProductSpec))
                        {
                            line.ProductSpec = line2.ProductSpec;
                        }
                    }

                    if (string.IsNullOrEmpty(line.ProductSpec))
                        hash.Add(line.ItemDetails.ExternalId);
                } 
            }

            //generate product specs TODO: throw exception
            var productSpecs = _productSpecService.CreateProductSpec(hash.ToList(), "");
            if (productSpecs.Errors.Count > 0)
                return items;

            foreach(var line in lines)
            {
                SalesOrderMaintainRequestItem item = new SalesOrderMaintainRequestItem();

                int i = lines.IndexOf(line);                
                item.itemScheduleLineListCompleteTransmissionIndicator = true;
                item.itemScheduleLineListCompleteTransmissionIndicatorSpecified = true;

                item.ID = line.LineNum.ToString();                

                item.ItemProduct = new SalesOrderMaintainRequestItemProduct();
                item.ItemProduct.ProductInternalID = new SalesOrderManageService.ProductInternalID();
                item.ItemProduct.ProductInternalID.Value = line.ItemDetails.ExternalId; // set to external id

                item.ItemProduct.ProductRequirementSpecificationKey = new SalesOrderManageService.RequirementSpecificationKey();
                item.ItemProduct.ProductRequirementSpecificationKey.RequirementSpecificationID = new SalesOrderManageService.RequirementSpecificationID();
                if (string.IsNullOrEmpty(line.ProductSpec))
                {
                    line.ProductSpec = GetProductSpecFromList(productSpecs, line.ItemDetails.ExternalId);
                }
                item.ItemProduct.ProductRequirementSpecificationKey.RequirementSpecificationID.Value = line.ProductSpec;


                item.BuyerID = line.CustomerLine.ToString(); //set to customerline, should be okay w/o external

                item.ItemScheduleLine = new SalesOrderMaintainRequestItemScheduleLine[1];
                item.ItemScheduleLine[0] = new SalesOrderMaintainRequestItemScheduleLine();
                item.ItemScheduleLine[0].Quantity = new SalesOrderManageService.Quantity();
                item.ItemScheduleLine[0].Quantity.Value = line.Qty;//

                item.PriceAndTaxCalculationItem = new SalesOrderMaintainRequestPriceAndTaxCalculationItem();
                item.PriceAndTaxCalculationItem.ItemMainPrice = new SalesOrderMaintainRequestPriceAndTaxCalculationItemItemMainPrice();
                item.PriceAndTaxCalculationItem.ItemMainPrice.Rate = new SalesOrderManageService.Rate();
                item.PriceAndTaxCalculationItem.ItemMainPrice.Rate.DecimalValue = line.Price;//

                item.ItemScheduleLine[0].DateTimePeriod = new SalesOrderManageService.UPPEROPEN_LOCALNORMALISED_DateTimePeriod();
                item.ItemScheduleLine[0].DateTimePeriod.StartDateTime = new SalesOrderManageService.LOCALNORMALISED_DateTime1();
                item.ItemScheduleLine[0].DateTimePeriod.StartDateTime.Value = Convert.ToDateTime(line.DueDate).ToUniversalTime();

                //custom fields
                item.EstCost = new SalesOrderManageService.Amount{ Value = line.Cost, currencyCode =  currency};                
                item.DateCode = line.DateCode;
                item.PackagingType = line.PackagingId;
                item.PackagingCondition = line.PackageConditionalId;
                item.CustomerPN = line.CustomerPartNum;
                item.ShipDate = Convert.ToDateTime(line.ShipDate);

                items[i] = item;
            }

            return items;
        }

        public SalesOrderDetailsResponse ParseSapManageResponse(SalesOrderMaintainConfirmationBundleMessage_sync sapResponse, SalesOrderDetailsRequest salesOrder = null)
        {
            SalesOrderDetailsResponse response = new SalesOrderDetailsResponse();
            BaseResponse tempRes = SapLogParser.ParseSapResponseLog(sapResponse.Log);
            response.Errors = tempRes.Errors;
            response.Warnings = tempRes.Warnings;

            if (sapResponse.SalesOrder != null)
            {
                response.SalesOrderExternalId = sapResponse.SalesOrder[0].ID.Value;
            }             
            
            if(salesOrder != null && salesOrder.SOLines.Count > 0)
            {
                response.Lines = new List<ItemProductSpecs>();
                foreach(var line in salesOrder.SOLines)
                {
                    string trimmedProdSpec = line.ProductSpec;
                    trimmedProdSpec = trimmedProdSpec.TrimStart('0');
                    response.Lines.Add(new ItemProductSpecs { LineNum = line.LineNum, ProductSpec = trimmedProdSpec });
                }

                response.Items = new List<PairedIdsResponse>();
                foreach (var line in salesOrder.SOLines)
                {
                    response.Items.Add(new PairedIdsResponse { LocalId = line.ItemDetails.Id, ExternalId = line.ItemDetails.ExternalId });
                }
            }

            return response;
        }

        public SalesOrderDetailsResponse DeleteSalesOrderLine(string salesOrderExternalId, int salesOrderLineNum)
        {
            SalesOrderDetailsResponse response = new SalesOrderDetailsResponse();

            var request = new SalesOrderMaintainRequestBundleMessage_sync();
            request.SalesOrder = new SalesOrderMaintainRequest[1];
            var sapSalesOrder = new SalesOrderMaintainRequest();
            sapSalesOrder.ID = new SalesOrderManageService.BusinessTransactionDocumentID();
            sapSalesOrder.ID.Value = salesOrderExternalId;

            sapSalesOrder.Item = new SalesOrderMaintainRequestItem[1];

            sapSalesOrder.Item[0] = new SalesOrderMaintainRequestItem();
            sapSalesOrder.Item[0].ID = salesOrderLineNum.ToString();
            sapSalesOrder.Item[0].actionCode = ActionCode.Item03;
            sapSalesOrder.Item[0].actionCodeSpecified = true;

            request.SalesOrder[0] = sapSalesOrder;
            var sapResponse = _manageClient.MaintainBundle(request);
            return ParseSapManageResponse(sapResponse);
        }

        public string GetProductSpecFromList(ProductSpecResponse productSpecList, string productId)
        {
            string mappedSpec = "";

            foreach(var spec in productSpecList.ProductSpec)
            {
                if (spec.MaterialId.Equals(productId))
                    mappedSpec = spec.ProductSpecId;
            }

            return mappedSpec;
        }
    }
}
