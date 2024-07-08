using OMT.DTO;

namespace OMT.DataService.Interface
{
    public interface ITotalOrderFeesService
    {
        ResultDTO CreateTotalOrderFee(string TotalOrderfeeAmount);
        ResultDTO DeleteTotalOrderFee(int orderFeeId);
        ResultDTO GetTotalOrderFeeList();
        ResultDTO UpdateTotalOrderFee(UpdateTotalOrderFeeDTO updateTotalOrderFeeDTO);


    }
}
