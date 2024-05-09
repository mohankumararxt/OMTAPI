using System.ComponentModel.DataAnnotations;

namespace OMT.DataAccess.Entities
{
    public class ProcessType
    {
        [Key]
        public int ProcessTypeId { get; set; }
        public string ProcessTypeName { get; set; }
        public bool IsActive { get; set; }
    }
}
