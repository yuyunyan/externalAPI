using Sourceportal.SAP.Domain.Models.Middleware.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Sourceportal.SAP.Domain.Models.Middleware
{
    public class MiddlewareSyncRequest<T> where T : MiddlewareSyncBase
    {
        private string _direction = MiddlewareSyncDirections.Incoming.ToString();
        private string _action = MiddlewareActionType.Sync.ToString();

        public MiddlewareSyncRequest(int objectId, string objectType)
        {
            ObjectId = objectId;
            ObjectType = objectType;
        }

        [DataMember(Name = "objectId")]
        public int ObjectId { get; private set; }

        [DataMember(Name = "objectType")]
        public string ObjectType { get; private set; }

        [DataMember(Name = "action")]
        public string Action { get { return _action; } set { _action = value; } }

        [DataMember(Name = "data")]
        public T Data { get; set; }

        [DataMember(Name = "direction")]
        public string Direction { get { return _direction; } set { _direction = value; } }

        [DataMember(Name = "source")]
        public string Source { get { return "SAP"; } }
    }
}
