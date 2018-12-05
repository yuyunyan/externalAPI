using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sourceportal.SAP.Domain.Models.Responses.ProductSpec;

namespace Sourceportal.SAP.Services.ProductSpec
{
    public interface IProductSpecService
    {
        ProductSpecResponse CreateProductSpec(List<string>productId, string description);
    }
}
