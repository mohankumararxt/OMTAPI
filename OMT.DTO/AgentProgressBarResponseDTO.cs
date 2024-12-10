using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DTO
{
    public class AgentProgressBarResponseDTO
    {
        public string username { get; set; }
        public string email { get; set; }
        public int wpm { get; set; }
        public float accuracy {  get; set; }
        public  DateOnly testdate { get; set; }

    }
}
