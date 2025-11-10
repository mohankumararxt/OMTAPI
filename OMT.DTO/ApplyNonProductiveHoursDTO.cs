namespace OMT.DTO
{
    public class ApplyNonProductiveHoursDTO
    {
        public int Reasons { get; set; }
        public string Remarks { get; set; }
        public DateTime NonProductiveHours_Date { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public decimal Applied_Hours { get; set; }
    }

   
}
