using Sourceportal.SAP.Domain.Models.Responses.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Sourceportal.SAP.Domain.Models.WebApi.Responses.Accounts
{
    [DataContract]
    public class ContactListResponse : BaseResponse
    {
        [DataMember(Name = "contacts")]
        public IList<ContactResponse> Contacts { get; set; }

        [DataMember(Name = "totalRowCount")]
        public int TotalRowCount;
    }

    [DataContract]
    public class ContactResponse
    {
        [DataMember(Name = "contactId")]
        public int ContactId { get; set; }

        [DataMember(Name = "accountId")]
        public int AccountId { get; set; }

        [DataMember(Name = "accountName")]
        public string AccountName { get; set; }

        //[DataMember(Name = "accountTypes")]
        //public List<AccountType> AccountTypes { get; set; }

        [DataMember(Name = "accountStatus")]
        public string AccountStatus { get; set; }

        [DataMember(Name = "firstName")]
        public string FirstName { get; set; }

        [DataMember(Name = "lastName")]
        public string LastName { get; set; }

        [DataMember(Name = "phone")]
        public string Phone { get; set; }

        [DataMember(Name = "email")]
        public string Email { get; set; }

        [DataMember(Name = "externalId")]
        public string ExternalId { get; set; }

        [DataMember(Name = "isActive")]
        public bool IsActive { get; set; }

        //[DataMember(Name = "owners")]
        //public List<Owner> Owners { get; set; }

        [DataMember(Name = "title")]
        public string Title { get; set; }
    }
}
