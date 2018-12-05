using System;
using System.ComponentModel;

namespace Sourceportal.SAP.Services.Utils
{
    public static class EnumExtensions
    {
        public static string GetEnumDescription(this Enum value)
        {
            var enumType = value.GetType();
            var field = enumType.GetField(value.ToString());
            var attributes = field.GetCustomAttributes(typeof(DescriptionAttribute), false);

            // return  
            return attributes.Length == 0 ? value.ToString() : ((DescriptionAttribute)attributes[0]).Description;
        }

    }
}
