﻿namespace OMT.DTO
{
    public class SkillSetCreateDTO
    {
        public int SystemofRecordId { get; set; }
        public string? SkillSetName { get; set; }
        public int Threshold { get; set; }
        public List<string>? HardstateNames { get; set; }  
        public bool IsHardState { get; set; }
    }
}
