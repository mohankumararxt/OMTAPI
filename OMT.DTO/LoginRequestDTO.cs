namespace OMT.DTO
{
    public class LoginRequestDTO
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class CheckinoutRequestDTO
    {
        public int UserId { get; set; }
        public DateTime DateTime { get; set; }
    }
}
