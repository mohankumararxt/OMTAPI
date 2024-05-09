namespace OMT.DTO
{
    public class UserSkillSetUpdateDTO
    {
        public int UserId { get; set; }
        public int UserSkillSetId { get; set; }
        public int SkillSetId { get; set; }
        public bool IsPrimary { get; set; }
        public int Percentage { get; set; }
        public bool IsHardStateUser { get; set; }
        public List<string>? HardStateName { get; set; }
    }
}
