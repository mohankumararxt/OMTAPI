using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DTO
{
    public class AgentDashDTO
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }

    public class AgentCompletionResultDTO
    {
        public string SystemOfRecord { get; set; }
        public string SkillSet { get; set; }
        public int Completed { get; set; }
        public int Exception { get; set; }
        public int Pending { get; set; }
        public int Complex { get; set; }
        public int Not_Keyed { get; set; }
        public int Reject { get; set; }
        public int Completed_NoFind { get; set; }
        public int Completed_Manual { get; set; }
        public int Not_found { get; set; }
    }
}
