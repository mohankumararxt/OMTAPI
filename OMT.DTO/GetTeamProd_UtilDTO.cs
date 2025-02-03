using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DTO
{
    public class GetTeamProd_UtilDTO    
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int TeamId { get; set; }
    }

    public class GetTeamProd_AverageDTO
    {
        public GetTeamProd_AverageDTO()
        {
            TeamProductivity = new List<GetTeamProd_ResponseDTO>();
        }

        public int TotalAverageProductivity { get; set; }
        public List<GetTeamProd_ResponseDTO> TeamProductivity { get; set;}
    }


    public class GetTeamUtil_AverageDTO
    {
        public GetTeamUtil_AverageDTO()
        {
            TeamUtilization = new List<GetTeamUtil_ResponseDTO>();
        }

        public int TotalAverageUtilization { get; set; }
        public List<GetTeamUtil_ResponseDTO> TeamUtilization { get; set; }
    }
    public class GetTeamProd_ResponseDTO
    {
        public string AgentName { get; set; }
        public List<GetTeamProd_DatewisedataDTO> DatewiseData { get; set; }
        public int OverallProductivity { get; set; }

    }

    public class GetTeamProd_DatewisedataDTO
    {
        public string Date { get; set; }
        public int Productivity { get; set; }
    }

    public class GetTeamUtil_ResponseDTO
    {
        public string AgentName { get; set; }
        public List<GetTeamUtil_DatewisedataDTO> DatewiseData { get; set; }
        public int OverallUtilization { get; set; }

    }

    public class GetTeamUtil_DatewisedataDTO
    {
        public string Date { get; set; }
        public int Utilization { get; set; }
    }
}
