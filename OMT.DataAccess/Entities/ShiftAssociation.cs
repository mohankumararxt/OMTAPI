using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DataAccess.Entities
{
    public class ShiftAssociation
    {
        public int ShiftAssociationId { get; set; }
        public string AgentEmployeeId { get; set; }
        public string TlEmployeeId { get; set; }
        public int PrimarySystemOfRecordId { get; set; }
        public string ShiftCode { get; set; }
        public DateTime ShiftDate { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public int CreatedBy { get; set; }
        public int ModifiedBy { get; set; }

    }
}
