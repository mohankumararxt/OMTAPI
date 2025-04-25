using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DataAccess.Entities
{
    public class Productivity_Percentage
    {
        [Key]
        public int Productivity_PercentageId { get; set; }
        public int AgentUserId { get; set; }
        public int TlUserId { get; set; }
        public int SystemofRecordId { get; set; }
        public int SkillSetId { get; set; }
        public int Threshold { get; set; }
        public int OrdersCompleted { get; set; }
        public int ProductivityPercentage { get; set; }
        public decimal HoursWorked { get; set; }
        public DateTime Createddate { get; set; }
        public bool IsPrimarySor { get; set; }
        public int ShiftHours { get; set; }
        public int Utilization { get; set; }

    }
}
