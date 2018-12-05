using Sourceportal.SAP.Domain.Models.DB.Organizations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sourceportal.SAP.DB.Organizations
{
    public interface IOrganizationRepository
    {
        List<OrganizationDb> GetAllOrganizations();
        List<CompanyDb> GetAllCompanies();
    }
}
