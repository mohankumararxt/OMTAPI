using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DTO
{
    public class ShiftAssociationResponseDTO
    {
        public int ShiftAssociationId { get; set; }
        public string AgentName { get; set; }
        public string TlName{ get; set; }
        public string ShiftCode { get; set; }
        public DateTime ShiftDate { get; set; }
    }
}
