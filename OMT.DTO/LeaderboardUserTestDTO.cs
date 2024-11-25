using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DTO
{
    public class LeaderboardUserTestDTO
    {
        public string username { get; set; }  
        public string text { get; set; }
        public int? wpm { get; set; }
        public Double? accuracy { get; set; }
        public DateOnly testdate { get; set; }
    }
}
