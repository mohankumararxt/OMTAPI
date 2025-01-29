using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OMT.DataService.Interface;
using OMT.DTO;
using System.Reflection.Metadata.Ecma335;

namespace OMT.APIHost.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer")]
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
            var userid = UserId;
            return _templateService.UploadOrders(uploadTemplateDTO,userid);
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

        [HttpGet]
        [Route("GetPendingOrderDetails")]
        public ResultDTO GetPendingOrderDetails()
        {
            var userid = UserId;

            return _templateService.GetPendingOrderDetails(userid);
        }

        [HttpPost]
        [Route("GetComplexOrdersDetails")]

        public ResultDTO GetComplexOrdersDetails([FromBody] ComplexOrdersRequestDTO complexOrdersRequestDTO)
        {
            return _templateService.GetComplexOrdersDetails(complexOrdersRequestDTO);
        }

        [HttpPut]
        [Route("ReleaseOrder")]

        public ResultDTO ReleaseOrder(ReleaseOrderDTO releaseOrderDTO)
        {
            return _templateService.ReleaseOrder(releaseOrderDTO);
        }

        [HttpPost]
        [Route("TimeExceededOrders")]
        public ResultDTO TimeExceededOrders([FromBody] TimeExceededOrdersDTO timeExceededOrdersDTO)
        {
            return _templateService.TimeExceededOrders(timeExceededOrdersDTO);
        }

        [HttpGet]
        [Route("GetTemplateColumnNames/{skillsetid:int}")]
        public ResultDTO GetTemplateColumnNames(int skillsetid)
        {
            return _templateService.GetTemplateColumns(skillsetid);
        }

        [HttpPost]
        [Route("ReplaceOrders")]
        public ResultDTO ReplaceOrders([FromBody] ReplaceOrdersDTO replaceOrdersDTO)
        {
            return _templateService.ReplaceOrders(replaceOrdersDTO);
        }

        [HttpPut]
        [Route("RejectOrder")]
        public ResultDTO RejectOrder([FromBody] RejectOrderDTO rejectOrderDTO)
        {
            return _templateService.RejectOrder(rejectOrderDTO);
        }

        [HttpPut]
        [Route("AssignOrderToUser")]
        public ResultDTO AssignOrderToUser([FromBody] AssignOrderToUserDTO assignOrderToUserDTO)
        {
            return _templateService.AssignOrderToUser(assignOrderToUserDTO);
        }

        [HttpPost]
        [Route("DeleteOrders")]
        public ResultDTO DeleteOrders([FromBody] DeleteOrderDTO deleteOrderDTO)
        {
            var userid = UserId;
            return _templateService.DeleteOrders(deleteOrderDTO,userid);
        }

        [HttpPost]
        [Route("SkillsetWiseReports")]

        public ResultDTO SkillsetWiseReports([FromBody] SkillsetWiseReportsDTO skillsetWiseReportsDTO)
        {
            return _templateService.SkillsetWiseReports(skillsetWiseReportsDTO);
        }

        [HttpGet]
        [Route("GetMandatoryColumnNames/{skillsetid:int}")]
        public ResultDTO GetMandatoryColumnNames(int skillsetid)
        {
            return _templateService.GetMandatoryColumnNames(skillsetid);
        }

        [HttpGet]
        [Route("gettrdpendingorders")]
        public ResultDTO GetTrdPendingOrders()
        {
            var userid = UserId;
            return _templateService.GetTrdPendingOrders(userid);
        }

    }
}
