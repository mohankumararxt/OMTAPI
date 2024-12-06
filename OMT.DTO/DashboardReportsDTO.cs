using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DTO
{

    public class SkillSetDataDTO
    {
        public string? SystemOfRecordName { get; set; }
        public string? SkillSetName { get; set; }
        public int Completed_Count { get; set; }
        public int Pending_Count { get; set; }
        public int Reject_Count { get; set; }
        public int Threshold { get; set; }
        public int Total_WorkingDays { get; set; }


    }
}
