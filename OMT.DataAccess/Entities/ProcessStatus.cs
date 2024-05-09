using System.ComponentModel.DataAnnotations;

namespace OMT.DataAccess.Entities
{
    public class ProcessStatus
    {
        [Key]
        public int Id { get; set; }
        public int SystemOfRecordId { get; set; }
        public string Status { get; set; }
        public bool IsActive { get; set; }
    }
}
