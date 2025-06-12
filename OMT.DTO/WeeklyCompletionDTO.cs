using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DTO
{
    public class WeeklyCompletionDTO
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int SystemOfRecordId { get; set; }
        public int SkillsetId { get; set; }
    }

    public class WeeklyCompletionResponseDTO
    {
        public string Date { get; set; }
        public dynamic StatusCount { get; set; }
       
    }
}
