using System.ComponentModel.DataAnnotations;

namespace OMT.DataAccess.Entities
{
    public class ResWareProductDescriptionMap
    {
        [Key]
        public int ResWareProductDescriptionMapId { get; set; }
        public int ResWareProductDescriptionId { get; set; }
        public int ProductDescriptionId { get; set; }
        public int SkillSetId { get; set; }

    }
}
