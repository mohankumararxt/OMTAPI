using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DataAccess.Entities
{
    public class DailyCount_SOR
    {
        [Key]
        public int DailyCount_SORId { get; set; }
        public int SystemofRecordId { get; set; }
        public DateTime Date { get; set; }
        public int Count { get; set; }
    }
}
