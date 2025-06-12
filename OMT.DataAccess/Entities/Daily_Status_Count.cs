using System.ComponentModel.DataAnnotations;

namespace OMT.DataAccess.Entities
{
    public class Daily_Status_Count
    {
        [Key]
        public int Daily_Status_CountId { get; set; }
        public int SystemofRecordId { get; set; }
        public int SkillSetId { get; set; }
        public DateTime Date { get; set; }
        public int Status { get; set; }
        public int Count { get; set; }
    }
}
