using Sourceportal.SAP.Domain.Models.Responses.Accounts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sourceportal.SAP.Services.Accounts
{
    public interface ISupplierService
    {
        BusinessPartnerResponse SetSupplierFromBusinessPartner(string externalId, string billingCountry);
        BusinessPartnerResponse GetSupplierDetails(string externalId);
    }
}
