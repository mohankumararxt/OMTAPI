using System.ComponentModel.DataAnnotations;

namespace OMT.DataAccess.Entities
{
    public class Regularization_Status
    {
        [Key]   
        public int Id { get; set; }
        public string Status_Name { get; set; }
        public bool IsActive { get; set; }
    }
}
