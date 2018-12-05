using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sourceportal.SAP.Services.Utils;
using Sourceportal.SAP.Services.AccountHierarchyManageService;
using Sourceportal.SAP.Services.AccountHierarchyQueryService;
using Sourceportal.SAP.Services.ApiService;
using Sourceportal.SAP.Domain.Models.Responses.Accounts;
using Sourceportal.SAP.Domain.Models.Requests.Accounts;
using System.Xml.Serialization;
using System.IO;
using Sourceportal.SAP.Domain.Models.Responses.Shared;
using System.Configuration;

namespace Sourceportal.SAP.Services.Accounts
{
    public class AccountHierarchyService : IAccountHierarchyService
    {

        private readonly IRestClient _restClient;
        private QueryBusinessPartnerHierarchyClient _queryClient;
        private ManageBusinessPartnerHierarchyInClient _manageClient;
        private static readonly string SapUser = ConfigurationManager.AppSettings["SapUser"];
        private static readonly string SapPass = ConfigurationManager.AppSettings["SapPass"];

        public AccountHierarchyService(IRestClient restClient)
        {
            _restClient = restClient;
            _queryClient = new QueryBusinessPartnerHierarchyClient("binding_SOAP12");
            _manageClient = new ManageBusinessPartnerHierarchyInClient("binding_SOAP12");
            _queryClient.ClientCredentials.UserName.UserName = SapUser;
            _queryClient.ClientCredentials.UserName.Password = SapPass;
            _manageClient.ClientCredentials.UserName.UserName = SapUser;
            _manageClient.ClientCredentials.UserName.Password = SapPass;
        }

        public AccountHierarchyResponse GetAccountHierarchyByName(string hierarchyName)
        {
            AccountHierarchyResponse response = new AccountHierarchyResponse();
            var query = new BusinessPartnerHierarchyByIdentificationQueryMessage_sync();
            var queryMessage = new BusinessPartnerHierarchySelectionByIdentification();
            var queryMessageName = new BusinessPartnerHierarchySelectionByName[1];
            queryMessageName[0] = new BusinessPartnerHierarchySelectionByName();
            queryMessageName[0].LowerBoundaryName = hierarchyName;
            queryMessageName[0].IntervalBoundaryTypeCode = "1";
            queryMessageName[0].InclusionExclusionCode = "I";
            queryMessage.SelectionByName = queryMessageName;

            query.BusinessPartnerHierarchySelectionByIdentification = queryMessage;

            var sapResponse = _queryClient.FindByIdentification(query);
            response.ParentUUID = sapResponse.Hierarchy[0].UUID.Value;

            return response;
        }

        public AccountHierarchyResponse GetAccountHierarchyById(string hierarchyId, string accountId)
        {
            var query = new BusinessPartnerHierarchyByIdentificationQueryMessage_sync();
            var queryMessage = new BusinessPartnerHierarchySelectionByIdentification();
            var queryMessageId = new BusinessPartnerHierarchySelectionByID[1];
            queryMessageId[0] = new BusinessPartnerHierarchySelectionByID();
            var queryHierarchyId = new AccountHierarchyQueryService.BusinessPartnerHierarchyID();
            queryHierarchyId.Value = hierarchyId;
            queryMessageId[0].LowerBoundaryHierarchyID = queryHierarchyId;
            queryMessageId[0].IntervalBoundaryTypeCode = "1";
            queryMessageId[0].InclusionExclusionCode = "I";
            queryMessage.SelectionByID = queryMessageId;

            query.BusinessPartnerHierarchySelectionByIdentification = queryMessage;

            var sapResponse = _queryClient.FindByIdentification(query);
            AccountHierarchyResponse response = SetAccountHierarchyQueryResponse(sapResponse, accountId);
            
            return response;
        }

        public AccountHierarchyResponse SetAccountHierarchyQueryResponse(BusinessPartnerHierarchyResponseMessage_sync sapResponse, string accountId)
        {
            AccountHierarchyResponse response = new AccountHierarchyResponse();
            BaseResponse tempRes = SapLogParser.ParseSapResponseLog(sapResponse.Log);
            response.Errors = tempRes.Errors;
            response.Warnings = tempRes.Warnings;

            if (sapResponse.Hierarchy != null)
            {
                response.ParentId = sapResponse.Hierarchy[0].ID.Value;
                response.ParentName = sapResponse.Hierarchy[0].Name;
                response.ParentUUID = sapResponse.Hierarchy[0].UUID.Value;

                if (sapResponse.Hierarchy[0].Group != null)
                {
                    response.Children = new List<AccountHierarchyChildResponse>();

                    foreach (var group in sapResponse.Hierarchy[0].Group)
                    {
                        List<string> accountList = new List<string>();
                        if (group.BusinessPartner != null)
                        {
                            foreach (var account in group.BusinessPartner)
                            {
                                accountList.Add(account.InternalID);
                            }
                        }

                        bool accountIsAssigned = false;
                        if (accountList.Contains(accountId))
                            accountIsAssigned = true;

                        response.Children.Add(new AccountHierarchyChildResponse
                        {
                            ChildName = group.Name,
                            ChildId = group.ID,
                            AssignedToAccount = accountIsAssigned
                        });
                    }
                }
            }            

            if(sapResponse.ProcessingConditions != null)
            {
                if (sapResponse.ProcessingConditions.ReturnedQueryHitsNumberValue == 0)
                {
                    response.Errors = new List<string>();
                    response.Errors.Add("SAP Server Error.  Number of hits = " + sapResponse.ProcessingConditions.ReturnedQueryHitsNumberValue);
                }
            }

            return response;
        }

        public AccountHierarchyResponse SetAccountHierarchyManageResponse(BusinessPartnerHierarchyMaintainConfirmationMessage_sync sapResponse)
        {
            AccountHierarchyResponse response = new AccountHierarchyResponse();
            BaseResponse tempRes = SapLogParser.ParseSapResponseLog(sapResponse.Log);
            response.Errors = tempRes.Errors;
            response.Warnings = tempRes.Warnings;

            if (sapResponse.BusinessPartnerHierarchy != null)
            {
                response.ParentId = sapResponse.BusinessPartnerHierarchy[0].ID.Value;
                response.ParentUUID = sapResponse.BusinessPartnerHierarchy[0].UUID.Value;

            }

            return response;
        }

        public AccountHierarchyResponse SetAccountHierarchy(AccountHierarchyRequest accountHierarchy, string accountId)
        {
            AccountHierarchyResponse response = new AccountHierarchyResponse();            
            var request = new BusinessPartnerHierarchyMaintainRequestMessage_sync();

            //groups
            var groups = new BusinessPartnerHierarchyMaintainRequestGroup[4];
            for(int i = 0; i < 4; i++)
            {
                groups[i] = new BusinessPartnerHierarchyMaintainRequestGroup();

            }

            //set parent
            groups[0].ID = GenerateHierarchyIds(accountId, 0);
            groups[0].Name = accountHierarchy.ParentName;
            groups[0].actionCode = ActionCode.Item01;
            groups[0].actionCodeSpecified = true;
            groups[0].businessPartnerListCompleteTransmissionIndicator = true;
            groups[0].businessPartnerListCompleteTransmissionIndicatorSpecified = true;

            //set children
            for (int i = 1; i < groups.Length; i++)
            {
                groups[i].ID = GenerateHierarchyIds(accountId, i);
                groups[i].Name = GenerateHierarchyNames(i);
                groups[i].ParentGroupID = groups[0].ID;
                groups[i].actionCode = ActionCode.Item01;
                groups[i].actionCodeSpecified = true;
                groups[i].businessPartnerListCompleteTransmissionIndicator = true;
                groups[i].businessPartnerListCompleteTransmissionIndicatorSpecified = true;

                if (accountHierarchy.AssignTo == i)
                {
                    var assignAccount = new BusinessPartnerHierarchyMaintainRequestGroupBusinessPartner();
                    assignAccount.InternalID = accountId;
                    var bp = new BusinessPartnerHierarchyMaintainRequestGroup();
                    groups[i].BusinessPartner = new BusinessPartnerHierarchyMaintainRequestGroupBusinessPartner[1];
                    groups[i].BusinessPartner[0] = new BusinessPartnerHierarchyMaintainRequestGroupBusinessPartner();
                    groups[i].BusinessPartner[0] = assignAccount;
                    groups[i].BusinessPartner[0].actionCode = ActionCode.Item01;
                    groups[i].BusinessPartner[0].actionCodeSpecified = true;
                }
            }

            //create new hierarchy
            BusinessPartnerHierarchyMaintainRequest hierarchy = new BusinessPartnerHierarchyMaintainRequest();
            AccountHierarchyManageService.BusinessPartnerHierarchyID parentId = new AccountHierarchyManageService.BusinessPartnerHierarchyID();
            parentId.Value = "";
            
            //set hierarchy id, name, and groups;
            hierarchy.ID = parentId;
            hierarchy.Name = accountHierarchy.ParentName;
            hierarchy.actionCode = ActionCode.Item01;
            hierarchy.actionCodeSpecified = true;
            hierarchy.Group = groups;
            hierarchy.groupListCompleteTransmissionIndicator = true;
            hierarchy.groupListCompleteTransmissionIndicatorSpecified = true;

            request.BusinessPartnerHierarchy = new BusinessPartnerHierarchyMaintainRequest[1];
            request.BusinessPartnerHierarchy[0] = new BusinessPartnerHierarchyMaintainRequest();
            request.BusinessPartnerHierarchy[0] = hierarchy;

            BusinessPartnerHierarchyMaintainConfirmationMessage_sync sapResponse = _manageClient.MaintainBundle(request);

            response = SetAccountHierarchyManageResponse(sapResponse);

            if (response.Errors.Count > 0)
                return response;

            var newResponse = GetAccountHierarchyById(response.ParentId, accountId);

            return newResponse;
        }


        public AccountHierarchyResponse IsAccountAssignedToHierarchy(string accountId)
        {
            AccountHierarchyResponse response = new AccountHierarchyResponse();
            
            var query = new BusinessPartnerHierarchyByIdentificationQueryMessage_sync();
            var ident = new BusinessPartnerHierarchySelectionByIdentification();
            var queryMessage = new BusinessPartnerHierarchySelectionByBusinessPartnerInternalID();
            var queryMessageName = new BusinessPartnerHierarchySelectionByBusinessPartnerInternalID[1];
            queryMessageName[0] = new BusinessPartnerHierarchySelectionByBusinessPartnerInternalID();
            queryMessageName[0].LowerBoundaryInternalID = accountId;
            queryMessageName[0].IntervalBoundaryTypeCode = "1";
            queryMessageName[0].InclusionExclusionCode = "I";
            ident.SelectionByBusinessPartnerInternalID = queryMessageName;

            query.BusinessPartnerHierarchySelectionByIdentification = ident;

            var sapResponse = _queryClient.FindByIdentification(query);
            if(sapResponse.ProcessingConditions.ReturnedQueryHitsNumberValue > 0)
            {
                response.Errors = new List<string>();
                response.Errors.Add("Number of hierarchy hits = " + sapResponse.ProcessingConditions.ReturnedQueryHitsNumberValue);
                //return response;
                response.ParentId = sapResponse.Hierarchy[0].ID.Value;
                response.ParentName = sapResponse.Hierarchy[0].Name;
                response.ParentGroupId = sapResponse.Hierarchy[0].Group[0].ID;
                response.Children = new List<AccountHierarchyChildResponse>();
                
                foreach (var group in sapResponse.Hierarchy[0].Group)
                {

                    if (group.BusinessPartner != null)
                    {
                        List<string> accountList = new List<string>();
                        if (group.BusinessPartner != null)
                        {
                            foreach (var account in group.BusinessPartner)
                            {
                                accountList.Add(account.InternalID);
                            }
                        }

                        bool accountIsAssigned = false;
                        if (accountList.Contains(accountId))
                            accountIsAssigned = true;

                        response.Children.Add(new AccountHierarchyChildResponse
                        {
                            ChildName = group.Name,
                            ChildId = group.ID,
                            AssignedToAccount = accountIsAssigned
                        });

                    }
                }
            }

            return response;
        }

        public AccountHierarchyResponse RemoveAccountFromGroup(string accountId)
        {
            AccountHierarchyResponse response = new AccountHierarchyResponse();

            var groupsAssigned = IsAccountAssignedToHierarchy(accountId);
            var childGroup = new AccountHierarchyChildResponse();

            foreach(var child in groupsAssigned.Children)
            {
                if (child.AssignedToAccount)
                    childGroup = child;
            }
            
            var request = new BusinessPartnerHierarchyMaintainRequestMessage_sync();

            //groups
            var groups = new BusinessPartnerHierarchyMaintainRequestGroup[1];
            groups[0] = new BusinessPartnerHierarchyMaintainRequestGroup();
            groups[0].ID = childGroup.ChildId;
            groups[0].Name = childGroup.ChildName;
            //groups[0].ParentGroupID = groupsAssigned.ParentGroupId;
            var assignAccount = new BusinessPartnerHierarchyMaintainRequestGroupBusinessPartner();
            assignAccount.InternalID = accountId;
            var bp = new BusinessPartnerHierarchyMaintainRequestGroup();
            groups[0].BusinessPartner = new BusinessPartnerHierarchyMaintainRequestGroupBusinessPartner[1];
            groups[0].BusinessPartner[0] = new BusinessPartnerHierarchyMaintainRequestGroupBusinessPartner();
            groups[0].BusinessPartner[0] = assignAccount;
            groups[0].BusinessPartner[0].actionCode = ActionCode.Item03;
            groups[0].BusinessPartner[0].actionCodeSpecified = true;          

            //new hierarchy id
            BusinessPartnerHierarchyMaintainRequest hierarchy = new BusinessPartnerHierarchyMaintainRequest();
            AccountHierarchyManageService.BusinessPartnerHierarchyID parentId = new AccountHierarchyManageService.BusinessPartnerHierarchyID();
            parentId.Value = groupsAssigned.ParentId;

            //set hierarchy id, name, and groups;
            hierarchy.ID = parentId;
            hierarchy.Name = groupsAssigned.ParentName;
            hierarchy.Group = groups;
            request.BusinessPartnerHierarchy = new BusinessPartnerHierarchyMaintainRequest[1];
            request.BusinessPartnerHierarchy[0] = new BusinessPartnerHierarchyMaintainRequest();
            request.BusinessPartnerHierarchy[0] = hierarchy;

            var xml = SapXmlExtractor.GetXmlString(request);

            BusinessPartnerHierarchyMaintainConfirmationMessage_sync sapResponse = _manageClient.MaintainBundle(request);

            response = SetAccountHierarchyManageResponse(sapResponse);
            //var newResponse = GetAccountHierarchyById(response.ParentId);

            return response;
        }

        public AccountHierarchyResponse SetAccountToGroup(AccountHierarchyRequest accountHierarchy, string accountId)
        {
            AccountHierarchyResponse response = new AccountHierarchyResponse();

            var groupsAssigned = IsAccountAssignedToHierarchy(accountId);

            if (!string.IsNullOrEmpty(groupsAssigned.ParentId))
            {
                var removeResponse = RemoveAccountFromGroup(accountId);
                if (removeResponse.Errors.Count > 0)
                    return removeResponse;
            }

            var request = new BusinessPartnerHierarchyMaintainRequestMessage_sync();

            //groups
            var groups = new BusinessPartnerHierarchyMaintainRequestGroup[1];
            groups[0] = new BusinessPartnerHierarchyMaintainRequestGroup();
            groups[0].ID = accountHierarchy.childGroupId;
            groups[0].ParentGroupID = accountHierarchy.ParentGroupId;
            var assignAccount = new BusinessPartnerHierarchyMaintainRequestGroupBusinessPartner();
            assignAccount.InternalID = accountId;
            var bp = new BusinessPartnerHierarchyMaintainRequestGroup();
            groups[0].BusinessPartner = new BusinessPartnerHierarchyMaintainRequestGroupBusinessPartner[1];
            groups[0].BusinessPartner[0] = new BusinessPartnerHierarchyMaintainRequestGroupBusinessPartner();
            groups[0].BusinessPartner[0] = assignAccount;
            groups[0].BusinessPartner[0].actionCode = ActionCode.Item01;
            groups[0].BusinessPartner[0].actionCodeSpecified = true;


            //new hierarchy id
            BusinessPartnerHierarchyMaintainRequest hierarchy = new BusinessPartnerHierarchyMaintainRequest();
            AccountHierarchyManageService.BusinessPartnerHierarchyID parentId = new AccountHierarchyManageService.BusinessPartnerHierarchyID();
            parentId.Value = accountHierarchy.ParentId;

            //set hierarchy id, name, and groups;
            hierarchy.ID = parentId;
            hierarchy.Name = accountHierarchy.ParentName;
            hierarchy.Group = groups;
            request.BusinessPartnerHierarchy = new BusinessPartnerHierarchyMaintainRequest[1];
            request.BusinessPartnerHierarchy[0] = new BusinessPartnerHierarchyMaintainRequest();
            request.BusinessPartnerHierarchy[0] = hierarchy;

            string requestXml = SapXmlExtractor.GetXmlString(request);
            BusinessPartnerHierarchyMaintainConfirmationMessage_sync sapResponseCheck = new BusinessPartnerHierarchyMaintainConfirmationMessage_sync();
            try
            {
                sapResponseCheck = _manageClient.CheckMaintainBundle(request);
            }
            catch (Exception ex)
            {

            }

            if (sapResponseCheck.Log != null)
            {
                if (sapResponseCheck.Log.MaximumLogItemSeverityCode.Equals("3"))
                    return SetAccountHierarchyManageResponse(sapResponseCheck);
            }

            BusinessPartnerHierarchyMaintainConfirmationMessage_sync sapResponse = _manageClient.MaintainBundle(request);

            response = SetAccountHierarchyManageResponse(sapResponse);
            var newResponse = GetAccountHierarchyById(response.ParentId, accountId);

            return newResponse;
        }

        private string GenerateHierarchyIds(string accountId, int groupNumber)
        {
            if(groupNumber == 0)
            {
                return "SP" + accountId;
            }

            //if (account.Children != null && account.Children[groupNumber-1] != null && account.Children[groupNumber-1].ChildId != null){
            //    return account.Children[groupNumber-1].ChildId;
            //}

            if (groupNumber == 1)
            {
                return "SP" + accountId + "A";
            }
            else if (groupNumber == 2)
            {
                return "SP" + accountId + "B";
            }
            else if (groupNumber == 3)
            {
                return "SP" + accountId + "C";
            }
            else
            {
                return "SP" + accountId;
            }
        }

        private string GenerateHierarchyNames(int groupNumber)
        {
            if (groupNumber == 1)
            {
                return "Americas";
            }
            else if (groupNumber == 2)
            {
                return "EMEA";
            }
            else if (groupNumber == 3)
            {
                return "APAC";
            }
            else
            {
                return "";
            }
        }

    }


}
