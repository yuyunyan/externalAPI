using Sourceportal.SAP.DB.Organizations;
using Sourceportal.SAP.Domain.Models.Requests.Materials;
using Sourceportal.SAP.Domain.Models.Responses.Materials;
using Sourceportal.SAP.Domain.Models.Responses.Shared;
using Sourceportal.SAP.Domain.Models.Shared;
using Sourceportal.SAP.Services.CustomConfigs;
using Sourceportal.SAP.Services.MaterialManageService;
using Sourceportal.SAP.Services.MaterialQueryService;
using Sourceportal.SAP.Services.MaterialValuationDataService;
using Sourceportal.SAP.Services.SAP;
using Sourceportal.SAP.Services.Utils;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sourceportal.SAP.Services.Materials
{
    public class MaterialService : IMaterialService
    {
        private ManageMaterialInClient _manageClient;
        private ManageMaterialValuationDataInClient _materialValuationClient;
        private QueryMaterialInClient _queryClient;
        private SapOrganizations _sapOrgs;
        private ISapDbHelper _sapService;
        private static readonly string SapUser = ConfigurationManager.AppSettings["SapUser"];
        private static readonly string SapPass = ConfigurationManager.AppSettings["SapPass"];

        public MaterialService(ISapDbHelper sapService)
        {
            _manageClient = new ManageMaterialInClient("binding_SOAP12");
            _manageClient.ClientCredentials.UserName.UserName = SapUser;
            _manageClient.ClientCredentials.UserName.Password = SapPass;

            _materialValuationClient = new ManageMaterialValuationDataInClient("binding_SOAP12");
            _materialValuationClient.ClientCredentials.UserName.UserName = SapUser;
            _materialValuationClient.ClientCredentials.UserName.Password = SapPass;

            _queryClient = new QueryMaterialInClient("binding_SOAP12");
            _queryClient.ClientCredentials.UserName.UserName = SapUser;
            _queryClient.ClientCredentials.UserName.Password = SapPass;

            _sapService = sapService;
        }

        

        public MaterialResponse QueryMaterial(string externalId)
        {
            var query = new MaterialByElementsQueryMessage_sync();
            query.MaterialSelectionByElements = new MaterialByElementsQuerySelectionByElements();
            query.MaterialSelectionByElements.SelectionBySearchText = externalId;
            var res = ParseQueryResponse(_queryClient.FindByElements(query));
            return res;
        }

        private MaterialResponse ParseQueryResponse(MaterialByElementsResponseMessage_sync sapMessage)
        {
            var res = new MaterialResponse();

            BaseResponse tempRes = SapLogParser.ParseSapResponseLog(sapMessage.Log);
            res.Errors = tempRes.Errors;
            res.Warnings = tempRes.Warnings;

            if (sapMessage.Material != null)
            {
                res.ExternalId = sapMessage.Material[0].InternalID.Value;
            }

            return res;
        }

        public MaterialResponse SetMaterial(MaterialRequest material)
        {
            var checkExisting = QueryMaterial(material.PartNumber);
            if (!string.IsNullOrEmpty(checkExisting.ExternalId))
                return checkExisting;

            var request = new MaterialMaintainRequestBundleMessage_sync_V1();
            request.Material = new MaterialMaintainRequestBundleMaterial_sync_V1[1];
            var sapMaterial = new MaterialMaintainRequestBundleMaterial_sync_V1();

            sapMaterial.descriptionListCompleteTransmissionIndicator = true;
            sapMaterial.descriptionListCompleteTransmissionIndicatorSpecified = true;
            sapMaterial.detailListCompleteTransmissionIndicator = true;
            sapMaterial.detailListCompleteTransmissionIndicatorSpecified = true;

            sapMaterial.ProductCategoryID = material.CommodityExternalID;
            sapMaterial.BaseMeasureUnitCode = "EA";

            sapMaterial.IdentifiedStockTypeCode = new MaterialManageService.IdentifiedStockTypeCode();
            sapMaterial.IdentifiedStockTypeCode.Value = "04";

            sapMaterial.Description = new MaterialMaintainRequestBundleDescription[1];
            sapMaterial.Description[0] = new MaterialMaintainRequestBundleDescription();
            sapMaterial.Description[0].Description = new MaterialManageService.SHORT_Description();
            var truncatedString = material.PartNumber + " {" + material.Mfg;
            truncatedString = truncatedString.Substring(0, (truncatedString.Length > 39 ? 39 : truncatedString.Length));
            sapMaterial.Description[0].Description.Value = truncatedString + "}";

            if (!string.IsNullOrEmpty(material.Description))
            {
                sapMaterial.Detail = new MaterialBundleMaintainRequestText[1];
                sapMaterial.Detail[0] = new MaterialBundleMaintainRequestText();
                sapMaterial.Detail[0].ContentText = new MaterialManageService.Text();
                sapMaterial.Detail[0].ContentText.Value = material.Description;
            }

            sapMaterial.Purchasing = new MaterialMaintainRequestBundlePurchasing();
            sapMaterial.Purchasing.LifeCycleStatusCodeSpecified = true;
            sapMaterial.Purchasing.LifeCycleStatusCode = MaterialManageService.ProductProcessUsabilityLifeCycleStatusCode.Item2;
            sapMaterial.Purchasing.PurchasingMeasureUnitCode = "EA";
            
            _sapOrgs = _sapService.SetOrgsFromDb();
            sapMaterial.Planning = SetPlanningData(_sapOrgs.Planning);
            sapMaterial.AvailabilityConfirmation = SetAvailabilityData(_sapOrgs.Availability);
            sapMaterial.Sales = SetSalesData(_sapOrgs.Sales);
            sapMaterial.Logistics = SetLogisticsData(_sapOrgs.Logistics);

            sapMaterial.MFGPartNumber = material.PartNumber;
            sapMaterial.Manufacturer = material.Mfg;
            sapMaterial.ECCN = material.ECCN;
            sapMaterial.StrippedPartNumber = material.PartNumberStrip;
            sapMaterial.DatasheetURL = material.DatasheetURL;
            sapMaterial.IHSID = material.SourceDataID.ToString();
            sapMaterial.MSL = material.MSL;
            sapMaterial.EURoHSSpecified = true;
            sapMaterial.EURoHS = material.EURoHS;
            sapMaterial.HTSCode = material.HTS;            

            if (!string.IsNullOrEmpty(material.ExternalId))
            {
                sapMaterial.InternalID = new MaterialManageService.ProductInternalID();
                sapMaterial.InternalID.Value = material.ExternalId;
            }

            request.Material[0] = sapMaterial;
            var xmlParser = SapXmlExtractor.GetXmlString(request);
            var setResponse = ParseManageResponse(_manageClient.MaintainBundle_V1(request));

            if (string.IsNullOrEmpty(setResponse.ExternalId))
            {
                return setResponse;
            }

            return SetMaterialValuationData(setResponse.ExternalId);
        }

        private MaterialMaintainRequestBundlePlanning SetPlanningData(List<string> orgs)
        {
            var planning = new MaterialMaintainRequestBundlePlanning();
            planning.SupplyPlanning = new MaterialMaintainRequestBundleSupplyPlanning[orgs.Count];

            foreach(var org in orgs)
            {
                int index = orgs.IndexOf(org);
                var supplyPlanning = new MaterialMaintainRequestBundleSupplyPlanning();

                supplyPlanning.SupplyPlanningAreaID = new MaterialManageService.SupplyPlanningAreaID();
                supplyPlanning.SupplyPlanningAreaID.Value = org;
                supplyPlanning.LifeCycleStatusCodeSpecified = true;
                supplyPlanning.LifeCycleStatusCode = MaterialManageService.ProductProcessUsabilityLifeCycleStatusCode.Item2;
                supplyPlanning.ProcurementTypeCode = "2";
                supplyPlanning.PlanningProcedureCode = "1";
                supplyPlanning.LotSizeProcedureCode = "1";

                planning.SupplyPlanning[index] = supplyPlanning;
            }

            return planning;
        }

        private MaterialMaintainRequestBundleAvailabilityConfirmation[] SetAvailabilityData(List<string> orgs)
        {
            var availability = new MaterialMaintainRequestBundleAvailabilityConfirmation[orgs.Count];

            foreach (var org in orgs)
            {
                int index = orgs.IndexOf(org);
                var availabilityPlanning = new MaterialMaintainRequestBundleAvailabilityConfirmation();

                availabilityPlanning.PlanningAreaID = new MaterialManageService.SupplyPlanningAreaID();
                availabilityPlanning.PlanningAreaID.Value = org;
                availabilityPlanning.LifeCycleStatusCodeSpecified = true;
                availabilityPlanning.LifeCycleStatusCode = MaterialManageService.ProductProcessUsabilityLifeCycleStatusCode.Item2;
                availabilityPlanning.AvailabilityCheckScopeCode = new MaterialManageService.AvailabilityConfirmationModeCode();
                availabilityPlanning.AvailabilityCheckScopeCode.Value = "A12";

                availability[index] = availabilityPlanning;
            }

            return availability;
        }

        private MaterialMaintainRequestBundleSales[] SetSalesData(List<string> orgs)
        {
            var salesArray = new MaterialMaintainRequestBundleSales[orgs.Count];

            foreach (var org in orgs)
            {
                int index = orgs.IndexOf(org);
                var sales = new MaterialMaintainRequestBundleSales();

                sales.SalesOrganisationID = org;
                sales.LifeCycleStatusCodeSpecified = true;
                sales.LifeCycleStatusCode = MaterialManageService.ProductProcessUsabilityLifeCycleStatusCode.Item2;
                sales.DistributionChannelCode = new MaterialManageService.DistributionChannelCode();
                sales.DistributionChannelCode.Value = "01";
                sales.SalesMeasureUnitCode = "EA";
                sales.ItemGroupCode = "NORM";

                salesArray[index] = sales;
            }

            return salesArray;
        }

        private MaterialMaintainRequestBundleLogistics[] SetLogisticsData(List<string> orgs)
        {
            var logisticsArray = new MaterialMaintainRequestBundleLogistics[orgs.Count];

            foreach (var org in orgs)
            {
                int index = orgs.IndexOf(org);
                var logistics = new MaterialMaintainRequestBundleLogistics();

                logistics.SiteID = new MaterialManageService.LocationID();
                logistics.SiteID.Value = org;
                logistics.LifeCycleStatusCodeSpecified = true;
                logistics.LifeCycleStatusCode = MaterialManageService.ProductProcessUsabilityLifeCycleStatusCode.Item2;

                logisticsArray[index] = logistics;
            }

            return logisticsArray;
        }

        private MaterialResponse SetMaterialValuationData(string externalId)
        {
            var materialValuationMessage = new MaterialValuationDataMaintainRequestBundleMessage_sync();
            materialValuationMessage.MaterialValuationData = new MaterialValuationDataMaintainRequestBundle[_sapOrgs.Companies.Count];

            foreach(var org in _sapOrgs.Companies)
            {
                int index = _sapOrgs.Companies.IndexOf(org);
                var materialValuationData = new MaterialValuationDataMaintainRequestBundle();

                materialValuationData.accountDeterminationSpecificationListCompleteTransmissionIndicatorSpecified = true;
                materialValuationData.accountDeterminationSpecificationListCompleteTransmissionIndicator = true;

                materialValuationData.MaterialInternalID = new MaterialValuationDataService.ProductInternalID();
                materialValuationData.MaterialInternalID.Value = externalId;

                materialValuationData.CompanyID = org.Company;

                var warehouses = org.Warehouse;
                materialValuationData.ValuationPrice = SetValuationPrices(warehouses);
                materialValuationData.AccountDeterminationSpecification = SetAccountDeterminationSpecifications(warehouses);
                materialValuationData.InventoryValuationSpecification = SetInventoryValuationSpecifications(warehouses);
                materialValuationData.MaterialFinancialProcessInfo = SetMaterialFinancialProcessInfo(warehouses);

                materialValuationMessage.MaterialValuationData[index] = materialValuationData;
            }

            var sapRes = ParseMaterialValuationResponse(_materialValuationClient.MaintainBundle(materialValuationMessage));
            
            return sapRes;
        }

        private MaterialValuationDataMaintainRequestBundleValuationPrice[] SetValuationPrices(List<string> warehouses)
        {
            var valuationArray = new MaterialValuationDataMaintainRequestBundleValuationPrice[warehouses.Count];

            foreach (var warehouse in warehouses)
            {
                int index = warehouses.IndexOf(warehouse);
                var valuation = new MaterialValuationDataMaintainRequestBundleValuationPrice();

                valuation.actionCode = MaterialValuationDataService.ActionCode.Item04;
                valuation.actionCodeSpecified = true;

                valuation.PermanentEstablishmentID = warehouse;

                valuation.ValidityDatePeriod = new MaterialValuationDataService.CLOSED_DatePeriod();
                valuation.ValidityDatePeriod.StartDate = Convert.ToDateTime("2018-01-01");
                valuation.ValidityDatePeriod.EndDate = Convert.ToDateTime("9999-12-31");

                valuation.PriceTypeCode = new PriceTypeCode();
                valuation.PriceTypeCode.Value = "1";

                valuation.SetOfBooksID = new SetOfBooksID();
                valuation.SetOfBooksID.Value = "3000";

                valuation.LocalCurrencyValuationPrice = new Price();
                valuation.LocalCurrencyValuationPrice.Amount = new MaterialValuationDataService.Amount();
                valuation.LocalCurrencyValuationPrice.Amount.currencyCode = "USD";
                valuation.LocalCurrencyValuationPrice.Amount.Value = 0;
                valuation.LocalCurrencyValuationPrice.BaseQuantity = new MaterialValuationDataService.Quantity();
                valuation.LocalCurrencyValuationPrice.BaseQuantity.unitCode = "EA";
                valuation.LocalCurrencyValuationPrice.BaseQuantity.Value = 1;

                valuation.ProductValuationLevelTypeCode = new ProductValuationLevelTypeCode();
                valuation.ProductValuationLevelTypeCode.Value = "2";

                valuationArray[index] = valuation;
            }

            return valuationArray;
        }

        private MaterialValuationDataMaintainRequestBundleAccountDeterminationSpecification[] SetAccountDeterminationSpecifications(List<string> warehouses)
        {
            var adsArray = new MaterialValuationDataMaintainRequestBundleAccountDeterminationSpecification[warehouses.Count];

            foreach (var warehouse in warehouses)
            {
                int index = warehouses.IndexOf(warehouse);
                var ads = new MaterialValuationDataMaintainRequestBundleAccountDeterminationSpecification();

                ads.actionCode = MaterialValuationDataService.ActionCode.Item04;
                ads.actionCodeSpecified = true;

                ads.PermanentEstablishmentID = warehouse;
                ads.ProductValuationLevelTypeCode = new ProductValuationLevelTypeCode();
                ads.ProductValuationLevelTypeCode.Value = "2";
                ads.AccountDeterminationMaterialValuationDataGroupCode = new AccountDeterminationMaterialValuationDataGroupCode();
                ads.AccountDeterminationMaterialValuationDataGroupCode.Value = "3050";

                adsArray[index] = ads;
            }

            return adsArray;
        }

        private MaterialValuationDataMaintainRequestBundleInventoryValuationSpecification[] SetInventoryValuationSpecifications(List<string> warehouses)
        {
            var ivsArray = new MaterialValuationDataMaintainRequestBundleInventoryValuationSpecification[warehouses.Count];

            foreach (var warehouse in warehouses)
            {
                int index = warehouses.IndexOf(warehouse);
                var ivs = new MaterialValuationDataMaintainRequestBundleInventoryValuationSpecification();

                ivs.actionCode = MaterialValuationDataService.ActionCode.Item04;
                ivs.actionCodeSpecified = true;

                ivs.PermanentEstablishmentID = warehouse;
                ivs.PerpetualInventoryValuationProcedureCode = "2";
                ivs.ProductValuationLevelTypeCode = new ProductValuationLevelTypeCode();
                ivs.ProductValuationLevelTypeCode.Value = "2";

                ivsArray[index] = ivs;
            }

            return ivsArray;
        }

        private MaterialValuationDataMaintainRequestBundleMaterialFinancialProcessInfo[] SetMaterialFinancialProcessInfo(List<string> warehouses)
        {
            var mfpArray = new MaterialValuationDataMaintainRequestBundleMaterialFinancialProcessInfo[warehouses.Count];

            foreach (var warehouse in warehouses)
            {
                int index = warehouses.IndexOf(warehouse);
                var mfp = new MaterialValuationDataMaintainRequestBundleMaterialFinancialProcessInfo();

                mfp.actionCode = MaterialValuationDataService.ActionCode.Item04;
                mfp.actionCodeSpecified = true;
                mfp.PermanentEstablishmentID = warehouse;

                mfpArray[index] = mfp;
            }

            return mfpArray;
        }


        private MaterialResponse ParseManageResponse(MaterialMaintainConfirmationBundleMessage_sync_V1 sapMessage)
        {
            var res = new MaterialResponse();

            BaseResponse tempRes = SapLogParser.ParseSapResponseLog(sapMessage.Log);
            res.Errors = tempRes.Errors;
            res.Warnings = tempRes.Warnings;

            if (sapMessage.Material != null)
            {
                res.ExternalId = sapMessage.Material[0].InternalID.Value;
            }

            return res;
        }

        private MaterialResponse ParseMaterialValuationResponse(MaterialValuationDataMaintainConfirmationBundleMessage_sync sapMessage)
        {
            var res = new MaterialResponse();

            BaseResponse tempRes = SapLogParser.ParseSapResponseLog(sapMessage.Log);
            res.Errors = tempRes.Errors;
            res.Warnings = tempRes.Warnings;

            if (sapMessage.MaterialValuationData != null)
            {
                res.ExternalId = sapMessage.MaterialValuationData[0].MaterialInternalID.Value;
            }

            return res;
        }
    }
}
