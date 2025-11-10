using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DTO
{
    public class MonthlyUtilizationSorDTO
    {
        public List<int>? SystemOfRecordId { get; set; }
    }

    public class MonthlyUtilizationSorResponseDTO
    {
        public int SystemOfRecordId { get; set; }
        public string SystemOfRecordName { get; set; }
        public dynamic Monthly_Utilization { get; set; }

    }
}
