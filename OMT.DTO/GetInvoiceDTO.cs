namespace OMT.DTO
{
    public class GetInvoiceDTO
    {
        public int? SkillSetId { get; set; }
        public string? SkillSetName { get; set; }
        public int SystemofRecordId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public PaginationInputDTO Pagination { get; set; }
    }
}
