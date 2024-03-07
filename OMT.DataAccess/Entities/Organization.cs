using System.ComponentModel.DataAnnotations;

namespace OMT.DataAccess.Entities
{
    public class Organization
    {
        [Key]
        public int OrganizationId { get; set; }
        public string? OrganizationName { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsActive { get; set; }

    }
}
