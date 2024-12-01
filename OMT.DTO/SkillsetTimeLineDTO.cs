using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DTO
{
    public class SkillSetTimeLineDTO
    {
        public int SkillSetId { get; set; }
        public List<TimelineDetailDTO> HardStateTimelineDetails { get; set; }

        public List<TimelineDetailDTO> NormalStateTimelineDetails { get; set; }
    }

    public class TimelineDetailDTO
    {
        public string? HardStateName { get; set; }
        public int ExceedTime { get; set; }
        public bool IsHardstate { get; set; }

    }
}