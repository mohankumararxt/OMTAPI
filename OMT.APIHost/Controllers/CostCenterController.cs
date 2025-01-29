using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OMT.DataService.Interface;
using OMT.DataService.Service;
using OMT.DTO;

namespace OMT.APIHost.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class CostCenterController : ControllerBase
    {
        private readonly ICostCenterService _costCenterService;
        public CostCenterController(ICostCenterService costCenterService)
        {
            _costCenterService = costCenterService;
        }

        [HttpGet]
        [Route("GetCostCenterList")]

        public ResultDTO GetCostCenterList()
        {
            return _costCenterService.GetCostCenterList();
        }

        //[HttpDelete]
        //[Route("DeleteCostCenter/{costcenterid:int}")]

        //public ResultDTO DeleteCostCenter(int costcenterid)
        //{
        //    return _costCenterService.DeleteCostCenter(costcenterid);
        //}

        [HttpPost]
        [Route("CreateCostCenter")]
        public ResultDTO CreateCostCenter([FromBody] string CostcenterAmount)
        {
            return _costCenterService.CreateCostCenter(CostcenterAmount);
        }

        [HttpPut]
        [Route("UpdateCostCenter")]

        public ResultDTO UpdateCostCenter([FromBody] UpdateCostCenterDTO updateCostCenterDTO)
        {
            return _costCenterService.UpdateCostCenter(updateCostCenterDTO);
        }
    }
}
