using System.ComponentModel.DataAnnotations;

namespace OMT.DTO
{
    public class SkillSetResponseDTO
    {
        public int SkillSetId { get; set; }
        public int SystemofRecordId { get; set; }
        public string? SystemofRecordName { get; set; }
        public string? SkillSetName { get; set; }
        public int Threshold { get; set; }
        //public string? StateName {  get; set; } 
        public string[]? StateName { get; set; }
        public bool? IsHardState { get; set; }
        public bool? GetOrderByProject { get; set; }
    }
}
