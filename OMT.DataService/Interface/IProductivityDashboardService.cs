using OMT.DTO;


namespace OMT.DataService.Interface
{
    public interface IProductivityDashboardService
    {
        ResultDTO GetTeamProductivity(GetTeamProd_UtilDTO getTeamProd_UtilDTO, int UserId);
        ResultDTO GetTeamUtilization(GetTeamProd_UtilDTO getTeamProd_UtilDTO, int UserId);
        ResultDTO GetTeamProdUtil(GetTeamProd_UtilDTO getTeamProd_UtilDTO);
        ResultDTO GetAgentProductivity(GetAgentProd_UtilDTO getAgentProdUtilDTO);
        ResultDTO GetAgentUtilization(GetAgentProd_UtilDTO getAgentProdUtilDTO);
        ResultDTO GetSkillSetWiseProductivity(GetSkillsetWiseProductivity_DTO getSkillsetWiseProductivity_DTO, int UserId);
        ResultDTO GetSkillSetWiseUtilization(GetSkillsetWiseProductivity_DTO getSkillsetWiseProductivity_DTO, int UserId);
        ResultDTO GetSorWiseProductivity(GetSorWiseProductivity_DTO getSorWiseProductivity_DTO);
        ResultDTO GetSorWiseUtilization(GetSorWiseProductivity_DTO getSorWiseProductivity_DTO);

    }
}


