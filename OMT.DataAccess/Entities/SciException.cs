using System.ComponentModel.DataAnnotations;

namespace OMT.DataAccess.Entities
{
    public class SciException
    {
        [Key]
        public int Id { get; set; }
        public string Project { get; set; }
        public string Loan { get; set; }
        public string Valid_Invalid { get; set; }
        public string Status { get; set; }
        public string Question { get; set; }
        public string Code { get; set; }
        public string CodeName { get; set; }
        public string Description { get; set; }
        public string Comments { get; set; }
        public DateTime Date_Created { get; set; }
    }
}
