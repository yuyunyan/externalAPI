using System.ComponentModel;

namespace Sourceportal.SAP.Domain.Models.Shared
{
    public enum SeverityCodeEnums
    {
        [Description("1")]
        Success = 1,
        [Description("2")]
        Warning = 2,
        [Description("3")]
        Error = 3
    }
}
