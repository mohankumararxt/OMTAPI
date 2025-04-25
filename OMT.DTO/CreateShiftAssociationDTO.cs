using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DTO
{
    public class CreateShiftAssociationDTO
    {
        public string AgentEmployeeId { get; set; }
        public string TlEmployeeId { get; set; }
        public int PrimarySystemOfRecordId { get; set; }
        public string ShiftCode { get; set; }
        public DateTime ShiftDate { get; set;}
    }
}
