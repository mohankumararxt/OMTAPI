using System.ComponentModel.DataAnnotations;

namespace OMT.DataAccess.Entities
{
    public class CostCenter
    {
        [Key]
        public int CostCenterId { get; set; }
        public string CostCenterAmount { get; set; }
    }
}
