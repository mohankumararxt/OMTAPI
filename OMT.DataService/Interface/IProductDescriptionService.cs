using OMT.DataAccess.Entities;
using OMT.DTO;

namespace OMT.DataService.Interface
{
    public interface IProductDescriptionService
    {
        ResultDTO GetProductDescription();
        ResultDTO DeleteProductDescription(int id);
        ResultDTO CreateProductDescription(string ProductDescriptionName);
        ResultDTO UpdateProductDescription(ProductDescriptionResponseDTO productDescriptionResponseDTO);
    }
}
