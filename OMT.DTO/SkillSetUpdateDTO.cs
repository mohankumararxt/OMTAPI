using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DTO
{
    public class SkillSetUpdateDTO
    {
        public int SkillSetId { get; set; } 
        public int Threshold { get; set; }
        public List<string>? StateName { get; set; }
        public bool IsHardState { get; set; }  
    }
}
