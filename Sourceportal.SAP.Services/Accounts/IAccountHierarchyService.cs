using Sourceportal.SAP.Domain.Models.Requests.Accounts;
using Sourceportal.SAP.Domain.Models.Responses.Accounts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sourceportal.SAP.Services.Accounts
{
    public interface IAccountHierarchyService
    {
        AccountHierarchyResponse SetAccountHierarchy(AccountHierarchyRequest accountHierarchy, string accountId);
        AccountHierarchyResponse GetAccountHierarchyByName(string hierarchyName);
        //AccountHierarchyResponse GetAccountHierarchyById(string hierarchyId);
        AccountHierarchyResponse IsAccountAssignedToHierarchy(string accountId);
        AccountHierarchyResponse RemoveAccountFromGroup(string accountId);
        AccountHierarchyResponse SetAccountToGroup(AccountHierarchyRequest accountHierarchy, string accountId);
    }
}
