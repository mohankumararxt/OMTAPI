using System.ComponentModel.DataAnnotations;

namespace OMT.DataAccess.Entities
{
    public class DocType
    {
        [Key]
        public int DocTypeId { get; set; }
        public string DocumentName { get; set; }
        public bool IsActive { get; set; }
        public int TrdDocTypeId { get; set; }
    }
}
