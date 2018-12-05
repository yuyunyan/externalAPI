using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sourceportal.SAP.Domain.Models.WebApi.Requests.Accounts
{
    public class ContactsFilter
    {
        public string FreeTextSearch { get; set; }
        public int AccountId { get; set; }
        public int ContactId { get; set; }
        public int RowOffset { get; set; }
        public int RowLimit { get; set; }
    }
}
