namespace OMT.DataAccess.Entities
{
    public class InvoiceSkillSet
    {
        public int InvoiceSkillSetId { get; set; }
        public  string SkillSetName { get; set; }
        public string MergeSkillSets { get; set; }
        public string CompareSkillSets { get; set; }
        public int OperationType { get; set; }  
        public bool ShowInInvoice { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public int SystemofRecordId { get; set; }
    }
}
