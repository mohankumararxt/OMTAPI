﻿using System.ComponentModel.DataAnnotations;

namespace OMT.DataAccess.Entities
{
    public class UserSkillSet
    {
        [Key]
        public int UserSkillSetId { get; set; }
        public int UserId { get; set; }
        public int SkillSetId { get; set; }
        public int Percentage { get; set; }
        public bool IsPrimary { get; set; }
        public bool IsActive { get; set; }  
        public DateTime CreatedDate { get; set; }
        public bool IsHardStateUser { get; set; }
        public string? HardStateName { get; set; }
        public bool IsCycle1 { get; set; }
        public string ProjectId { get; set; }
        public int PriorityOrder { get; set; }

    }
}
