using OMT.DTO;
using System.Runtime.InteropServices.JavaScript;

namespace OMT.DataService.Interface
{
    public interface ITemplateService
    {
        ResultDTO CreateTemplate(CreateTemplateDTO createTemplateDTO);
        ResultDTO DeleteTemplate(int SkillSetId);
        ResultDTO UploadOrders(UploadTemplateDTO uploadTemplateDTO);
        ResultDTO ValidateOrders(UploadTemplateDTO uploadTemplateDTO);
        ResultDTO GetOrders(int userid);
        ResultDTO GetTemplateList();
        ResultDTO UpdateOrderStatus(UpdateOrderStatusDTO updateOrderStatusDTO);
        ResultDTO AgentCompletedOrders(AgentCompletedOrdersDTO agentCompletedOrdersDTO);
        ResultDTO TeamCompletedOrders(TeamCompletedOrdersDTO teamCompletedOrdersDTO);
    }
}
