using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sourceportal.SAP.Services.BusinessPartnerManageService;
using Sourceportal.SAP.Services.BusinessPartnerQueryService;
using System.Configuration;
using Sourceportal.SAP.Domain.Models.Responses.Accounts;
using Sourceportal.SAP.Domain.Models.Requests.Accounts;
using Sourceportal.SAP.Domain.Models.Responses.Shared;
using Sourceportal.SAP.Services.Utils;
using PartyLifeCycleStatusCode = Sourceportal.SAP.Services.BusinessPartnerManageService.PartyLifeCycleStatusCode;
using Sourceportal.SAP.Domain.Models.Shared;
using Sourceportal.SAP.Services.SAP;

namespace Sourceportal.SAP.Services.Accounts
{
    public class BusinessPartnerService : IBusinessPartnerService
    {
        private readonly IAccountService _customerService;
        private readonly ISupplierService _supplierService;
        private readonly IAccountHierarchyService _hierarchyService;
        private ManageBusinessPartnerInClient _manageClient;
        private QueryBusinessPartnerInClient _queryClient;
        private string _billingCountry;
        private static readonly string SapUser = ConfigurationManager.AppSettings["SapUser"];
        private static readonly string SapPass = ConfigurationManager.AppSettings["SapPass"];

        public BusinessPartnerService(IAccountService customerService, ISupplierService supplierService, IAccountHierarchyService hierarchyService)
        {
            _customerService = customerService;
            _supplierService = supplierService;
            _hierarchyService = hierarchyService;

            _manageClient = new ManageBusinessPartnerInClient("binding_SOAP12");
            _manageClient.ClientCredentials.UserName.UserName = SapUser;
            _manageClient.ClientCredentials.UserName.Password = SapPass;

            _queryClient = new QueryBusinessPartnerInClient("binding_SOAP12");
            _queryClient.ClientCredentials.UserName.UserName = SapUser;
            _queryClient.ClientCredentials.UserName.Password = SapPass;
        }

        public BusinessPartnerIncomingResponse MapBusinessPartnerResponse(BusinessPartnerResponse bpr)
        {
            //var response = new BusinessPartnerIncomingResponse();
            if (bpr.Errors.Count > 0)
                return new BusinessPartnerIncomingResponse { Errors = bpr.Errors };

            var response = GetBusinessPartnerForIncoming(bpr.BpExternalId);
            
            response.ExternalId = bpr.ExternalId;

            foreach(var contact in response.Contacts)
            {
                contact.ContactId = bpr.ContactExternalIds.Where(x => x.ExternalId == contact.ExternalId).First().LocalId;
            }

            foreach(var location in response.Locations)
            {
                location.LocationId = bpr.LocationExternalIds.Where(x => x.ExternalId == location.ExternalId).First().LocalId;
            }

            //Copying the Parent GroupId
            bpr.Hierarchy.ParentGroupId = response.AccountDetails.Hierarchy.ParentGroupId;
            response.AccountDetails.Hierarchy = bpr.Hierarchy;
            
            return response;
        }

        public BusinessPartnerResponse ProcessBusinessPartnerRequest(BusinessPartnerRequest businessPartner)
        {
            var bpManageResponse = SetBusinessPartner(businessPartner);
            var response = new BusinessPartnerResponse();

            if (bpManageResponse.Errors.Count > 0)
                return bpManageResponse;

            var bpQueryResponse = GetBusinessPartner(bpManageResponse.BpExternalId, businessPartner);

            if (string.IsNullOrEmpty(bpQueryResponse.BpExternalId))
                return bpQueryResponse;


            if (businessPartner.BpDetails.AccountTypes.Contains("supplier", StringComparer.OrdinalIgnoreCase))
            {
                var suppRequest = SetSupplierData(bpQueryResponse.BpExternalId);
                response.SupplierDetails = suppRequest.SupplierDetails;

                if (suppRequest.Errors.Count > 0)
                {
                    return suppRequest;
                }
            }

            if (businessPartner.BpDetails.AccountTypes.Contains("customer", StringComparer.OrdinalIgnoreCase))
            {
                var custRequest = SetCustomerData(bpQueryResponse.BpExternalId, businessPartner);

                if (custRequest.Errors.Count > 0)
                {
                    return custRequest;
                }

                response.Hierarchy = custRequest.Hierarchy;
                response.Hierarchy.Id = businessPartner.BpDetails.Hierarchy.Id;
                response.CustomerDetails = custRequest.CustomerDetails;                
            }
            
            response.BpExternalId = bpQueryResponse.BpExternalId;
            response.ExternalId = bpQueryResponse.BpExternalId;
            response.ContactExternalIds = bpQueryResponse.ContactExternalIds;
            response.LocationExternalIds = bpQueryResponse.LocationExternalIds;
            response.Status = bpQueryResponse.Status;
            response.Errors = bpQueryResponse.Errors;
            response.Warnings = bpQueryResponse.Warnings;
            response.Note = bpQueryResponse.Note;

            return response;
        }

        private BusinessPartnerResponseMessage_sync CreateBusinessPartnerQuery(string externalId)
        {
            var req = new BusinessPartnerByIdentificationQueryMessage_sync();
            req.BusinessPartnerSelectionByIdentification = new BusinessPartnerSelectionByIdentification();
            req.BusinessPartnerSelectionByIdentification.SelectionByInternalID = new SelectionByIdentifier[1];
            req.BusinessPartnerSelectionByIdentification.SelectionByInternalID[0] = new SelectionByIdentifier();
            req.BusinessPartnerSelectionByIdentification.SelectionByInternalID[0].LowerBoundaryIdentifier = externalId;
            req.BusinessPartnerSelectionByIdentification.SelectionByInternalID[0].InclusionExclusionCode = "I";
            req.BusinessPartnerSelectionByIdentification.SelectionByInternalID[0].IntervalBoundaryTypeCode = "1";

            return _queryClient.FindByIdentification(req);
        }

        public BusinessPartnerQueryResponse GetBusinessPartner(string externalId, BusinessPartnerRequest bpRequest)
        {
            var sapResponse = CreateBusinessPartnerQuery(externalId);
            var sapParsedResponse = ParseSapQueryResponse(sapResponse, bpRequest);
            return sapParsedResponse;
        }

        public BusinessPartnerIncomingResponse GetBusinessPartnerForIncoming(string externalId)
        {
            var sapResponse = CreateBusinessPartnerQuery(externalId);
            var sapParsedResponse = ParseSapQueryResponseForIncoming(sapResponse);
            return sapParsedResponse;
        }

        public BusinessPartnerResponse SetBusinessPartner(BusinessPartnerRequest businessPartner)
        {
            var req = new BusinessPartnerBundleMaintainRequestMessage_sync();

            //int bpCount = GetNumberOfBusinessPartners(businessPartner);
            req.BusinessPartner = new BusinessPartnerMaintainRequestBundleBusinessPartner[1];

            var sapBp = SetAccountDetails(businessPartner.BpDetails);
            req.BusinessPartner[0] = sapBp;

            _billingCountry = businessPartner.Locations.First(x => x.LocationTypeExternalId.Contains("BILL_TO")).CountryExternalId;

            //if this is a new business partner, we need to create it with only one location first
            if (string.IsNullOrEmpty(businessPartner.BpDetails.ExternalID))
            {
                //find the default and set the details
                var defaultLocation = businessPartner.Locations.First(x => x.LocationTypeExternalId.Contains("BILL_TO"));
                req.BusinessPartner[0].AddressInformation = SetDefaultLocation(defaultLocation, businessPartner.BpDetails.Email, businessPartner.BpDetails.Website);

                //var xml1 = SapXmlExtractor.GetXmlString(req);
                var defaultLocationResponse = ParseSapManageResponse(_manageClient.MaintainBundle(req));


                //was the call successful?
                if (defaultLocationResponse.Errors.Count > 0)
                {
                    return defaultLocationResponse;
                }
                else
                {
                    //we need to query the new business partner to get the UUID of the default location we just created
                    var bpQueryResponse = GetBusinessPartner(defaultLocationResponse.BpExternalId, businessPartner);

                    if (string.IsNullOrEmpty(bpQueryResponse.BpExternalId))
                        return bpQueryResponse;

                    //set the external ids so that we are now updating the business partner and the location that exists
                    defaultLocation.ExternalId = bpQueryResponse.LocationExternalIds.First().ExternalId;
                    req.BusinessPartner[0].InternalID = bpQueryResponse.BpExternalId;
                }
            }

            var sapLocations = SetLocationDetails(businessPartner.Locations, businessPartner.BpDetails.Email, businessPartner.BpDetails.Website);
            req.BusinessPartner[0].AddressInformation = sapLocations;

            var sapContacts = SetContactDetails(businessPartner.Contacts, businessPartner.BpDetails.Website, businessPartner.BpDetails.ExternalID);
            req.BusinessPartner[0].ContactPerson = sapContacts;

            //var xml2 = SapXmlExtractor.GetXmlString(req);
            var sapResponse = _manageClient.MaintainBundle(req);

            var sapResponseParsed = ParseSapManageResponse(sapResponse);
            return sapResponseParsed;
        }

        public BusinessPartnerMaintainRequestBundleBusinessPartner SetAccountDetails(AccountDetails account)
        {
            var sapBp = new BusinessPartnerMaintainRequestBundleBusinessPartner();

            sapBp.CategoryCode = "2";
            sapBp.contactPersonListCompleteTransmissionIndicator = true;
            sapBp.contactPersonListCompleteTransmissionIndicatorSpecified = true;

            sapBp.CompanyType = account.CompanyType;

            sapBp.Organisation = new BusinessPartnerMaintainRequestBundleOrganisation();
            sapBp.Organisation.FirstLineName = account.Name;

            if (!string.IsNullOrEmpty(account.ExternalID))
                sapBp.InternalID = account.ExternalID;

            sapBp.ApprovedVendor1 = account.VendorNum;
            sapBp.ApprovedVendorSpecified = true;
            sapBp.ApprovedVendor = account.ApprovedVendor;

            return sapBp;
        }

        private BusinessPartnerMaintainRequestBundleContactPerson[] SetContactDetails(List<ContactDetails> contacts, string website, string accountExternalId)
        {
            var sapContacts = new BusinessPartnerMaintainRequestBundleContactPerson[contacts.Count];

            foreach(var contact in contacts)
            {
                int index = contacts.IndexOf(contact);
                var sapContact = new BusinessPartnerMaintainRequestBundleContactPerson();

                sapContact.GivenName = contact.FirstName;
                sapContact.FamilyName = contact.LastName;

                if (!string.IsNullOrEmpty(contact.OfficePhone))
                {
                    sapContact.workplaceTelephoneListCompleteTransmissionIndicator = true;
                    sapContact.workplaceTelephoneListCompleteTransmissionIndicatorSpecified = true;
                    sapContact.WorkplaceTelephone = new BusinessPartnerMaintainRequestBundleWorkplaceTelephone[1];
                    sapContact.WorkplaceTelephone[0] = new BusinessPartnerMaintainRequestBundleWorkplaceTelephone();
                    sapContact.WorkplaceTelephone[0].FormattedNumberDescription = contact.OfficePhone;
                }

                sapContact.WorkplaceWebURI = website;
                sapContact.WorkplaceEmailURI = new BusinessPartnerManageService.EmailURI();
                sapContact.WorkplaceEmailURI.Value = contact.Email;

                if (!string.IsNullOrEmpty(contact.LocationExternalId))
                {
                    sapContact.WorkplaceBusinessAddressUUID = new BusinessPartnerManageService.UUID();
                    sapContact.WorkplaceBusinessAddressUUID.Value = contact.LocationExternalId;
                }

                if (!string.IsNullOrEmpty(contact.ExternalId))
                    sapContact.BusinessPartnerContactInternalID = contact.ExternalId;

                sapContacts[index] = sapContact;
            }

            return sapContacts;
        }

        private BusinessPartnerMaintainRequestBundleAddressInformation[] SetLocationDetails(List<LocationDetails> locations, string email, string website)
        {
            var sapLocations = new BusinessPartnerMaintainRequestBundleAddressInformation[locations.Count];

            foreach(var location in locations)
            {
                int index = locations.IndexOf(location);
                var sapLocation = new BusinessPartnerMaintainRequestBundleAddressInformation();

                if (string.IsNullOrEmpty(location.ExternalId))
                {
                    sapLocation.actionCode = ActionCode.Item01;
                    sapLocation.actionCodeSpecified = true;
                }
                else
                {
                    sapLocation.actionCode = ActionCode.Item02;
                    sapLocation.actionCodeSpecified = true;
                    sapLocation.UUID = new BusinessPartnerManageService.UUID();
                    sapLocation.UUID.Value = location.ExternalId;
                }

                /*
                sapLocation.addressUsageListCompleteTransmissionIndicator = true;
                sapLocation.addressUsageListCompleteTransmissionIndicatorSpecified = true;
                //address usage (type)
                sapLocation.AddressUsage = new BusinessPartnerMaintainRequestBundleAddressUsage[location.LocationTypeExternalId.Count + 1];
                foreach(var type in location.LocationTypeExternalId)
                {
                    int typeIndex = location.LocationTypeExternalId.IndexOf(type);
                    var addressUsage = new BusinessPartnerMaintainRequestBundleAddressUsage();
                    addressUsage.AddressUsageCode = new BusinessPartnerManageService.AddressUsageCode();
                    addressUsage.AddressUsageCode.Value = type;

                    sapLocation.AddressUsage[typeIndex] = addressUsage;

                    if (type.Equals("BILL_TO"))
                    {
                        var defaultUsage = new BusinessPartnerMaintainRequestBundleAddressUsage();
                        defaultUsage.AddressUsageCode = new BusinessPartnerManageService.AddressUsageCode();
                        defaultUsage.AddressUsageCode.Value = "XXDEFAULT";

                        sapLocation.AddressUsage[location.LocationTypeExternalId.Count] = addressUsage;
                    }
                }*/


                sapLocation.Address = new BusinessPartnerMaintainRequestBundleAddress();
                sapLocation.Address.PostalAddress = new BusinessPartnerMaintainRequestBundleAddressPostalAddress();

                sapLocation.Address.PostalAddress.CareOfName = location.Address1;

                sapLocation.Address.PostalAddress.AdditionalStreetPrefixName = location.Address2;

                sapLocation.Address.PostalAddress.StreetSuffixName = location.Address4;

                sapLocation.Address.PostalAddress.HouseID = location.HouseNo;

                sapLocation.Address.PostalAddress.StreetName = location.Street;

                sapLocation.Address.PostalAddress.CityName = location.City;

                sapLocation.Address.PostalAddress.RegionCode = new BusinessPartnerManageService.RegionCode();
                sapLocation.Address.PostalAddress.RegionCode.Value = location.StateExternalId;

                sapLocation.Address.PostalAddress.StreetPostalCode = location.PostalCode;

                sapLocation.Address.PostalAddress.DistrictName = location.District;

                sapLocation.Address.PostalAddress.CountryCode = location.CountryExternalId;

                sapLocation.Address.EmailURI = new BusinessPartnerManageService.EmailURI();
                sapLocation.Address.EmailURI.Value = email;

                sapLocation.Address.WebURI = website;

                sapLocations[index] = sapLocation;
            }

            return sapLocations;
        }

        private BusinessPartnerMaintainRequestBundleAddressInformation[] SetDefaultLocation(LocationDetails location, string email, string website)
        {
            List<LocationDetails> defaultLocation = new List<LocationDetails>();
            defaultLocation.Add(location);

            return SetLocationDetails(defaultLocation, email, website);
        }

        private BusinessPartnerResponse ParseSapManageResponse(BusinessPartnerBundleMaintainConfirmationMessage_sync sapResponse)
        {
            BusinessPartnerResponse response = new BusinessPartnerResponse();
            BaseResponse tempRes = SapLogParser.ParseSapResponseLog(sapResponse.Log);
            response.Errors = tempRes.Errors;
            response.Warnings = tempRes.Warnings;

            if (sapResponse.BusinessPartner != null)
            {
                response.BpExternalId = sapResponse.BusinessPartner[0].InternalID;
            }

            return response;
        }

        private BusinessPartnerQueryResponse ParseSapQueryResponse(BusinessPartnerResponseMessage_sync sapResponse, BusinessPartnerRequest bpRequest)
        {
            BusinessPartnerQueryResponse response = new BusinessPartnerQueryResponse();
            BaseResponse tempRes = SapLogParser.ParseSapResponseLog(sapResponse.Log);
            response.Errors = tempRes.Errors;
            response.Warnings = tempRes.Warnings;

            if (sapResponse.BusinessPartner != null)
            {
                response.BpExternalId = sapResponse.BusinessPartner[0].InternalID;
                response.IsSupplier = sapResponse.BusinessPartner[0].SupplierIndicator;
                response.IsCustomer = sapResponse.BusinessPartner[0].CustomerIndicator;
                response.Status = sapResponse.BusinessPartner[0].LifeCycleStatusCode.GetEnumDescription();
            }
            else
                return response;

            BusinessPartnerResponse conAndLocData = SetLocationAndContactIds(sapResponse, bpRequest);
            response.LocationExternalIds = conAndLocData.LocationExternalIds;
            response.ContactExternalIds = conAndLocData.ContactExternalIds;

            return response;
        }

        

        private BusinessPartnerResponse SetCustomerData(string externalId, BusinessPartnerRequest businessPartner)
        {
            var response = new BusinessPartnerResponse();

            response = _customerService.SetCustomerFromBusinessPartner(businessPartner, _billingCountry, externalId);

            if (response.Errors.Count > 0)
                return response;

            AccountHierarchyResponse hierResponse = new AccountHierarchyResponse();
            if (string.IsNullOrEmpty(businessPartner.BpDetails.Hierarchy.ParentId))
                hierResponse = _hierarchyService.SetAccountHierarchy(businessPartner.BpDetails.Hierarchy, externalId);
            else
                hierResponse = _hierarchyService.SetAccountToGroup(businessPartner.BpDetails.Hierarchy, externalId);

            response.Errors = hierResponse.Errors;
            response.Warnings = hierResponse.Warnings;
            response.Note = hierResponse.Note;
            response.Hierarchy = hierResponse;

            return response;
        }

        private BusinessPartnerResponse SetSupplierData(string externalId)
        {
            return _supplierService.SetSupplierFromBusinessPartner(externalId, _billingCountry);
        }

        private int GetNumberOfBusinessPartners(BusinessPartnerRequest businessPartner)
        {
            int accountCount = 1;
            int contactCount = businessPartner.Contacts.Count;
            int locationCount = businessPartner.Locations.Count;

            return accountCount + contactCount + locationCount;
        }

        public BusinessPartnerResponse SetLocationAndContactIds(BusinessPartnerResponseMessage_sync queryResponse, BusinessPartnerRequest bpRequest)
        {
            var response = new BusinessPartnerResponse();

            if (queryResponse.BusinessPartner[0].AddressInformation != null)
            {
                response.LocationExternalIds = new List<PairedIdsResponse>();

                foreach (var location in bpRequest.Locations)
                {

                    var match = queryResponse.BusinessPartner[0].AddressInformation.FirstOrDefault(x => FindLocationMatch(x, location));

                    if (match != null)
                    {
                        response.LocationExternalIds.Add(new PairedIdsResponse
                        {
                            LocalId = location.LocationId,
                            ExternalId = match.UUID.Value
                        });
                    }
                }
            }

            if (queryResponse.BusinessPartner[0].ContactPerson != null)
            {
                response.ContactExternalIds = new List<PairedIdsResponse>();

                foreach (var contact in bpRequest.Contacts)
                {
                    try
                    {
                        var match = queryResponse.BusinessPartner[0].ContactPerson.First(x => FindContactMatch(x, contact));
                    
                        response.ContactExternalIds.Add(new PairedIdsResponse
                        {
                            LocalId = contact.ContactId,
                            ExternalId = match.BusinessPartnerContactInternalID
                        });
                    }
                    catch (Exception e)
                    {
                        response.Errors.Add(e.Message);
                        return response;
                    }
                }
            }

            return response;
        }

        private bool FindLocationMatch(BusinessPartnerReponseBusinessPartnerAddressInformation address, LocationDetails location)
        {
            return (location.Street.Trim(' ').ToLower() == address.Address.PostalAddress.StreetName.Trim(' ').ToLower() &&
                    location.HouseNo.Trim(' ').ToLower() == address.Address.PostalAddress.HouseID.Trim(' ').ToLower() && 
                    location.City.Trim(' ').ToLower() == address.Address.PostalAddress.CityName.Trim(' ').ToLower());
        }

        private bool FindContactMatch(BusinessPartnerReponseBusinessPartnerContactPerson sapContact, ContactDetails reqContact)
        {
            var reqContactFname = reqContact.FirstName.Trim(' ').ToLower();
            var sapContactFname = sapContact.GivenName.Trim(' ').ToLower();
            var reqContactLname = reqContact.LastName.Trim(' ').ToLower();
            var sapContactLname = sapContact.FamilyName.Trim(' ').ToLower();

            return (reqContactFname == sapContactFname && reqContactLname == sapContactLname);
        }

        private BusinessPartnerIncomingResponse ParseSapQueryResponseForIncoming(BusinessPartnerResponseMessage_sync sapResponse)
        {
            BusinessPartnerIncomingResponse response = SetLocationAndContactIdsForIncoming(sapResponse);

            if (sapResponse.BusinessPartner != null)
            {
                var bp = sapResponse.BusinessPartner[0];
                response.AccountDetails = new AccountDetailsIncoming();
                response.AccountDetails.ExternalID = bp.InternalID;
                response.AccountDetails.Status = bp.LifeCycleStatusCode.GetEnumDescription();
                response.AccountDetails.Name = bp.Organisation != null ? bp.Organisation.FirstLineName : null;

                var defaultLocation = response.Locations.Where(x => x.isDefault == true).FirstOrDefault();
                if (defaultLocation != null)
                {
                    response.AccountDetails.Website = defaultLocation.Website;
                    response.AccountDetails.Email = defaultLocation.Email;
                }

                response.AccountDetails.VendorNum = bp.ApprovedVendor1;
                response.AccountDetails.ApprovedVendor = bp.ApprovedVendor;
                response.AccountDetails.CompanyType = bp.CompanyType;
                response.AccountDetails.AccountTypes = new List<string>();

                if (sapResponse.BusinessPartner[0].CustomerIndicator)
                {
                    var custResponse = _customerService.GetCustomerDetails(bp.InternalID);
                    response.AccountDetails.CustomerDetails = custResponse.CustomerDetails != null ? custResponse.CustomerDetails : new AccountTypeDetails();
                    response.AccountDetails.AccountTypes.Add("customer");

                    //hierarchy
                    var hierarchy = _hierarchyService.IsAccountAssignedToHierarchy(bp.InternalID);
                    if(hierarchy.ParentId != null)
                        response.AccountDetails.Hierarchy = hierarchy;
                }

                if (sapResponse.BusinessPartner[0].SupplierIndicator)
                {
                    var suppResponse = _supplierService.GetSupplierDetails(bp.InternalID);
                    response.AccountDetails.SupplierDetails = suppResponse.SupplierDetails != null ? suppResponse.SupplierDetails : new AccountTypeDetails();
                    response.AccountDetails.AccountTypes.Add("supplier");
                }
            }

            BaseResponse tempRes = SapLogParser.ParseSapResponseLog(sapResponse.Log);
            response.Errors = tempRes.Errors;
            response.Warnings = tempRes.Warnings;

            return response;
        }

        private BusinessPartnerIncomingResponse SetLocationAndContactIdsForIncoming(BusinessPartnerResponseMessage_sync queryResponse)
        {
            var response = new BusinessPartnerIncomingResponse();
            response.Locations = new List<LocationDetails>();
            response.Contacts = new List<ContactDetails>();

            if (queryResponse.BusinessPartner[0] != null) {

                if (queryResponse.BusinessPartner[0].AddressInformation != null)
                {
                    foreach (var addressInformation in queryResponse.BusinessPartner[0].AddressInformation)
                    {
                        var location = new LocationDetails();

                        if (addressInformation.Address != null && addressInformation.Address.PostalAddress != null)
                        {
                            var postalAddress = addressInformation.Address.PostalAddress;

                            location.Address1 = postalAddress.CareOfName;
                            location.Address2 = postalAddress.AdditionalStreetPrefixName;
                            location.Address4 = postalAddress.StreetSuffixName;
                            location.HouseNo = postalAddress.HouseID;
                            location.Street = postalAddress.StreetName;
                            location.City = postalAddress.CityName;
                            location.StateExternalId = postalAddress.RegionCode != null ? postalAddress.RegionCode.Value : null;
                            location.District = postalAddress.DistrictName;
                            location.CountryExternalId = postalAddress.CountryCode;
                            location.Website = addressInformation.Address.WebURI;
                            location.Email = addressInformation.Address.EmailURI != null ? addressInformation.Address.EmailURI.Value : null;
                            location.PostalCode = postalAddress.StreetPostalCode;
                        }

                        if (addressInformation.AddressUsage != null)
                        {
                            location.LocationTypeExternalId = new List<string>();
                            
                            if(queryResponse.BusinessPartner[0].AddressInformation.Count() == 1)
                            {
                                location.isDefault = true;
                                location.LocationTypeExternalId.Add("BILL_TO");
                                location.LocationTypeExternalId.Add("SHIP_TO");
                            }
                            else
                            {
                                foreach (var addressUsage in addressInformation.AddressUsage)
                                {
                                    if (addressUsage.AddressUsageCode.Value == "BILL_TO")
                                    {
                                        if (addressUsage.DefaultIndicator == true)
                                        {
                                            location.LocationTypeExternalId.Add("BILL_TO");
                                        }
                                    }
                                    else
                                    {
                                        if(addressUsage.AddressUsageCode.Value == "XXDEFAULT")
                                        {                                            
                                            location.isDefault = true;
                                        }
                                        if (addressUsage.AddressUsageCode.Value == "SHIP_TO")
                                        {
                                            location.LocationTypeExternalId.Add(addressUsage.AddressUsageCode.Value);
                                        }
                                    }
                                }
                            }
                        }

                        location.ExternalId = addressInformation.UUID != null ? addressInformation.UUID.Value : null;
                        response.Locations.Add(location);
                    }
                }

                if (queryResponse.BusinessPartner[0].ContactPerson != null)
                {
                    foreach (var sapContact in queryResponse.BusinessPartner[0].ContactPerson)
                    {
                        var contact = new ContactDetails();

                        contact.FirstName = sapContact.GivenName;
                        contact.LastName = sapContact.FamilyName;
                        contact.OfficePhone = sapContact.WorkplaceTelephone[0] != null ? sapContact.WorkplaceTelephone[0].FormattedNumberDescription : null;
                        contact.Email = sapContact.WorkplaceEmailURI != null ? sapContact.WorkplaceEmailURI.Value : null;
                        contact.LocationExternalId = sapContact.WorkplaceBusinessAddressUUID != null ? sapContact.WorkplaceBusinessAddressUUID.Value : null;
                        contact.ExternalId = sapContact.BusinessPartnerContactInternalID;

                        response.Contacts.Add(contact);
                    }
                }
            }

            return response;
        }
    }
}
