using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DTO
{
    public class UpdateUserSkillSetThWtDTO
    {
        public int UserId { get; set; }
        public List<UserSkillSetDetailsThWtDTO> FirstCycle { get; set; }
        public List<UserSkillSetDetailsThWtDTO> SecondCycle { get; set; }
    }
    public class UserSkillSetDetailsThWtDTO
    {
        public int? UserSkillSetId { get; set; } 
        public int SkillSetId { get; set; }
        public int? Weightage { get; set; }
        public bool IsHardStateUser { get; set; }
        public string? HardStateName { get; set; }
    }
}