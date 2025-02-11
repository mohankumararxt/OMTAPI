using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DTO
{
    public class ShiftAssociationResponseDTO
    {
        public string AgentName { get; set; }
        public List<ShiftDTO> ShiftDetails { get; set; }
    }
    public class ShiftDTO
    {
        public string TLName { get; set; }
        public string TLId { get; set; }
        public int PrimarySystemOfRecordId { get; set; }
        public string PrimarySystemOfRecordName { get; set; }
        public int ShiftAssociationId { get; set; }
        public string ShiftCode { get; set; }
        public string ShiftDate { get; set; }
    }

}
