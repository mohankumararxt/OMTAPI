using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DTO
{
    public class UserTestDTO
    {
        public string username { get; set; }

        public string text { get; set; }
        public int duration { get; set; }
        public int? wpm { get; set; }
        public Double? accuracy { get; set; }
        public int userTestId { get; set; }
        public DateOnly testdate { get; set; }

        public int pendingCount { get; set; }
    }
}