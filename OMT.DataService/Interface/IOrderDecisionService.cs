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
        ResultDTO UpdateGetOrderCalculation();
        void InsertGetOrderCalculation(ResultDTO resultDTO, int userid);
        void Update_by_priorityOrder(ResultDTO resultDTO, SqlConnection connection, int userid);
        ResultDTO GetOrderForUser(int userid);
        ResultDTO GetTrdPendingOrderForUser(int userid);
    }
}
