using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DataAccess.Entities
{
    public class Tat_History
    {
        [Key]
        public int Tat_HistoryId { get; set; }
        public int SciPendingStatusSkillsetsId { get; set; }
        public DateTime TatDate { get; set; }
        public int DisabledBy { get; set; }
        public DateTime DisabledTime { get; set; }
        public int? EnabledBy { get; set; }
        public DateTime? EnabledTime { get; set; }
    }
}
