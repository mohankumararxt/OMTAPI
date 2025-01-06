using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DTO
{
    public class EditShiftDetailsDTO
    {
        public int ShiftCodeId { get; set; }
        public string ShiftCode { get; set; }
        public string ShiftStartTime { get; set; }
        public string ShiftEndTime { get; set; }
        public string ShiftDays { get; set; }

    }
}
