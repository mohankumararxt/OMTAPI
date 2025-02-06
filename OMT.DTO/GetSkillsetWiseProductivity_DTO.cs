namespace OMT.DTO
{
    public class GetSkillsetWiseProductivity_DTO
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public List<int> SkillSetId { get; set; }
        public int? TeamId { get; set; }
    }

    public class GetSkillsetWiseProd_ResponseDTO
    {
        public string AgentName { get; set; }
        public string SkillSet { get; set; }
        public List<GetTeamProd_DatewisedataDTO> DatewiseData { get; set; }
        public int OverallProductivity { get; set; }
    }

    public class GetSkillsetWiseUtil_ResponseDTO
    {
        public string AgentName { get; set; }
        public string SkillSet { get; set; }
        public List<GetTeamUtil_DatewisedataDTO> DatewiseData { get; set; }
        public int OverallUtilization { get; set; }
    }


    public class GetSkillsetProd_AverageDTO
    {
        public GetSkillsetProd_AverageDTO()
        {
            Productivity = new List<GetSkillsetWiseProd_ResponseDTO>();
        }

        public int TotalOverallProductivity { get; set; }
        public List<GetSkillsetWiseProd_ResponseDTO> Productivity { get; set; }
    }


    public class GetSkillsetUtil_AverageDTO
    {
        public GetSkillsetUtil_AverageDTO()
        {
            Utilization = new List<GetSkillsetWiseUtil_ResponseDTO>();
        }

        public int TotalOverallUtilization { get; set; }
        public List<GetSkillsetWiseUtil_ResponseDTO> Utilization { get; set; }
    }


}
