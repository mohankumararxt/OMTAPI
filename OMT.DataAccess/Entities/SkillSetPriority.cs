using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DataAccess.Entities
{
    public class SkillSetPriority
    {
        public int Id { get; set; }
        public int SkillSetId { get; set; }
        public bool IsComplete { get; set; }
        public DateTime CreatedDate { get; set; }   
    }
}
