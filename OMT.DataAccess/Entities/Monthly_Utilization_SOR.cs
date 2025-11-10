using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DataAccess.Entities
{
    public class Monthly_Utilization_SOR
    {
        [Key]
        public int Monthly_Utilization_SORId { get; set; }
        public int SystemofRecordId { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public int Utilization { get; set; }
    }
}
