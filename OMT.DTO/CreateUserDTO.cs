namespace OMT.DTO
{
    public class CreateUserDTO
    {
        public int OrganizationId { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set;}
        public string? Email { get; set; }
        public int RoleId { get; set; }
        public string Password { get; set; }
        public string MobileNumber { get; set; }
        public string EmployeeId { get; set; }

    }
}
