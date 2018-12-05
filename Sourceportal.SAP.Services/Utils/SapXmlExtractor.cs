using Sourceportal.SAP.DB.Triggers;
using Sourceportal.SAP.Domain.Models.DB.Triggers;
using Sourceportal.SAP.Domain.Models.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace Sourceportal.SAP.Services.Utils
{
    public static class SapXmlExtractor
    {
        public static String GetXmlString<T>(T request){
            string xmlString = string.Empty;
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = false;
            settings.NewLineHandling = NewLineHandling.None;
            settings.OmitXmlDeclaration = true;

            using (StringWriter stringWriter = new StringWriter())
            {
                using (XmlWriter xmlWriter = XmlWriter.Create(stringWriter, settings))
                {
                    XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
                    namespaces.Add(string.Empty, string.Empty);
                    XmlSerializer xmlSerializer = new XmlSerializer(request.GetType());
                    xmlSerializer.Serialize(xmlWriter, request, namespaces);
                    xmlString = stringWriter.ToString();
                    xmlWriter.Close();
                }
                stringWriter.Close();
            }

            return xmlString;
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

        /*public static TriggerXml DeserializeXml(HttpRequestMessage request)
        {
            var doc = new XmlDocument();
            doc.Load(request.Content.ReadAsStreamAsync().Result);
            LogTrigger(doc.DocumentElement.OuterXml);
            var serializer = new XmlSerializer(typeof(TriggerXml));
            var stream = GenerateStreamFromString(doc.DocumentElement.OuterXml);
            TriggerXml sapTrigger = (TriggerXml)serializer.Deserialize(stream);
            return sapTrigger;
        }*/

        public static void LogTrigger(Trigger xml, string objectType, string host)
        {
            var triggerRepo = new TriggerRepository();
            var trigger = new TriggerDb { Trigger = GetXmlString(xml), Created = DateTime.Now, ObjectType = objectType, Host = host };
            triggerRepo.AddTrigger(trigger);
        }

    }
}
