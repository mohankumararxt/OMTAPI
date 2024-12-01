using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DTO
{
    public class SkillSetTimelineResponseDTO
    {
        public int SkillSetId { get; set; }
        public List<ResponseTimelineDetailDTO> HardStateTimelineDetails { get; set; }

        public List<ResponseTimelineDetailDTO> NormalStateTimelineDetails { get; set; }
    }

    public class ResponseTimelineDetailDTO
    {
        public string? HardStateName { get; set; }
        public int ExceedTime { get; set; }
        public bool IsHardstate { get; set; }

    }
}
