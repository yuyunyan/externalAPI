using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace Sourceportal.SAP.Services.CustomConfigs
{
    public class SapOrganizationsSection : ConfigurationSection
    {

        /*[ConfigurationProperty("orgDetails")]
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
        }*/

        [ConfigurationProperty("", IsRequired = true, IsDefaultCollection = true)]
        public SapOrganizationsInstanceCollection Instances
        {
            get { return (SapOrganizationsInstanceCollection)this[""]; }
            set { this[""] = value; }
        }
    }

    public class SapOrganizationsInstanceCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new SapOrganizationElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            //set to whatever Element Property you want to use for a key
            return ((SapOrganizationElement)element);
        }
    }

    public class SapOrganizationElement : ConfigurationElement
    {
        [ConfigurationProperty("name", IsRequired = true)]
        //[StringValidator(InvalidCharacters = "~!@#$%^&*()[]{}/;'\"|\\", MinLength = 1, MaxLength = 60)]
        public string Name
        {
            get
            {
                return (string) base["name"];
            }
            set
            {
                base["name"] = value;
            }
        }

        [ConfigurationProperty("type", IsRequired = true)]
        //[StringValidator(InvalidCharacters = "~!@#$%^&*()[]{}/;'\"|\\", MinLength = 1, MaxLength = 60)]
        public string Type
        {
            get
            {
                return (string) base["type"];
            }
            set
            {
                base["type"] = value;
            }
        }

        [ConfigurationProperty("company", IsRequired = false)]
        //[StringValidator(InvalidCharacters = "~!@#$%^&*()[]{}/;'\"|\\", MinLength = 1, MaxLength = 60)]
        public string Company
        {
            get
            {
                return (string)base["company"];
            }
            set
            {
                base["company"] = value;
            }
        }
    }

}
