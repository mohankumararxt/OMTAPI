namespace OMT.DTO
{
    public class UpdateUserSkillsetListDTO
    {
        public int TeamId { get; set; }
        public int SystemOfRecordId { get; set; }

    }

    public class UpdateUserSkillsetListResponseDTO
    {   
        public string? UserName { get; set; }
        public int? UserId { get; set; }
        public List<UserskillsetAssociationdetailsDTO> UserSkillsetDetails { get; set; }
       
    }

    public class UserskillsetAssociationdetailsDTO
    {
        public int SkillSetId { get; set; }
        public string? SkillSetName { get; set; }
        public bool IsPrimary { get; set; }
        public int UserSkillSetId { get; set; }
    }

    public class userskillsetdetailsDTO
    {
        public List<string> SkillSetNames { get; set; }
        public List<UpdateUserSkillsetListResponseDTO> updateUserSkillsetListResponse { get; set; }

    }

    
}
