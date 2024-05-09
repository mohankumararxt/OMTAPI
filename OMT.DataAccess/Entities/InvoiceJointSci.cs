using System.ComponentModel.DataAnnotations;

namespace OMT.DataAccess.Entities
{
    public class InvoiceJointSci
    {
        [Key]
        public int InvoiceJointSciId { get; set; }
        public int SystemOfRecordId { get; set; }
        public int SkillSetId { get; set; }
        public int CustomerId { get; set; }
        public int BusinessId { get; set; }
        public int BusinessGroupId { get; set; }
        public int WorkflowstatusId { get; set; }
        public int CostCenterId { get; set; }
        public int TotalOrderFeesId { get; set; }
    }
}
