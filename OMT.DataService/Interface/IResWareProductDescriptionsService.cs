using OMT.DTO;

namespace OMT.DataService.Interface
{
    public interface IResWareProductDescriptionsService
    {
        ResultDTO GetResWareProductDescriptions();
        ResultDTO GetResWareProductDescriptionsMap(int? productId);
        ResultDTO CreateResWareProductDescriptions(ResWareProductDescriptionsDTO resWareProductDescriptionsDTO);
        ResultDTO UpdateResWareProductDescriptions(ResWareProductDescriptionsUpdateDTO res);
        ResultDTO DeleteResWareProductDescriptions(int rprodid);
    }
}
