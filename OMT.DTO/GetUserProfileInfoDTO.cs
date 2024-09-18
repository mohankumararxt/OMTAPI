using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DTO
{
    public class GetUserProfileInfoDTO
    {

        public int UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public bool IsHardStateUser { get; set; }
        public string EmployeeId { get; set; }  
        public int OrganizationId { get; set; }
        public string OrganizationName { get; set; }
        public string Mobile { get; set; }
        public string Password { get; set; }
        public int RoleId { get; set; }
        public string RoleName { get; set; }
    }
}
