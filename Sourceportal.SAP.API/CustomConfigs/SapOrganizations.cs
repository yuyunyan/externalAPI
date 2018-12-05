using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace Sourceportal.SAP.API.CustomConfigs
{
    public class SapOrganizations : ConfigurationSection
    {

        [ConfigurationProperty("orgDetails")]
        public SapOrganizationElement SapOrganization
        {
            get
            {
                return (SapOrganizationElement)this["orgDetails"];
            }
            set
            {
                this["orgDetails"] = value;
            }
        }

        public class SapOrganizationElement : ConfigurationElement
        {
            [ConfigurationProperty("name", DefaultValue = "Arial", IsRequired = true)]
            [StringValidator(InvalidCharacters = "~!@#$%^&*()[]{}/;'\"|\\", MinLength = 1, MaxLength = 60)]
            public String Name
            {
                get
                {
                    return (String)this["name"];
                }
                set
                {
                    this["name"] = value;
                }
            }

            [ConfigurationProperty("type", DefaultValue = "Arial", IsRequired = true)]
            [StringValidator(InvalidCharacters = "~!@#$%^&*()[]{}/;'\"|\\", MinLength = 1, MaxLength = 60)]
            public String Type
            {
                get
                {
                    return (String)this["type"];
                }
                set
                {
                    this["type"] = value;
                }
            }
        }


    }
}