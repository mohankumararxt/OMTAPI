namespace OMT.DTO
{
    public class GetAgentProd_UtilDTO
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }

    public class GetAgentProd_ResponseDTO
    {
        public string SkillSet { get; set; }
        public List<GetTeamProd_DatewisedataDTO> DatewiseData { get; set; }
        public int OverallProductivity { get; set; }
    }


    public class GetAgentProd_AverageDTO
    {
        public GetAgentProd_AverageDTO()
        {
           AgentProductivity = new List<GetAgentProd_ResponseDTO>();
        }

        public int TotalOverallProductivity { get; set; }
        public List<GetAgentProd_ResponseDTO> AgentProductivity { get; set; }
    }

    public class GetAgentUtil_AverageDTO
    {
        public GetAgentUtil_AverageDTO()
        {
            AgentUtilization = new List<GetAgentUtil_ResponseDTO>();
        }

        public int TotalOverallUtilization { get; set; }
        public List<GetAgentUtil_ResponseDTO> AgentUtilization { get; set; }
    }
    public class GetAgentUtil_ResponseDTO
    {
        public string SkillSet { get; set; }
        public List<GetTeamUtil_DatewisedataDTO> DatewiseData { get; set; }
        public int OverallUtilization { get; set; }
    }
}
