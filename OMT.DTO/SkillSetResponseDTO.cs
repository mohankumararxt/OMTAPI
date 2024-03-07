using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DTO
{
    public class SkillSetResponseDTO
    {
        public int SkillSetId { get; set; }
        public int SystemofRecordId { get; set; }
        public string SystemofRecordName { get; set; }
        public string? SkillSetName { get; set; } 
        public int Threshold { get; set; }

    }
}
