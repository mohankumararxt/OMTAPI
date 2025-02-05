using OMT.DTO;


namespace OMT.DataService.Interface
{
    public interface IProductivityDashboardService
    {
        ResultDTO GetTeamProductivity(GetTeamProd_UtilDTO getTeamProd_UtilDTO);
        ResultDTO GetTeamUtilization(GetTeamProd_UtilDTO getTeamProd_UtilDTO);
        ResultDTO GetTeamProdUtil(GetTeamProd_UtilDTO getTeamProd_UtilDTO);
        ResultDTO GetAgentProductivity(GetAgentProd_UtilDTO getAgentProdUtilDTO,int UserId);
        ResultDTO GetAgentUtilization(GetAgentProd_UtilDTO getAgentProdUtilDTO, int UserId);
        ResultDTO GetSkillSetWiseProductivity(GetSkillsetWiseProductivity_DTO getSkillsetWiseProductivity_DTO);

    }
}


