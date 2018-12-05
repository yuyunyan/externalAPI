using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sourceportal.SAP.Services.ProductSpecManageService;
using Sourceportal.SAP.Services.ProductSpecQueryService;
using Sourceportal.SAP.Services.ApiService;
using Sourceportal.SAP.Domain.Models.Responses.ProductSpec;
using Sourceportal.SAP.Domain.Models.Shared;
using Sourceportal.SAP.Services.Utils;
using Sourceportal.SAP.Domain.Models.Responses.Shared;
using System.Configuration;

namespace Sourceportal.SAP.Services.ProductSpec
{
    public class ProductSpecService : IProductSpecService
    {
        private readonly IRestClient _restClient;
        private ManagePRSInClient _manageClient;
        private QueryProductRequirementSpecificationInClient _queryClient;
        private static readonly string SapUser = ConfigurationManager.AppSettings["SapUser"];
        private static readonly string SapPass = ConfigurationManager.AppSettings["SapPass"];

        public ProductSpecService(IRestClient restClient)
        {
            _restClient = restClient;
            _queryClient = new QueryProductRequirementSpecificationInClient("binding_SOAP12");
            _manageClient = new ManagePRSInClient("binding_SOAP12");
            _queryClient.ClientCredentials.UserName.UserName = SapUser;
            _queryClient.ClientCredentials.UserName.Password = SapPass;
            _manageClient.ClientCredentials.UserName.UserName = SapUser;
            _manageClient.ClientCredentials.UserName.Password = SapPass;
        }

        public ProductSpecResponse CreateProductSpec(List<string> productId, string description)
        {
            PRSMaintainRequestBundleMessage_sync request = new PRSMaintainRequestBundleMessage_sync();
            request.ProductRequirementSpecification = new PRSMaintainRequestBundle[productId.Count];

            foreach (var product in productId)
            {

                PRSMaintainRequestBundle productSpec = new PRSMaintainRequestBundle();
                productSpec.RequirementObjectListCompleteTransmissionIndicator = true;
                productSpec.RequirementObjectListCompleteTransmissionIndicatorSpecified = true;
                productSpec.DescriptionListCompleteTransmissionIndicator = true;
                productSpec.DescriptionListCompleteTransmissionIndicatorSpecified = true;

                PRSDescription productSpecDesc = new PRSDescription();
                productSpecDesc.Description = new ProductSpecManageService.MEDIUM_Description();
                productSpecDesc.Description.Value = product; //description descided later
                productSpec.Description = new PRSDescription[1];
                productSpec.Description[0] = productSpecDesc;

                PRSRequirementObject requirementObj = new PRSRequirementObject();
                requirementObj.RequirementObjectMaterial = new PRSRequirementObjectMaterial();
                requirementObj.RequirementObjectMaterial.RequirementObjectMaterialKey = new PRSRequirementOjbectMaterialKey();
                requirementObj.RequirementObjectMaterial.RequirementObjectMaterialKey.ProductIdentifierTypeCode = "1";
                requirementObj.RequirementObjectMaterial.RequirementObjectMaterialKey.ProductID = new ProductSpecManageService.ProductID();
                requirementObj.RequirementObjectMaterial.RequirementObjectMaterialKey.ProductID.Value = product;
                productSpec.RequirementObject = new PRSRequirementObject[1];
                productSpec.RequirementObject[0] = requirementObj;

                request.ProductRequirementSpecification[productId.IndexOf(product)] = productSpec;

            }
            var response = _manageClient.MaintainBundle(request);

            var parsedResponse = ParseProductSpecResponse(response);
            if(parsedResponse.Errors.Count > 0)
            {
                return ParseProductSpecResponse(response);
            }

            try
            {
                MapMaterialIdsToResponse(parsedResponse);
            }
            catch (Exception e)
            {
                parsedResponse.Errors = new List<string>();
                parsedResponse.Errors.Add(e.Message);
            }

            return parsedResponse;                
        }

        public ProductSpecResponse ParseProductSpecResponse(PRSMaintainConfirmationBundleMessage_sync response)
        {
            ProductSpecResponse parsedResponse = new ProductSpecResponse();
            BaseResponse tempRes = SapLogParser.ParseSapResponseLog(response.Log);
            parsedResponse.Errors = tempRes.Errors;
            parsedResponse.Warnings = tempRes.Warnings;
            parsedResponse.ProductSpec = new List<ProductSpecObject>();

            if (response.ProductRequirementSpecification != null)
            {
                foreach (var spec in response.ProductRequirementSpecification)
                {
                    parsedResponse.ProductSpec.Add(new ProductSpecObject{ProductSpecId = spec.ID.Value, VersionUUID = spec.VersionUUID.Value});
                }
            }

            return parsedResponse;
        }

        public void MapMaterialIdsToResponse(ProductSpecResponse response)
        {
            var request = new ProductRequirementSpecificationByVersionUUIDQueryMessage_sync();
            request.ProductRequirementSpecificationByVersionUUID = new ProductSpecManageService.UUID[response.ProductSpec.Count];

            foreach(var spec in response.ProductSpec)
            {
                int index = response.ProductSpec.IndexOf(spec);
                request.ProductRequirementSpecificationByVersionUUID[index] = new ProductSpecManageService.UUID();
                request.ProductRequirementSpecificationByVersionUUID[index].Value = spec.VersionUUID;
            }

            var productSpecList = _manageClient.ReadPRS(request);

            //TODO check if read call failed

            foreach(var spec in productSpecList.ProductRequirementSpecification)
            {
                foreach(var res in response.ProductSpec)
                {
                    if (Int32.Parse(spec.ID.Value) == Int32.Parse(res.ProductSpecId))
                    {
                        res.MaterialId = spec.RequirementObject[0].RequirementObjectMaterial.RequirementObjectMaterialKey.ProductID.Value;
                    }
                }
            }

        }
    }
}
