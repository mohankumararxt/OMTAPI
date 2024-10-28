using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OMT.DTO;

namespace OMT.DataService.Interface
{
    public interface IOrderDecisionService
    {
        ResultDTO UpdateGetOrderCalculation();
        void InsertGetOrderCalculation(ResultDTO resultDTO, int userid);
        ResultDTO GetOrderForUser(int userid);
        ResultDTO GetTrdPendingOrderForUser(int userid);
        ResultDTO GetOrderInfo(OrderInfoDTO orderInfoDTO);  
    }
}
