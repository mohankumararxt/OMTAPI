using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DTO
{
    public class GetSorWiseProductivity_DTO
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int SystemOfRecordId { get; set; }
        public int? TeamId { get; set; }
        public int IsSplit { get; set; }
    }

    public class GetSorWiseProductivity_ResponseDTO
    {
        public string AgentName { get; set; }
        public List<GetTeamProd_DatewisedataDTO> DatewiseData { get; set; }
        public int OverallProductivity { get; set; }
    }

    public class GetSorWiseUtil_ResponseDTO
    {
        public string AgentName { get; set; }
        public List<GetTeamUtil_DatewisedataDTO> DatewiseData { get; set; }
        public int OverallUtilization { get; set; }
    }

    public class GetSorProd_AverageDTO
    {
        public GetSorProd_AverageDTO()
        {
            Productivity = new List<GetSorWiseProductivity_ResponseDTO>();
        }

        public int TotalOverallProductivity { get; set; }
        public List<GetSorWiseProductivity_ResponseDTO> Productivity { get; set; }
    }


    public class GetSorUtil_AverageDTO
    {
        public GetSorUtil_AverageDTO()
        {
            Utilization = new List<GetSorWiseUtil_ResponseDTO>();
        }

        public int TotalOverallUtilization { get; set; }
        public List<GetSorWiseUtil_ResponseDTO> Utilization { get; set; }
    }
}
