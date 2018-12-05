using Sourceportal.SAP.DB.Organizations;
using Sourceportal.SAP.Domain.Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sourceportal.SAP.Services.Utils
{
    public class SapDbHelper : ISapDbHelper
    {
        private IOrganizationRepository _organizationRepository;

        public SapDbHelper(IOrganizationRepository orgRepo)
        {
            _organizationRepository = orgRepo;
        }
        public SapOrganizations SetOrgsFromDb()
        {
            var sapOrgs = new SapOrganizations();
            sapOrgs.Availability = new List<string>();
            sapOrgs.Logistics = new List<string>();
            sapOrgs.Planning = new List<string>();
            sapOrgs.Sales = new List<string>();
            sapOrgs.Companies = new List<CompanyWarehouses>();

            var dbOrgs = _organizationRepository.GetAllOrganizations();
            var dbCompanies = _organizationRepository.GetAllCompanies();

            foreach (var org in dbOrgs)
            {
                if (org.Types.Contains("planning"))
                {
                    sapOrgs.Planning.Add(org.Name);
                }

                if (org.Types.Contains("logistics"))
                {
                    sapOrgs.Logistics.Add(org.Name);
                }

                if (org.Types.Contains("availability"))
                {
                    sapOrgs.Availability.Add(org.Name);
                }

                if (org.Types.Contains("sales"))
                {
                    sapOrgs.Sales.Add(org.Name);
                }
            }

            foreach (var comp in dbCompanies)
            {
                CompanyWarehouses company = new CompanyWarehouses
                {
                    Company = comp.Name,
                    Warehouse = comp.Warehouses.Select(x => x.AsString).ToList(),
                    CountryCode = comp.CountryCode

                };
                sapOrgs.Companies.Add(company);
            }

            return sapOrgs;
        }
    }
}
