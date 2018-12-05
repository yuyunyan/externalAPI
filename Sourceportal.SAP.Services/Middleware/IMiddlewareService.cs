using Sourceportal.SAP.Domain.Models.Middleware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sourceportal.SAP.Services.Middleware
{
    public interface IMiddlewareService
    {
        string Sync<T>(MiddlewareSyncRequest<T> rqeuest) where T : MiddlewareSyncBase;
    }
}
