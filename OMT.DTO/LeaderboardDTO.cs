using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DTO
{
    public class LeaderboardDTO
    {
        public string username { get; set; }
        public int experience { get; set; }
        public string email { get; set; }
        public string phone {  get; set; }
        public int? wpm { get; set; }
        public Double? accuracy { get; set; }
        public int duration { get; set; }
        public DateOnly testdate { get; set; }
    }
}
