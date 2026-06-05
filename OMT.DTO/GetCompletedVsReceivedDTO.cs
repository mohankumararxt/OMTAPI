namespace OMT.DTO
{
    public class DayWiseCompletedDTO
    {
        public string Date { get; set; }
        public dynamic StatusCount { get; set; }

    }

    public class DayWiseReceivedDTO
    {
        public string Date { get; set; }
        public dynamic ReceivedCount { get; set; }

    }
    public class GetCompletedVsReceivedDTO
    {
        public List<DayWiseCompletedDTO> Completed { get; set; }
        public List<DayWiseReceivedDTO> Received { get; set; }
    }
}
