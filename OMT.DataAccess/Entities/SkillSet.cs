using System.ComponentModel.DataAnnotations;

namespace OMT.DataAccess.Entities
{
    public class SkillSet
    {
        [Key]
        public int SkillSetId { get; set; }
        public int SystemofRecordId { get; set; }
        public string SkillSetName { get; set; }
        public int Threshold { get; set; }
        public bool IsActive { get; set; }
        public bool IsHardState {  get; set; }

    }
}
