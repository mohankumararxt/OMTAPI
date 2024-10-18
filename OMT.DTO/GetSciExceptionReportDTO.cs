namespace OMT.DTO
{
    public class GetSciExceptionReportDTO
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }

    public class SciExceptionReportResponseDTO
    {
        public int Id { get; set; }
        public string Project { get; set; }
        public string Loan { get; set; }
        public string Valid_Invalid { get; set; }
        public string Status { get; set; }
        public string Question { get; set; }
        public string Code { get; set; }
        public string CodeName { get; set; }
        public string Description { get; set; }
        public string Comments { get; set; }
        public string Date_Created { get; set; }
    }
}
    