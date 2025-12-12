using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DataAccess.Entities
{
    public class Monthly_Status_Count_SkillSet
    {
        [Key]

        public int Monthly_Status_Count_SkillSetId { get; set; }
        public int SystemofRecordId { get; set; }
        public int SkillSetId { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public int Status { get; set; }
        public int Count { get; set; }
    }
}
