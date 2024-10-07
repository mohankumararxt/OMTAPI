using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DTO
{
    public class SkillSetUpdateTimeLineDTO
    { 
        public int SkillSetId { get; set; }
        public List<UpdateTimelineDetailDTO> HardStateTimelineDetails { get; set; }

        public List<UpdateTimelineDetailDTO> NormalStateTimelineDetails { get; set; }
    }

    public class UpdateTimelineDetailDTO
    {
        public string? HardStateName { get; set; }
        public int ExceedTime { get; set; }
        public bool IsHardstate { get; set; }

    }
}

