using System.ComponentModel.DataAnnotations;

namespace OMT.DataAccess.Entities
{
    public class UserProfile
    {
        [Key]
        public int UserId { get; set; }
        public int OrganizationId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Mobile { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public int? RoleId { get; set; }
        public bool Is_Verified { get; set; }
        public bool Is_Active { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? Last_Login { get; set; }

    }
}
