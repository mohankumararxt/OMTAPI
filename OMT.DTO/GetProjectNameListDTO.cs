namespace OMT.DTO
{
    public class GetProjectNameListDTO
    {
        public int MasterProjectNameId { get; set; }
        public int SkillSetId { get; set; }
        public string SkillSetName { get; set; }
        public List<ProjectNamesDTO> ProjectNames { get; set; }

    }

    public class ProjectNamesDTO
    {
        public string ProjectId { get; set; }
        public string ProjectName { get; set; }
    }
}
