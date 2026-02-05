using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DTO
{
    public class UpdateCheckindateBotRequestDTO
    {
        public string SystemOfRecordName { get; set; }
        public string SkillSetName { get; set; }
        public string OrderId { get; set; }
        public DateTime EndTime { get; set; }
        public DateTime CheckIn_date { get; set; }
    }
}
