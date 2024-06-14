using System.ComponentModel.DataAnnotations;

namespace OMT.DataAccess.Entities
{
    public class InvoiceDump
    {
        [Key]
        public int InvoiceDumpId { get; set; }
        public string SystemOfRecord { get; set; }
        public string SkillSet { get; set; }
        public DateTime CompletionDate { get; set; }
        public string CustomerId { get; set; }
        public string OrderId { get; set; }
        public string PropertyState { get; set; }
        public string County { get; set; }
        public string ProjectId { get; set; }
        public string BusinessGroup { get; set; }
        public string CostCenter { get; set; }
        public string ProcessType { get; set; }
        public string SourceType { get; set; }
        public string Customer { get; set; }
        public string Business { get; set; }
        public string Workflowstatus { get; set; }
        public string TotalOrderFees { get; set; }
        public string ProductDescription { get; set; }
    }
}
