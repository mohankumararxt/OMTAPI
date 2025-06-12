using System.ComponentModel.DataAnnotations;

namespace OMT.DataAccess.Entities
{
    public class MonthlyCount_SkillSet
    {
        [Key]
        public int MonthlyCount_SkillSetId { get; set; }
        public int SystemofRecordId { get; set; }
        public int SkillSetId { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public int Count { get; set; }
    }
}
