namespace OMT.DTO
{
    public class VolumeProjectionInputDTO
    {
        public int SystemOfRecordId { get; set; }
        public int SkillsetId { get; set; }
    }

    public class VolumeProjectionOutputDTO
    {
        public int Received { get; set; }
        public int Completed { get; set; }
        public int Not_Assigned { get; set; }
    }
}
