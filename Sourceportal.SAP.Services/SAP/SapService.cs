using Sourceportal.SAP.DB.Organizations;
using Sourceportal.SAP.Domain.Models.Shared;
using Sourceportal.SAP.Services.Accounts;
using Sourceportal.SAP.Services.Middleware;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace Sourceportal.SAP.Services.SAP
{
    public class SapService : ISapService
    {
        private IBusinessPartnerService _businessPartnerService;
        private IBusinessPartnerSyncRequestCreator _businessPartnerSyncRequestCreator;
        private IMiddlewareService _middlewareService;

        public SapService(IBusinessPartnerService businessPartnerService, IBusinessPartnerSyncRequestCreator businessPartnerSyncRequestCreator,
            IMiddlewareService middlewareService)
        {
            _businessPartnerService = businessPartnerService;
            _businessPartnerSyncRequestCreator = businessPartnerSyncRequestCreator;
            _middlewareService = middlewareService;
        }

        /*public string SapUpdate(string sapRequest)
        {
            var serializer = new XmlSerializer(typeof(TriggerXml));
            TriggerXml sapTrigger = (TriggerXml)serializer.Deserialize(GenerateStreamFromString(sapRequest));
            return ProcessTrigger(sapTrigger);
        }*/

        public string ProcessTrigger(Trigger sapTrigger)
        {
            if(sapTrigger.TriggerObject.ToLower().Equals("supplier") || sapTrigger.TriggerObject.ToLower().Equals("customer"))
            {
                var businessPartner = _businessPartnerService.GetBusinessPartnerForIncoming(sapTrigger.ID);
                var syncRequest = _businessPartnerSyncRequestCreator.Create(businessPartner);
                return _middlewareService.Sync(syncRequest);
            }

            return null;
        }

        public static Stream GenerateStreamFromString(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        public void HandleInboundSapRequest(string sapRequest)
        {
            string ConnectionString = ConfigurationManager.ConnectionStrings["SourcePortalConnection"].ConnectionString;

            var con = new SqlConnection(ConnectionString);

            String query = "INSERT INTO dbo.tempSapRequests (Request) VALUES (@Request)";

            using (SqlCommand command = new SqlCommand(query, con))
            {
                command.Parameters.Add("@Request", sapRequest);

                con.Open();
                int result = command.ExecuteNonQuery();

                if (result < 0)
                    Console.WriteLine("Error Inserting SAP Request into Database!");
            }
        }


    }
}
