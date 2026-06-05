using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DTO
{
    public class GetNormalStateNameListDTO
    {
       // public int MasterNormalStateId { get; set; }
        public int SkillSetId { get; set; }
        public string SkillSetName { get; set; }
        public List<NormalStateNameDTO> NormalStateNames { get; set; }
    }

    public class NormalStateNameDTO
    {
        public string NormalStateCode { get; set; }
        public string NormalStateName { get; set; }
    }
}
