using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DTO
{
    public class UpdateShiftAssociationDTO
    {
        public int ShiftAssociationId { get; set; }
        public string? TlEmployeeId { get; set; }
        public int? PrimarySystemOfRecordId { get; set; }
        public string? ShiftCode { get; set; }
       


    }
}
