namespace OMT.DTO
{
    //public class GetNphProductivityResponseDTO
    //{
    //    public string Productivity_Date { get; set; }
    //    public decimal Applied_Hours { get; set; }
    //    public int Productivity { get; set; }
    //    public int Non_Productive_Productivity { get; set; }
    //    public string Tl_Name { get; set; }
    //}

    //public class GetAgentNphProductivityResponseDTO
    //{
    //    public List<GetNphProductivityResponseDTO> Datewisedata { get; set; }
    //}

    public class GetAgentNphProductivityDTO
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int UserId { get; set; }
        public PaginationInputDTO Pagination { get; set; }
    }

    public class GetTeamNphProductivityDTO
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int? TeamId { get; set; }
        public PaginationInputDTO Pagination { get; set; }
    }


}
