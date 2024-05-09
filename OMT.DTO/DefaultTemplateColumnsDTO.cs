namespace OMT.DTO
{
    public class DefaultTemplateColumnsDTO
    {
        public string DefaultColumnName { get; set; }
    }

    public class DefaultTemplateColumnlistDTO
    {
        public string DefaultColumnName { get; set; }
        public int SystemofRecordId { get; set; }
        public string DataType { get; set; }
        public bool IsDuplicateCheck { get; set; }
    }
}
