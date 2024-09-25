using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DTO
{
    public class SkillsetTimeLineDTO
    {
        public int SkillSetId { get; set; }
        public List<Timelinedetails> Timelinedetails { get; set; }  

        public List<Timelinedetails2> Timelinedetails2 { get; set; }    
    }
    public class Timelinedetails //add
    {
        public String? HardStateName { get; set; }
        public int ExceedTime { get; set; }
        public bool IsHardState { get; set; }

    }
    public class Timelinedetails2 //add
    {
        public String? HardStateName { get; set; }
        public int ExceedTime { get; set; }
        public bool IsHardState { get; set; }

    }

    //public class SkillsetTimeLineDTO
    //{
    //    public int SkillSetId { get; set; }
    //    public string? HardStateName { get; set; }
    //    public int ExceedTime { get; set; }
    //    public bool IsHardState { get; set; }

    //}
}
