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

    public class GetTeamProd_ResponseDTO
    {
        public string AgentName { get; set; }
        public List<GetTeamProd_DatewisedataDTO> DatewiseData { get; set; }
        public decimal AverageProductivity { get; set; }

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
        public decimal AverageUtilization { get; set; }

    }

    public class GetTeamUtil_DatewisedataDTO
    {
        public string Date { get; set; }
        public int Utilization { get; set; }
    }
}
