using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Sourceportal.SAP.Domain.Models.Requests.Accounts
{
    [DataContract]
    public class LocationDetails
    {
        [DataMember(Name = "locationId")]
        public int LocationId;

        [DataMember(Name = "name")]
        public string Name;

        [DataMember(Name = "externalId")]
        public string ExternalId;

        [DataMember(Name = "locationTypeExternalIds")]
        public List<string> LocationTypeExternalId;

        [DataMember(Name = "countryExternalId")]
        public string CountryExternalId;

        [DataMember(Name = "address1")]
        public string Address1;

        [DataMember(Name = "address2")]
        public string Address2;

        [DataMember(Name = "houseNo")]
        public string HouseNo;

        [DataMember(Name = "street")]
        public string Street;

        [DataMember(Name = "address4")]
        public string Address4;

        [DataMember(Name = "city")]
        public string City;

        [DataMember(Name = "stateExternalId")]
        public string StateExternalId;

        [DataMember(Name = "postalCode")]
        public string PostalCode;

        [DataMember(Name = "district")]
        public string District;

        [DataMember(Name = "website")]
        public string Website;

        [DataMember(Name = "email")]
        public string Email;

        public bool isDefault;
    }
}
