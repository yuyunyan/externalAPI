using Sourceportal.SAP.Domain.Models.DB.Triggers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sourceportal.SAP.DB.Triggers
{
    public interface ITriggerRepository
    {
        TriggerDb AddTrigger(TriggerDb trigger);
    }
}
