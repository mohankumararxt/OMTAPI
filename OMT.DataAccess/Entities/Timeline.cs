namespace OMT.DataAccess.Entities
{
    public class Timeline
    {
        public int TimelineId { get; set; }
        public int SkillSetId { get; set; }
        public string? Hardstatename { get; set; }
        public int ExceedTime { get; set; }
        public bool IsHardState { get; set; }
        public bool IsActive { get; set; }

    }
}
