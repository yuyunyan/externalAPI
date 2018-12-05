using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sourceportal.SAP.Services.PurchaseOrderManageService;
using Sourceportal.SAP.Domain.Models.Responses.PurchaseOrders;
using Sourceportal.SAP.Domain.Models.Requests.PurchaseOrders;
using Sourceportal.SAP.Domain.Models.Shared;
using Sourceportal.SAP.Services.Utils;
using Sourceportal.SAP.Domain.Models.Responses.Shared;
using System.Configuration;
using Sourceportal.SAP.Services.Materials;
using Sourceportal.SAP.Domain.Models.Responses.Accounts;

namespace Sourceportal.SAP.Services.PurchaseOrders
{
    public class PurchaseOrderService : IPurchaseOrderService
    {
        private ManagePurchaseOrderInClient _manageClient;
        private readonly IMaterialService _materialService;
        private static readonly string SapUser = ConfigurationManager.AppSettings["SapUser"];
        private static readonly string SapPass = ConfigurationManager.AppSettings["SapPass"];

        public PurchaseOrderService(IMaterialService materialService)
        {
            _manageClient = new ManagePurchaseOrderInClient("binding_SOAP12");
            _manageClient.ClientCredentials.UserName.UserName = SapUser;
            _manageClient.ClientCredentials.UserName.Password = SapPass;

            _materialService = materialService;
        }

        public PurchaseOrderResponse GetPurchaseOrder(string externalId)
        {
            //ManagePurchaseOrderInReadRequest request = new ManagePurchaseOrderInReadRequest();

            PurchaseOrderByIDQueryMessage_sync query = new PurchaseOrderByIDQueryMessage_sync();

            query.PurchaseOrder = new PurchaseOrderByIDQuery();
            query.PurchaseOrder.ID = new BusinessTransactionDocumentID[1];
            query.PurchaseOrder.ID[0] = new BusinessTransactionDocumentID();
            query.PurchaseOrder.ID[0].Value = externalId;

            //request.PurchaseOrderByIDQuery_sync = query;

            var sapResponse = _manageClient.ManagePurchaseOrderInRead(query);

            PurchaseOrderResponse response = new PurchaseOrderResponse();
            response.ExternalId = sapResponse.PurchaseOrder[0].ID.Value;

            return response;
        }

        public PurchaseOrderResponse SetPurchaseOrder(PurchaseOrderRequest purchaseOrder)
        {
           
            var request = new PurchaseOrderBundleMaintainRequestMessage_sync();
            request.PurchaseOrderMaintainBundle = new PurchaseOrderMaintainRequestBundle[1];
            var poRequest = new PurchaseOrderMaintainRequestBundle();

            poRequest.BusinessTransactionDocumentTypeCode = new BusinessTransactionDocumentTypeCode();
            poRequest.BusinessTransactionDocumentTypeCode.Value = "001";

            poRequest.ItemListCompleteTransmissionIndicator = true;
            poRequest.ItemListCompleteTransmissionIndicatorSpecified = true;

            poRequest.SellerParty = new PurchaseOrderMaintainRequestBundleParty();
            poRequest.SellerParty.ObjectNodePartyTechnicalID = "3";
            poRequest.SellerParty.PartyKey = new PartyKey();
            poRequest.SellerParty.PartyKey.PartyID = new PartyID();
            poRequest.SellerParty.PartyKey.PartyID.Value = purchaseOrder.AccountExternalId;

            poRequest.BillToParty = new PurchaseOrderMaintainRequestBundleParty();
            poRequest.BillToParty.ObjectNodePartyTechnicalID = "5";
            poRequest.BillToParty.PartyKey = new PartyKey();
            poRequest.BillToParty.PartyKey.PartyID = new PartyID();
            poRequest.BillToParty.PartyKey.PartyID.Value = purchaseOrder.OrganizationID;

            poRequest.BuyerParty = new PurchaseOrderMaintainRequestBundleParty();
            poRequest.BuyerParty.ObjectNodePartyTechnicalID = "2";
            poRequest.BuyerParty.PartyKey = new PartyKey();
            poRequest.BuyerParty.PartyKey.PartyID = new PartyID();
            poRequest.BuyerParty.PartyKey.PartyID.Value = purchaseOrder.OrganizationID;

            poRequest.EmployeeResponsibleParty = new PurchaseOrderMaintainRequestBundleParty();
            poRequest.EmployeeResponsibleParty.PartyKey = new PartyKey();
            poRequest.EmployeeResponsibleParty.PartyKey.PartyID = new PartyID();
            poRequest.EmployeeResponsibleParty.PartyKey.PartyID.Value =
                purchaseOrder.Ownership.LeadOwner.ExternalId;

            poRequest.DeliveryTerms = new PurchaseOrderMaintainRequestBundleDeliveryTerms();
            poRequest.DeliveryTerms.IncoTerms = new Incoterms();
            poRequest.DeliveryTerms.IncoTerms.ClassificationCode = purchaseOrder.IncotermID;

            poRequest.CashDiscountTerms = new PurchaseOrderMaintenanceCashDiscountTerms();
            poRequest.CashDiscountTerms.Code = new CashDiscountTermsCode();
            poRequest.CashDiscountTerms.Code.Value = purchaseOrder.PaymentTermID;

            poRequest.CurrencyCode = purchaseOrder.CurrencyID;

            poRequest.Date = new DateTime();
            poRequest.Date = Convert.ToDateTime(purchaseOrder.OrderDate);

            if (purchaseOrder.ExternalId != null)
            {
                poRequest.PurchaseOrderID = new BusinessTransactionDocumentID();
                poRequest.PurchaseOrderID.Value = purchaseOrder.ExternalId;
            }

            //lines
            if (purchaseOrder.Lines != null && purchaseOrder.Lines.Count > 0)
            {
                try
                {
                    poRequest.Item = SetPurchaseOrderLines(purchaseOrder.Lines, purchaseOrder.ToLocationExternalId);
                }
                catch (Exception ex)
                {
                    var error =  new PurchaseOrderResponse();
                    error.Errors.Add(ex.Message);
                    return error;
                }
            }

            request.PurchaseOrderMaintainBundle[0] = poRequest;
            
            var sapResponse = _manageClient.ManagePurchaseOrderInMaintainBundle(request);
            var parsedSapResponse = ParseSapManageResponse(sapResponse, purchaseOrder.Lines);

            return parsedSapResponse;
            
        }

        private PurchaseOrderMaintainRequestBundleItem[] SetPurchaseOrderLines(List<PurchaseOrderLine> lines, string toLocationId)
        {
            PurchaseOrderMaintainRequestBundleItem[] items = new PurchaseOrderMaintainRequestBundleItem[lines.Count];

            foreach(var line in lines)
            {
                PurchaseOrderMaintainRequestBundleItem item = new PurchaseOrderMaintainRequestBundleItem();
                int index = lines.IndexOf(line);

                item.ItemID = line.LineNum.ToString();

                item.Quantity = new Quantity();
                item.Quantity.unitCode = "EA";
                item.Quantity.Value = line.Qty;

                item.BusinessTransactionDocumentItemTypeCode = "18";

                item.ListUnitPrice = new Price();
                item.ListUnitPrice.Amount = new Amount();
                item.ListUnitPrice.Amount.Value = line.Cost;
                item.ListUnitPrice.BaseQuantity = new Quantity();
                item.ListUnitPrice.BaseQuantity.Value = 1;

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

                item.ItemProduct = new PurchaseOrderMaintainRequestBundleItemProduct();
                item.ItemProduct.ObjectNodePartyTechnicalID = "9";
                item.ItemProduct.CashDiscountDeductibleIndicator = true;
                item.ItemProduct.ProductKey = new ProductKey();
                item.ItemProduct.ProductKey.ProductTypeCode = "1";
                item.ItemProduct.ProductKey.ProductIdentifierTypeCode = "1";
                item.ItemProduct.ProductKey.ProductID = new ProductID();
                item.ItemProduct.ProductKey.ProductID.Value = line.ItemDetails.ExternalId;

                item.ShipToLocation = new PurchaseOrderMaintainRequestBundleItemLocation();
                item.ShipToLocation.AddressHostUUID = new UUID{Value = toLocationId };
                
                if (line.IsSpecBuy)
                {
                    item.SpecBuy = line.IsSpecBuy;
                    item.SpecBuyFor = line.SpecBuyForAccount + " (" + line.SpecBuyForUser + ")";
                    item.SpecBuyReason = line.SpecBuyReason;
                    item.SpecBuySpecified = true;
                }

                item.DeliveryPeriod = new UPPEROPEN_LOCALNORMALISED_DateTimePeriod();
                item.DeliveryPeriod.StartDateTime = new LOCALNORMALISED_DateTime();
                item.DeliveryPeriod.StartDateTime.Value = Convert.ToDateTime(line.DueDate).ToUniversalTime();
                item.DeliveryPeriod.EndDateTime = new LOCALNORMALISED_DateTime();
                item.DeliveryPeriod.EndDateTime.Value = Convert.ToDateTime(line.DueDate).ToUniversalTime();

                item.PackagingType = line.PackagingTypeExternalId;

                items[index] = item;
            }

            return items;
        }

        public PurchaseOrderResponse ParseSapManageResponse(PurchaseOrderMaintainConfirmationBundleMessage_sync sapResponse, List<PurchaseOrderLine> lines)
        {
            PurchaseOrderResponse response = new PurchaseOrderResponse();
            BaseResponse tempRes = SapLogParser.ParseSapResponseLog(sapResponse.Log);
            response.Errors = tempRes.Errors;
            response.Warnings = tempRes.Warnings;

            if (sapResponse.PurchaseOrder != null)
            {
                response.ExternalId = sapResponse.PurchaseOrder[0].BusinessTransactionDocumentID.Value;
            }

            response.Items = new List<PairedIdsResponse>();
            foreach(var line in lines)
            {
                response.Items.Add(new PairedIdsResponse { LocalId = line.ItemDetails.Id, ExternalId = line.ItemDetails.ExternalId });
            }

            return response;
        }
    }
}
