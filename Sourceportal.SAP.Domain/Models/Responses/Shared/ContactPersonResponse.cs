using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Sourceportal.SAP.Domain.Models.Shared
{
    [DataContract]
    public class ContactPersonResponse
    {
        [DataMember(Name = "uuid")]
        public string UUID { get; set; }

        [DataMember(Name = "contactId")]
        public string ContactId { get; set; }

        [DataMember(Name = "defaultContact")]
        public bool DefaultContact { get; set; }

        [DataMember(Name = "formOfAddressCode")]
        public string FormOfAddressCode { get; set; }

        [DataMember(Name = "givenName")]
        public string GivenName { get; set; }

        [DataMember(Name = "familyName")]
        public string FamilyName { get; set; }

        [DataMember(Name = "genderCode")]
        public string GenderCode { get; set; }

        [DataMember(Name = "birthDate")]
        public string BirthDate { get; set; }

        [DataMember(Name = "nonVerbalCommunicationLanguageCode")]
        public string NonVerbalCommunicationLanguageCode { get; set; }

        [DataMember(Name = "occupationCode")]
        public string OccupationCode { get; set; }

        [DataMember(Name = "maritalStatusCode")]
        public string MaritalStatusCode { get; set; }

        [DataMember(Name = "functionTypeCode")]
        public string FunctionTypeCode { get; set; }

        [DataMember(Name = "functionalAreaCode")]
        public string FunctionalAreaCode { get; set; }

        [DataMember(Name = "currentWorkplaceAddressUuid")]
        public string CurrentWorkplaceAddressUuid { get; set; }

        [DataMember(Name = "workplaceAddressUuid")]
        public string WorkdplaceAddressUuid { get; set; }

        [DataMember(Name = "workplaceEmailUri")]
        public string WorkplaceEmailUri { get; set; }

        [DataMember(Name = "workplaceFacsNumberDescription")]
        public string WorkplaceFacsNumberDescription { get; set; }

        [DataMember(Name = "workplaceTelephoneList")]
        public List<WorkplaceTelephone> WorkplaceTelephoneList { get; set; }

        [DataMember(Name = "workplaceAddressDescription")]
        public string WorkplaceAddressDescription { get; set; }

        [DataMember(Name = "workplaceAddressPostalDescription")]
        public string WorkplaceAddressPostalDescription { get; set; }

    }

    [DataContract]
    public class WorkplaceTelephone
    {
                [DataMember(Name = "workplaceTelephoneDescription")]
                public string WorkplaceTelephoneDescription { get; set; }
        
                [DataMember(Name = "workplaceTelephoneMobile")]
                public bool WorkplaceTelephoneMobile { get; set; }
    }
}
