using System.ComponentModel.DataAnnotations;

namespace OMT.DataAccess.Entities
{
    public class DailyCount_SkillSet
    {
        [Key]
        public int DailyCount_SkillSetId { get; set; }
        public int SystemofRecordId { get; set; }
        public int SkillSetId { get; set; }
        public DateTime Date { get; set; }
        public int Count { get; set; }  
    }
}
