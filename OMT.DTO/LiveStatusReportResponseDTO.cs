namespace OMT.DTO
{
    public class LiveStatusReportResponseDTO
    {
        public List<AgentCompletionCountDTO> AgentCompletedOrdersCount { get; set; }
        public List<StatusCountDTO> StatusCount { get; set; }
        public dynamic CompletionReport { get; set; }
        public dynamic NotAssignedReport { get; set; }

    }

    public class AgentCompletionCountDTO
    {
        public string UserName { get; set; }
        public int TotalCount { get; set; }
    }

    public class StatusCountDTO
    {
        public string StatusName { get; set; }
        public int TotalCount { get; set; }
    }



}
