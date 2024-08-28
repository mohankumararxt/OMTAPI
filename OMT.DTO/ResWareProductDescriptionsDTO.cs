namespace OMT.DTO
{
    public class ResWareProductDescriptionsDTO
    {
        public int SkillSetId { get; set; }
        public int ProductDescriptionId { get; set; }
        public List<int> ResWareProductDescriptionIds { get; set; }

    }

    public class ResWareProductDescriptionOnlyDTO
    {
         public List<string> ResWareProductDescriptionNames { get; set; }
    }



}
