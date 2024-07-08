using OMT.DTO;

namespace OMT.DataService.Interface
{
    public interface ICostCenterService
    {
        ResultDTO CreateCostCenter(string CostcenterAmount);
        ResultDTO DeleteCostCenter(int costCenterId);
        ResultDTO GetCostCenterList();
        ResultDTO UpdateCostCenter(UpdateCostCenterDTO updateCostCenterDTO);
    }
}
