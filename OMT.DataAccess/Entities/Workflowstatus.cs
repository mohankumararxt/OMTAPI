using System.ComponentModel.DataAnnotations;

namespace OMT.DataAccess.Entities
{
    public class Workflowstatus
    {
        [Key]
        public int WorkflowstatusId { get; set; }
        public string WorkflowstatusName { get; set;}
        public string WorkflowstatusAlias { get; set; }
        public bool IsActive { get; set; }
    }
}
    