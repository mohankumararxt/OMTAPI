using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DataAccess.Entities
{
    public class UserSkillSet
    {
        public int UserSkillSetId { get; set; }
        public int UserId { get; set; }
        public int SkillSetId { get; set; }
        public int Percentage { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
}
}
