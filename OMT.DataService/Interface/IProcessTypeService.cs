using OMT.DTO;

namespace OMT.DataService.Interface
{
    public interface IProcessTypeService
    {
        ResultDTO CreateProcessType(string ProcessType);
        ResultDTO UpdateProcessType(ProcessTypeDTO processTypeDTO);
      //  ResultDTO DeleteProcessType(int id);
        ResultDTO GetProcessType();
    }
}
