namespace OMT.DTO
{
    public class SorCompletionCountInputDTO
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int SystemOfRecordId { get; set; }

    }

    public class SorCompletionCountDTO
    {
        public int SkillsetId { get; set; }
        public string SkillsetName { get; set; }
        public int Count { get; set; }
    }
}
