using OMT.DTO;

namespace OMT.DataService.Interface
{
    public interface IResWareProductDescriptionsService
    {
        ResultDTO GetResWareProductDescriptions();
        ResultDTO GetResWareProductDescriptionsMap(int? skillsetId);
        ResultDTO CreateResWareProductDescriptionsMap(ResWareProductDescriptionsDTO resWareProductDescriptionsDTO);
        ResultDTO UpdateResWareProductDescriptions(ResWareProductDescriptionsUpdateDTO res);
        ResultDTO DeleteResWareProductDescriptions(int rprodid);
        ResultDTO CreateOnlyResWareProductDescriptions(ResWareProductDescriptionOnlyDTO resWareProductDescriptionOnlyDTO);
       
    }
}
