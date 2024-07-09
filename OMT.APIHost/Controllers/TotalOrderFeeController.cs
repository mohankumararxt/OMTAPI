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
    //[Authorize]
    public class TotalOrderFeeController : ControllerBase
    {
        private readonly ITotalOrderFeesService _totalOrderFeesService;
        public TotalOrderFeeController(ITotalOrderFeesService totalOrderFeesService)
        {
            _totalOrderFeesService = totalOrderFeesService;
        }

        [HttpGet]
        [Route("GetTotalOrderFeeList")]

        public ResultDTO GetTotalOrderFeeList()
        {
            return _totalOrderFeesService.GetTotalOrderFeeList();
        }

        //[HttpDelete]
        //[Route("DeleteTotalOrderFee/{orderFeeid:int}")]

        //public ResultDTO DeleteTotalOrderFee(int orderFeeid)
        //{
        //    return _totalOrderFeesService.DeleteTotalOrderFee(orderFeeid);
        //}

        [HttpPost]
        [Route("CreateTotalOrderFee")]
        public ResultDTO CreateTotalOrderFee([FromBody] string TotalOrderfeeAmount)
        {
            return _totalOrderFeesService.CreateTotalOrderFee(TotalOrderfeeAmount);
        }

        [HttpPut]
        [Route("UpdateTotalOrderFee")]

        public ResultDTO UpdateTotalOrderFee([FromBody] UpdateTotalOrderFeeDTO updateTotalOrderFeeDTO)
        {
            return _totalOrderFeesService.UpdateTotalOrderFee(updateTotalOrderFeeDTO);
        }
    }
}
