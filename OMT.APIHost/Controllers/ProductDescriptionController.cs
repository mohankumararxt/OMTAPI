using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OMT.DataService.Interface;
using OMT.DTO;

namespace OMT.APIHost.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class ProductDescriptionController : ControllerBase
    {
        private readonly IProductDescriptionService _productDescriptionService;

        public ProductDescriptionController(IProductDescriptionService productDescriptionService)
        {
            _productDescriptionService = productDescriptionService;
        }

        [HttpGet]
        [Route("GetProductDescription")]
        public ResultDTO GetProductDescription()
        {
            return _productDescriptionService.GetProductDescription();
        }

        [HttpDelete]
        [Route("DeleteProductDescription/{id:int}")]
        public ResultDTO DeleteProductDescription(int id)
        {
            return _productDescriptionService.DeleteProductDescription(id);
        }

        [HttpPost]
        [Route("CreateProductDescription")]
        public ResultDTO CreateProductDescription([FromBody] string ProductDescriptionName)
        {
            return _productDescriptionService.CreateProductDescription(ProductDescriptionName);
        }

        [HttpPut]
        [Route("UpdateProductDescription")]
        public ResultDTO UpdateProductDescription([FromBody] ProductDescriptionResponseDTO productDescriptionResponseDTO)
        {
            return _productDescriptionService.UpdateProductDescription(productDescriptionResponseDTO);
        }

    }
}
