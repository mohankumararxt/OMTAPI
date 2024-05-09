using System.ComponentModel.DataAnnotations;

namespace OMT.DataAccess.Entities
{
    public class SkillSetHardStates
    {
        [Key]
        public int Id { get; set; }
        public int SkillSetId { get; set; }
        public string StateName { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }

    }
}
