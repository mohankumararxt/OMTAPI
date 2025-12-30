using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DataService.Settings
{
    public class Create_batchesSettings
    {
        public int RicSkillsetId { get; set; }
        public string TriggerURL { get; set; }
        public List<int> DocPrepSkillsetIds { get; set; }
        public string DocPrepTriggerURL { get; set; }
    }
}
