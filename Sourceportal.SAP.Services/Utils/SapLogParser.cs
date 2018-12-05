using Sourceportal.SAP.Domain.Models.Responses.Shared;
using Sourceportal.SAP.Domain.Models.Shared;
using Sourceportal.SAP.Services.AccountCustomerSapService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sourceportal.SAP.Services.Utils
{
    public static class SapLogParser
    {
        public static BaseResponse ParseSapResponseLog(dynamic sapRes)
        {
            BaseResponse response = new BaseResponse();
            response.Errors = new List<string>();
            response.Warnings = new List<string>();

            if (sapRes != null && sapRes.Item != null)
            {
                foreach (var item in sapRes.Item)
                {
                    if (Int32.Parse(item.SeverityCode) == (int)SeverityCodeEnums.Error)
                        response.Errors.Add(item.SeverityCode + " " + item.Note);
                    else
                        response.Warnings.Add(item.SeverityCode + " " + item.Note);
                }
            }

            return response;
        }
    }
}
