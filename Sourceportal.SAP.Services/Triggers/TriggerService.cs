using Sourceportal.SAP.DB.Triggers;
using Sourceportal.SAP.Domain.Models.DB.Triggers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sourceportal.SAP.Services.Triggers
{
    public class TriggerService : ITriggerService
    {

        private readonly ITriggerRepository _triggerRepo = null;

        public TriggerService(ITriggerRepository triggerRepo)
        {
            _triggerRepo = triggerRepo;
        }

        public void AddTrigger(string trigger)
        {
            TriggerDb triggerDb = new TriggerDb();

            triggerDb.Trigger = trigger;
            triggerDb.Created = DateTime.Now;

            try
            {
                _triggerRepo.AddTrigger(triggerDb);
            }
            catch (Exception ex)
            {
                
            }
        }
    }
}
