using System.ComponentModel.DataAnnotations;

namespace OMT.DataAccess.Entities
{
    public class GetOrderCalculation
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }
        public int UserSkillSetId { get; set; }
        public int SkillSetId { get; set; }
        public int TotalOrderstoComplete { get; set; }
        public int OrdersCompleted { get; set; }
        public int Weightage { get; set; }
        public int PriorityOrder { get; set; }
        public bool Utilized { get; set; }
        public bool IsActive { get; set; }
        public DateTime UpdatedDate { get; set; }
        public bool IsCycle1 { get; set; }
        public bool IsHardStateUser { get; set; }
        public bool HardStateUtilized { get; set; }
    }
}
