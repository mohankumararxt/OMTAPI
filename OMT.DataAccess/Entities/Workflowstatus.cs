using System.ComponentModel.DataAnnotations;

namespace OMT.DataAccess.Entities
{
    public class Workflowstatus
    {
        [Key]
        public int WorkflowstatusId { get; set; }
        public int SkillSetId { get; set; }
        public string ProcessType { get; set; }
        public bool IsProjectIdUsed { get; set; }
        public string ProjectId { get; set; }
        public bool IsWsDefault { get; set; }
        public string WorkFlowStatus { get; set; }

    }
}
    