using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DataAccess.Entities
{
    public class Daily_system_pending_Count
    {
        [Key]
        public int Daily_system_pending_CountId { get; set; }
        public int SystemofRecordId { get; set; }
        public int SkillSetId { get; set; }
        public DateTime Date { get; set; }
        public int Count { get; set; }
        public int Pre_day_count { get; set; }

    }
}
