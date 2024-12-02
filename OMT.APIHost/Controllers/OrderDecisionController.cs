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
    [Authorize]
    public class OrderDecisionController : BaseController
    {
        private readonly IOrderDecisionService _orderdecisionservice;

        public OrderDecisionController(IOrderDecisionService orderdecisionservice)
        {
            _orderdecisionservice = orderdecisionservice;
        }

        [HttpGet]
        [Route("GetOrderForUser")]
        public ResultDTO GetOrderForUser()
        {
            var userid = UserId;
            return _orderdecisionservice.GetOrderForUser(userid);
        }

        [HttpGet]
        [Route("GetTrdPendingOrderForUser")]
        public ResultDTO GetTrdPendingOrderForUser()
        {
            var userid = UserId;
            return _orderdecisionservice.GetTrdPendingOrderForUser(userid);
        }

        [HttpPost]
        [Route("GetOrderInfo")]
        public ResultDTO GetOrderInfo([FromBody] OrderInfoDTO orderInfoDTO)
        {
            return _orderdecisionservice.GetOrderInfo(orderInfoDTO);
        }

        [HttpPut]
        [Route("UpdateOrderStatusByTL")]
        public ResultDTO UpdateOrderStatusByTL([FromBody] UpdateOrderStatusByTLDTO updateOrderStatusByTLDTO)
        {
            var userid = UserId;
            return _orderdecisionservice.UpdateOrderStatusByTL(userid, updateOrderStatusByTLDTO);
        }

        [HttpPost]
        [Route("GetUnassignedOrderInfo")]
        public ResultDTO GetUnassignedOrderInfo([FromBody] UnassignedOrderInfoDTO unassignedOrderInfoDTO)
        {
            return _orderdecisionservice.GetUnassignedOrderInfo(unassignedOrderInfoDTO);
        }

        [HttpPut]
        [Route("UpdateUnassignedOrder")]
        public ResultDTO UpdateUnassignedOrder([FromBody] UpdateUnassignedOrderDTO updateUnassignedOrderDTO)
        {
            var userid = UserId;
            return _orderdecisionservice.UpdateUnassignedOrder(userid,updateUnassignedOrderDTO);
        }
    }
}