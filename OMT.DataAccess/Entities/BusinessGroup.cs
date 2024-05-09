using System.ComponentModel.DataAnnotations;

namespace OMT.DataAccess.Entities
{
    public class BusinessGroup
    {
        [Key]
        public int BusinessGroupId { get; set; }
        public string BusinessGroupName { get; set; }
        public bool IsActive { get; set; }

    }
}
