using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DataAccess.Entities
{
    public class SkillSet
    {
        [Key]
        public int SkillSetId { get; set; }
        public string? SkillSetName { get; set; } 
        public bool IsActive { get; set; }

    }
}
