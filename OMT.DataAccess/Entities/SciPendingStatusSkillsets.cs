using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DataAccess.Entities
{
    public class SciPendingStatusSkillsets
    {
        [Key]
        public int Id { get; set; }
        public int SkillSetId { get; set; }
        public bool IsActive { get; set; }
    }
}
