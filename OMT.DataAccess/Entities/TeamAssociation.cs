using System.ComponentModel.DataAnnotations;

namespace OMT.DataAccess.Entities
{
    public class TeamAssociation
    {
        [Key]
        public int AssociationId { get; set; }
        public int? UserId { get; set; }
        public int? TeamId { get; set; }
        public int? ThresholdCount { get; set; }
        public string Description { get; set; }
        public DateTime? CreatedDate { get; set; }
    }
}
