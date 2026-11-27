using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DataAccess.Entities
{
    public class DailyHardStateCount
    {
        [Key]
        public int Id { get; set; }
        public int SkillsetId { get; set; }
        public string? SkillsetName { get; set; }
        public DateTime AllocationDate { get; set; }
        public int TotalCount { get; set; }
        public DateTime CreateDate { get; set; }
    }
}
