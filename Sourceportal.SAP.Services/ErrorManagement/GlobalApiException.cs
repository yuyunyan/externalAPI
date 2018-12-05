using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sourceportal.SAP.Services.ErrorManagement
{
    public class GlobalApiException : Exception
    {


        public GlobalApiException()
        {

        }

        public GlobalApiException(string message) : base(message)
        {

        }

        public GlobalApiException(string message, Exception inner) : base(message, inner)
        {

        }

        public GlobalApiException(string message, string source) : base(message)
        {
            base.Source = source;
        }
    }
}
