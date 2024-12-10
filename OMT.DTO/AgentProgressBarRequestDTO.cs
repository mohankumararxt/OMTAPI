using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DTO
{
    public class AgentProgressBarRequestDTO
    {
        public List<int> userids {get;set;}
        public DateTime startdate { get;set;}
        public DateTime enddate { get;set;}
    }
}
