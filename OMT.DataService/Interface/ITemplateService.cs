﻿using OMT.DTO;

namespace OMT.DataService.Interface
{
    public interface ITemplateService
    {
        ResultDTO CreateTemplate(CreateTemplateDTO createTemplateDTO);
        ResultDTO DeleteTemplate(int SkillSetId);
        ResultDTO UploadOrders(UploadTemplateDTO uploadTemplateDTO,int userid);
        ResultDTO ValidateOrders(ValidateOrderDTO validateorderDTO);
        ResultDTO GetOrders(int userid);
        ResultDTO GetTemplateList();
        ResultDTO UpdateOrderStatus(UpdateOrderStatusDTO updateOrderStatusDTO);  //to call trd api
        ResultDTO AgentCompletedOrders(AgentCompletedOrdersDTO agentCompletedOrdersDTO);
        ResultDTO TeamCompletedOrders(TeamCompletedOrdersDTO teamCompletedOrdersDTO);
        ResultDTO GetDefaultColumnNames(int systemofrecordid);
        ResultDTO GetPendingOrderDetails(int userid);
        ResultDTO GetComplexOrdersDetails(ComplexOrdersRequestDTO complexOrdersRequestDTO);
        ResultDTO ReleaseOrder(ReleaseOrderDTO releaseOrderDTO);
        ResultDTO TimeExceededOrders(TimeExceededOrdersDTO timeExceededOrdersDTO);
        ResultDTO ReplaceOrders(ReplaceOrdersDTO replaceOrdersDTO);
        ResultDTO GetTemplateColumns(int skillsetId);
        ResultDTO RejectOrder(RejectOrderDTO rejectOrderDTO);
        ResultDTO AssignOrderToUser(AssignOrderToUserDTO assignOrderToUserDTO);
        ResultDTO DeleteOrders(DeleteOrderDTO deleteOrderDTO);
        ResultDTO SkillsetWiseReports(SkillsetWiseReportsDTO skillsetWiseReportsDTO);
        ResultDTO GetMandatoryColumnNames(int skillsetid);
        ResultDTO GetTrdPendingOrders(int userid);
    }
}
