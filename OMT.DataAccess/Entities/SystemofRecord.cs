namespace OMT.DataAccess.Entities
{
    public class SystemofRecord
    {
        public int SystemofRecordId { get; set; }
        public string SystemofRecordName { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
