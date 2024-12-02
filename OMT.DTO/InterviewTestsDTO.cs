using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DTO
{
    public class InterviewTestsDTO
    {
        public int id {  get; set; }
        public int UserId { get; set; }
        public int TestId { get; set; }

        public string Test_text { get; set; } = string.Empty;
        public int Duration { get; set; }
    }
}
