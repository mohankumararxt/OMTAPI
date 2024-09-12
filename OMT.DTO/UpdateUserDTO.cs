namespace OMT.DTO
{
    public class UpdateUserDTO
    {


        public int UserId { get; set; }
        public int OrganizationId { get; set; }
        public int? RoleId { get; set; }
        public string? Password { get; set; }
        public string? Mobile { get; set; }


    }
}

