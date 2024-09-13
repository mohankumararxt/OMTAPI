namespace OMT.DTO
{
    public class TeamAssociationResponseDTO
    {
        public string FirstName { get; set; }
        public string? TeamName { get; set; }
        public int? ThresholdCount { get; set; }
        public string Description { get; set; }
        public int AssociationId { get; set; }
        public string UserName { get; set; }
        public int TeamId { get; set; }
        public int UserId { get; set; }
    }
}
