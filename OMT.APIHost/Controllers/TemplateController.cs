using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OMT.DataService.Interface;
using OMT.DTO;
using System.Text.Json.Nodes;

namespace OMT.APIHost.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TemplateController : BaseController
    {
        private readonly ITemplateService _templateService;
        public TemplateController(ITemplateService templateService)
        {
            _templateService = templateService;
        }


        [HttpPost]
        [Route("new")]
        public ResultDTO CreateTemplate([FromBody] CreateTemplateDTO createTemplateDTO)
        {
            ResultDTO resultDTO = _templateService.CreateTemplate(createTemplateDTO);
            return resultDTO;
        }

        [HttpDelete]
        [Route("delete/{skillsetid:int}")]
        public ResultDTO DeleteTemplate(int skillsetid)
        {
            return _templateService.DeleteTemplate(skillsetid);
        }

        [HttpPost]
        [Route("uploadOrders")]
        public ResultDTO UploadOrders([FromBody] UploadTemplateDTO uploadTemplateDTO)
        {
            return _templateService.UploadOrders(uploadTemplateDTO);
        }

        [HttpPost]
        [Route("validateOrders")]
        public ResultDTO ValidateOrders([FromBody] ValidateOrderDTO validateorderDTO)
        {
            return _templateService.ValidateOrders(validateorderDTO);
        }

        [HttpGet]
        [Route("getOrders")]
        public ResultDTO GetOrders()
        {
            var userid = UserId;
            return _templateService.GetOrders(userid);
        }

        [HttpGet]
        [Route("list")]
        public ResultDTO GetTemplateList()
        {
            return _templateService.GetTemplateList();
        }

        [HttpPut]
        [Route("updateOrderStatus")]
        public ResultDTO UpdateOrderStatus([FromBody] UpdateOrderStatusDTO updateOrderStatusDTO)
        {
            //var userid = UserId;
            return _templateService.UpdateOrderStatus(updateOrderStatusDTO);
        }

        [HttpPost]
        [Route("GetAgentCompletedOrders")]
        public ResultDTO AgentCompletedOrders([FromBody] AgentCompletedOrdersDTO agentCompletedOrdersDTO)
        {
            return _templateService.AgentCompletedOrders(agentCompletedOrdersDTO);
        }

        [HttpPost]
        [Route("GetTeamCompletedOrders")]
        public ResultDTO TeamCompletedOrders([FromBody] TeamCompletedOrdersDTO teamCompletedOrdersDTO)
        {
            return _templateService.TeamCompletedOrders(teamCompletedOrdersDTO);
        }

        [HttpGet]
        [Route("GetDefaultColumnNames/{systemofrecordid:int}")]
        public ResultDTO GetDefaultColumnNames(int systemofrecordid)
        {
            return _templateService.GetDefaultColumnNames(systemofrecordid);
        }

    }
}
