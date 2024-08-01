using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OMT.DataService.Interface;
using OMT.DTO;

namespace OMT.APIHost.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BusinessController : ControllerBase
    {
        private readonly IBusinessService _businessService;

        public BusinessController(IBusinessService businessService)
        {
            _businessService = businessService;
        }

        [HttpPost]
        [Route("new")]
        public ResultDTO CreateBusines([FromBody] string businessname)
        {
            return _businessService.CreateBusiness(businessname);
        }

        [HttpPut]
        [Route("update")]
        public ResultDTO UpdateBusines([FromBody] BusinessDTO businessDTO)
        {
            return _businessService.UpdateBusiness(businessDTO); 
        }

        [HttpGet]
        [Route("get")]
        public ResultDTO GetBusines()
        {
            return _businessService.GetBusiness();
    }
    }
}
