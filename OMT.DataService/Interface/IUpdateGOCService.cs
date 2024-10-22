using Microsoft.Data.SqlClient;
using OMT.DTO;

namespace OMT.DataService.Interface
{
    public interface IUpdateGOCService
    {
        void InsertGetOrderCalculation(ResultDTO resultDTO, int userid);
        void Update_by_priorityOrder(ResultDTO resultDTO, SqlConnection connection, int userid);
        ResultDTO UpdateGetOrderCalculation();
    }
}
