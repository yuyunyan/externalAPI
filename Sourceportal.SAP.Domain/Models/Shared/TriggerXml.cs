using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Sourceportal.SAP.Domain.Models.Shared
{
    [Serializable()]
    [XmlRoot("Trigger")]
    public class TriggerXml
    {
        [XmlElement("TriggerObject", DataType = "string")]
        public string TriggerObject { get; set; }

        [XmlElement("ID", DataType = "string")]
        public string Id { get; set; }
    }

    public class Trigger
    {
        public string TriggerObject { get; set; }
        
        public string ID { get; set; }
    }
}
