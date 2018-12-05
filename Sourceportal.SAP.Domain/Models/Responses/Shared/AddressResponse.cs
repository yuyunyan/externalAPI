using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Sourceportal.SAP.Domain.Models.Shared
{
    [DataContract]
    public class AddressResponse
    {
        [DataMember(Name = "uuid")]
        public string UUID { get; set; }

        [DataMember(Name = "currentAddressSnapshotUUID")]
        public string CurrentAddressSnapshotUUID { get; set; }

        [DataMember(Name = "addressUsageCode")]
        public string AddressUsageCode { get; set; }

        [DataMember(Name = "usageValidityStartDate")]
        public string UsageValidityStartDate { get; set; }

        [DataMember(Name = "usageValidityEndDate")]
        public string UsageValidityEndDate { get; set; }

        [DataMember(Name = "informationValidityStartDate")]
        public string InformationValidityStartDate { get; set; }

        [DataMember(Name = "informationValidityEndDate")]
        public string InformationValidityEndDate { get; set; }

        [DataMember(Name = "correspondenceLanguageCode")]
        public string CorrespondenceLanguageCode { get; set; }

        [DataMember(Name = "emailUri")]
        public string EmailUri { get; set; }

        [DataMember(Name = "facsimileFormattedNumberDescription")]
        public string facsimileFormattedNumberDescription { get; set; }

        [DataMember(Name = "countryCode")]
        public string CountryCode { get; set; }

        [DataMember(Name = "regionCode")]
        public string RegionCode { get; set; }

        [DataMember(Name = "cityName")]
        public string CityName { get; set; }

        [DataMember(Name = "postalCode")]
        public string PostalCode { get; set; }

        [DataMember(Name = "streetName")]
        public string StreetName { get; set; }

        [DataMember(Name = "houseId")]
        public string HouseId { get; set; }

        [DataMember(Name = "taxJurisdictionCode")]
        public string TaxJurisdictionCode { get; set; }

        [DataMember(Name = "timeZoneCode")]
        public string TimeZoneCode { get; set; }

        [DataMember(Name = "telephoneNumber")]
        public string TelephoneNumber { get; set; }

        [DataMember(Name = "webUri")]
        public string WebUri { get; set; }

    }
}
