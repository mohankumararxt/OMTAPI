namespace OMT.DTO
{
    public class UserSkillSetResponseDTO
    {
        public string? UserName { get; set; }
        public int UserId { get; set; }
        public int UserSkillSetId { get; set; }
        public int SkillSetId { get; set; }
        public string? SkillSetName { get; set; }
        public bool IsPrimary { get; set; }
        public int Percentage { get; set; }
        public bool IsHardStateUser { get; set; }
        public string? HardStateName { get; set; }
        
    }
}
