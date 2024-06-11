namespace OMT.DTO
{
    public class AssignOrderToUserDTO
    {
        public List<Dictionary<string, object>> Orders { get; set; }
        public int UserId { get; set; }
    }
}
