namespace OMT.DTO
{
    public class UserListResponseDTO
    {
        public int UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public bool IsHardStateUser { get; set; }
        public string EmployeeId { get; set; }
    }
}
