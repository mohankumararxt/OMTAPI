using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DTO
{
    public class UserSkillSetCreateDTO
    {
        public int SkillSetId { get; set; }
        public int Percentage { get; set; }
        public bool IsPrimary { get; set; }
    }
}
