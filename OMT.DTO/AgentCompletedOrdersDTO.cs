namespace OMT.DTO
{
    public class AgentCompletedOrdersDTO
    {
        public int UserId { get; set; }
        public int? SystemOfRecordId { get; set; }
        public int? SkillSetId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set;}
        public Dateoption DateFilter { get; set; }
    }

    public enum Dateoption
    {
        FilterBasedOnAllocationdate = 1,
        FilterBasedOnCompletiontime = 2,
    }
}
