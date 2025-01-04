using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using OMT.DTO;

namespace OMT.DataService.Interface
{
    public interface IOrderDecisionService
    {
        ResultDTO GetOrderForUser(int userid);
        ResultDTO GetTrdPendingOrderForUser(int userid);
        ResultDTO GetOrderInfo(OrderInfoDTO orderInfoDTO);
        ResultDTO UpdateOrderStatusByTL(int userid,UpdateOrderStatusByTLDTO updateOrderStatusByTLDTO);
        ResultDTO GetUnassignedOrderInfo(UnassignedOrderInfoDTO unassignedOrderInfoDTO);
        ResultDTO UpdateUnassignedOrder(int userid,UpdateUnassignedOrderDTO updateUnassignedOrderDTO);
        ResultDTO GetSkillsetOrderdetails(GetSkillsetOrderdetailsDTO getSkillsetOrderdetailsDTO);
    }
}