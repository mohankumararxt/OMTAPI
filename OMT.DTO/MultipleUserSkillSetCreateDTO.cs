using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DTO
{
    public class MultipleUserSkillSetCreateDTO
    {
        public int UserId { get; set; }
        public List<UserSkillSetDetailDTO> FirstCycle { get; set; }

        public List<UserSkillSetDetailDTO> SecondCycle { get; set; }
    }

    public class HardStateDetails
    {
        public string HardStateName { get; set; }
        public int Weightage { get; set; }
    }
    public class UserSkillSetDetailDTO
    {
        public int SkillSetId { get; set; }
        public int? Weightage { get; set; }
        public bool IsHardStateUser { get; set; }
        public List<HardStateDetails> HardStateDetails { get; set; }

    }
}
