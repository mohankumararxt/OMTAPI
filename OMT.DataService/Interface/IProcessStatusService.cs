using OMT.DTO;

namespace OMT.DataService.Interface
{
    public interface IProcessStatusService
    {
        ResultDTO GetStatusList(int systemofrecordid);
    }
}
