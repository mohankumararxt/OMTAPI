namespace OMT.DTO
{
    public class GetSkillsetWiseProductivity_DTO
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public List<int> SkillSetId { get; set; }
        public int? TeamId { get; set; }
    }

    public class GetSkillsetWiseProductivity_ResponseDTO
    {
        public List<GetTeamProd_DatewisedataDTO> DatewiseData { get; set; }
        public int OverallProductivity { get; set; }
    }

    public class GetSkillsetWiseProductivityUtil_ResponseDTO
    {
        public List<GetTeamUtil_DatewisedataDTO> DatewiseData { get; set; }
        public int OverallUtilization { get; set; }
    }
}
