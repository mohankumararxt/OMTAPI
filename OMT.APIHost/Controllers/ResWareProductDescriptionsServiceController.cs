using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OMT.DataService.Interface;
using OMT.DTO;

namespace OMT.APIHost.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ResWareProductDescriptionsServiceController : ControllerBase
    {
        private readonly IResWareProductDescriptionsService _resWareProductDescriptionsService;

        public ResWareProductDescriptionsServiceController(IResWareProductDescriptionsService resWareProductDescriptionsService)
        {
            _resWareProductDescriptionsService = resWareProductDescriptionsService;
        }

        [HttpGet]
        [Route("list")]
        public ResultDTO GetResWareProductDescriptions()
        {
            return _resWareProductDescriptionsService.GetResWareProductDescriptions();
        }

        //[HttpGet]
        //[Route("list/{productId:int}")]
        //public ResultDTO GetResWareProductDescriptionsMap(int productId)
        //{
        //    return _resWareProductDescriptionsService.GetResWareProductDescriptionsMap(productId);
        //}

        //[HttpPost]
        //[Route("new")]

        //public ResultDTO CreateResWareProductDescriptions(ResWareProductDescriptionsDTO resWareProductDescriptionsDTO)
        //{
        //    return _resWareProductDescriptionsService.CreateResWareProductDescriptions(resWareProductDescriptionsDTO);
        //}

        //[HttpDelete]
        //[Route("delete/{rproid:int}")]

        //public ResultDTO DeleteResWareProductDescriptions(int rprodid)
        //{
        //    return _resWareProductDescriptionsService.DeleteResWareProductDescriptions(rprodid);
        //}

        //[HttpPut]
        //[Route("update/{rproid:int}")]

        //public ResultDTO UpdateResWareProductDescriptions()
        //{
        //    return _resWareProductDescriptionsService.UpdateResWareProductDescriptions();
        //}
    }
}
