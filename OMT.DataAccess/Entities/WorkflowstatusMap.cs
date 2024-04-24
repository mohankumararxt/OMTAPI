using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DataAccess.Entities
{
    public class WorkflowstatusMap
    {
        [Key]
        public int WorkflowstatusMapId { get; set; }
        public int SkillSetId { get; set; }
        public int WorkflowstatusId { get; set; }

    }
}
