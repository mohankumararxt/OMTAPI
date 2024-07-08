using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OMT.DataService.Interface;
using OMT.DTO;

namespace OMT.APIHost.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BusinessGroupController : ControllerBase
    {
        private readonly IBusinessGroupService _businessGroupService;

        public BusinessGroupController(IBusinessGroupService businessGroupService)
        {
            _businessGroupService = businessGroupService;
        }

        [HttpGet]
        [Route("Get")]
        public ResultDTO GetBusinessGroup()
        {
            return _businessGroupService.GetBusinessGroup();
        }

        [HttpDelete]
        [Route("Delete/{id:int}")]
        public ResultDTO DeleteBusinessGroup(int id)
        {
            return _businessGroupService.DeleteBusinessGroup(id);
        }

        [HttpPost]
        [Route("new")]
        public ResultDTO CreateBusinessGroup([FromBody] string BusinessGroupName)
        {
            return _businessGroupService.CreateBusinessGroup(BusinessGroupName);
        }

        [HttpPut]
        [Route("update")]
        public ResultDTO UpdateBusinessGroup(BusinessGroupDTO businessGroupDTO)
        {
            return _businessGroupService.UpdateBusinessGroup(businessGroupDTO);
        }

    }
}
