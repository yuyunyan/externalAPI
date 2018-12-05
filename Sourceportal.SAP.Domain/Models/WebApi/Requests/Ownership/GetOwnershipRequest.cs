using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sourceportal.SAP.Domain.Models.WebApi.Requests.Ownership
{
    public class GetOwnershipRequest
    {
        public int ObjectID { get; set; }
        public int ObjectTypeID { get; set; }
    }
}
