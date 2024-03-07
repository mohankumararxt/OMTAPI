namespace OMT.DataAccess.Entities
{
    public class UserSkillSet
    {
        public int UserSkillSetId { get; set; }
        public int UserId { get; set; }
        public int SkillSetId { get; set; }
        public int Percentage { get; set; }
        public bool IsPrimary { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
}
}
