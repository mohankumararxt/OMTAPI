using OMT.DTO;

namespace OMT.DataService.Interface
{
    public interface IBusinessGroupService
    {
        ResultDTO CreateBusinessGroup(string BusinessGroupName);
        ResultDTO UpdateBusinessGroup(BusinessGroupDTO businessGroupDTO);
        ResultDTO DeleteBusinessGroup(int id);
        ResultDTO GetBusinessGroup();
    }
}
