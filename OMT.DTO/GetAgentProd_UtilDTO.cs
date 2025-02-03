namespace OMT.DTO
{
    public class GetAgentProd_UtilDTO
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }

    public class GetAgentProd_ResponseDTO
    {
        
        public List<GetTeamProd_DatewisedataDTO> DatewiseData { get; set; }
        public int OverallProductivity { get; set; }
    }
}
