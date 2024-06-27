namespace OMT.DTO
{
    public class UserInfoToUpdateDTO
    {
        public int SkillSetId { get; set; }
        public int UserSkillSetId { get; set; }
        public int UserId { get; set; }
      
    }

    public class BulkUserSkillsetUpdateDTO
    {
        public List<UserInfoToUpdateDTO> UserInfoToUpdate { get; set; }
    }
}
