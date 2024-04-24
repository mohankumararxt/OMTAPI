using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DataAccess.Entities
{
    public class Workflowstatus
    {
        [Key]
        public int WorkflowstatusId { get; set; }
        public string WorkflowstatusName { get; set;}
        public bool IsActive { get; set; }
    }
}
