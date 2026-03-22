using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DTO
{
    public class RetrieveSystemPendingOrdersRequsetDTO
    {
        public string SystemOfRecordName { get; set; }
        public string SkillSetName { get; set; }
        public DateTime Scheduled_Datetime { get; set; }
    }
}
