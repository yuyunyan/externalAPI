using Sourceportal.SAP.Domain.Models.Middleware;
using Sourceportal.SAP.Domain.Models.Responses.Shared;

namespace Sourceportal.SAP.Services.ApiService
{
    public interface IRestClient
    {
        TResult Post<T, TResult>(string path, T objectToPost) where TResult : MiddlewareResponse;
    }
}