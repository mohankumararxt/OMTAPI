using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DataAccess.Entities
{
    public class MasterNormalStates
    {
        [Key]
        public int MasterNormalStateId { get; set; }
        public int SkillSetId { get; set; }
        public string NormalStateCode { get; set; }
        public string NormalStateName { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
