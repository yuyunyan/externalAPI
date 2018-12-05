using System;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using Sourceportal.SAP.Domain.Models.Responses.Shared;
using Sourceportal.SAP.Domain.Models.Middleware;

namespace Sourceportal.SAP.Services.ApiService
{
    public class RestClient : IRestClient
    {
        private static readonly string MiddlewareUrl = ConfigurationManager.AppSettings["MiddlewareApiUrl"];

        public TResult Post<TRequestType, TResult>(string path, TRequestType objectToPost) where TResult : MiddlewareResponse
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(MiddlewareUrl);

            // Add an Accept header for JSON format.
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            //var authorizationHeaderKey = "Authorization";
            //var authorizationHeaderValue = HttpContext.Current.Request.Headers[authorizationHeaderKey];
            //client.DefaultRequestHeaders.Add(authorizationHeaderKey, authorizationHeaderValue);

            // List data response.
            HttpResponseMessage response = client.PostAsJsonAsync(path, objectToPost).Result;  // Blocking call!

            try
            {
                if (response.IsSuccessStatusCode)
                {
                    // Parse the response body. Blocking!
                    var responseObject = response.Content.ReadAsAsync<TResult>().Result;
                    return responseObject;
                }

                return ReturnError<TResult>(response.ReasonPhrase);
            }
            catch (Exception ex)
            {
                return ReturnError<TResult>(ex.Message);
            }
        }


        private static TResult ReturnError<TResult>(string message) where TResult : MiddlewareResponse
        {
            Type objectType = typeof(TResult);
            var instance = (TResult)Activator.CreateInstance(objectType);
            instance.ErrorMessage = message;
            return instance;
        }

    }
}
