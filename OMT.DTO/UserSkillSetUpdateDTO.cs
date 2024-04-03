using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DTO
{
    public class UserSkillSetUpdateDTO
    {
        public int UserSkillSetId { get; set; }
        public int SkillSetId { get; set; }
        public bool IsPrimary { get; set; }
        public int Percentage { get; set; }
        public bool IsHardStateUser { get; set; }
        public List<string>? HardStateName { get; set; }
    }
}
