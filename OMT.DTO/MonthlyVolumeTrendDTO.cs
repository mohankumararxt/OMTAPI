namespace OMT.DTO
{
    public class MonthlyVolumeTrendDTO
    {
        public int? SystemOfRecordId { get; set; }
        public int? SkillsetId { get; set; }
    }

    public class MonthlyVolumeTrendResponseDTO
    {
        public int SystemOfRecordId { get; set; }
        public string SystemOfRecordName { get; set; }
        public dynamic MonthlyCount { get; set; }

    }

    public class MonthCountDTO
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int Count { get; set; }
    }
}
