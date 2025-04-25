namespace OMT.DTO
{
    public class GetUserExcelDTO
    {
        public string UserName { get; set; }
        public string EmployeeId { get; set; }
        public string Email { get; set; }
    }

    public class CombinedUsersListDTO
    {
        public List<GetUserExcelDTO> TlnAbove {  get; set; }
        public List<GetUserExcelDTO> Agent { get; set; }
    }
}
