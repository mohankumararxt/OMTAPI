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
    }

    public class GetSorWiseProductivity_ResponseDTO
    {
        public List<GetTeamProd_DatewisedataDTO> DatewiseData { get; set; }
        public int OverallProductivity { get; set; }
    }

    public class GetSorWiseUtil_ResponseDTO
    {
        public List<GetTeamUtil_DatewisedataDTO> DatewiseData { get; set; }
        public int OverallUtilization { get; set; }
    }

    public class GetSorProd_AverageDTO
    {
        public GetSorProd_AverageDTO()
        {
            Productivity = new List<GetTeamProd_ResponseDTO>();
        }

        public int TotalOverallProductivity { get; set; }
        public List<GetTeamProd_ResponseDTO> Productivity { get; set; }
    }


    public class GetSorUtil_AverageDTO
    {
        public GetSorUtil_AverageDTO()
        {
            Utilization = new List<GetTeamUtil_ResponseDTO>();
        }

        public int TotalOverallUtilization { get; set; }
        public List<GetTeamUtil_ResponseDTO> Utilization { get; set; }
    }
}
