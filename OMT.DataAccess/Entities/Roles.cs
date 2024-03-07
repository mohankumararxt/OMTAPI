using System.ComponentModel.DataAnnotations;

namespace OMT.DataAccess.Entities
{
    public class Roles
    {
        [Key]
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public bool IsActive { get; set; }

    }
}
