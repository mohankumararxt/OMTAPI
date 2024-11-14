using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DTO
{
    public class ConsolidatedUserSkillSetlistDTO
    {
        public string Username { get; set; }
        public int UserId { get; set; }
        public List<UserSkillSetDetailsDTO> FirstCycle { get; set; }
        public List<UserSkillSetDetailsDTO> SecondCycle { get; set; }
    }

    public class UPdateHardStateDetailsDTO
    {
        public string HardStateName { get; set; }
        public int Weightage { get; set; }
        public int UserSkillSetId { get; set; }
    }
    public class UserSkillSetDetailsDTO
    {
        public int UserSkillSetId { get; set; } 
        public int SkillSetId { get; set; }
        public string SkillSetName { get; set; }
        public int? Weightage { get; set; }
        public bool IsHardStateUser { get; set; }
        // public string? HardStateName { get; set; }
        public List<UPdateHardStateDetailsDTO> HardStateDetails { get; set; }
    }
}
