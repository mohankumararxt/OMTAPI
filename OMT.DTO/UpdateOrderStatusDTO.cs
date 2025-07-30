namespace OMT.DTO
{
    public class UpdateOrderStatusDTO
    {
        public int Id { get; set; }
        public int StatusId { get; set; }
        public string Remarks { get; set; }
        public int SkillSetId { get; set; }
        public string? TrdStatus { get; set; }
        public int UserId { get; set; }
        public int? Number_Of_Documents { get; set; }
        public int? Number_Of_Manual_Splits { get; set; }
        public int? ImageID { get; set; }   
        public bool MoveToSecondKey { get; set; }
    }
}
