﻿using OMT.DTO;

namespace OMT.DataService.Interface
{
    public interface ITemplateService
    {
        ResultDTO CreateTemplate(CreateTemplateDTO createTemplateDTO);
        ResultDTO DeleteTemplate(int SkillSetId);
        ResultDTO UploadOrders(UploadTemplateDTO uploadTemplateDTO);
        ResultDTO ValidateOrders(ValidateOrderDTO validateorderDTO);
        ResultDTO GetOrders(int userid);
        ResultDTO GetTemplateList();
        ResultDTO UpdateOrderStatus(UpdateOrderStatusDTO updateOrderStatusDTO);
        ResultDTO AgentCompletedOrders(AgentCompletedOrdersDTO agentCompletedOrdersDTO);
        ResultDTO TeamCompletedOrders(TeamCompletedOrdersDTO teamCompletedOrdersDTO);
        ResultDTO GetDefaultColumnNames(int systemofrecordid);
        
    }
}
