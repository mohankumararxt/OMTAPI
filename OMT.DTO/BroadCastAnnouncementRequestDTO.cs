using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DTO
{
    public class BroadCastAnnouncementRequestDTO
    {
        public string BroadCastMessage { get; set; } = string.Empty; 

        public DateTime StartDateTime { get; set; }

        public DateTime EndDateTime { get; set; }
    }
}
