using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sourceportal.SAP.Domain.Models.Shared
{
    public class SapOrganizations
    {
        public List<string> Logistics { get; set; }

        public List<string> Planning { get; set; }

        public List<string> Availability { get; set; }

        public List<string> Sales { get; set; }

        public List<CompanyWarehouses> Companies { get; set; }
    }

    public class CompanyWarehouses
    {
        public string Company { get; set; }

        public List<string> Warehouse { get; set; }

        public string CountryCode { get; set; }
    }
}
