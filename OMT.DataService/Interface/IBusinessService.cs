using OMT.DTO;

namespace OMT.DataService.Interface
{
    public interface IBusinessService
    {
        ResultDTO CreateBusiness(string businessname);
        ResultDTO UpdateBusiness(BusinessDTO businessDTO);  
        ResultDTO GetBusiness();
    }
}
