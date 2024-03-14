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
    public class TemplateController : ControllerBase
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
        [Route("upload")]
        public ResultDTO UploadData([FromBody] UploadTemplateDTO uploadTemplateDTO)
        {
            return _templateService.UploadData(uploadTemplateDTO);
        }

        [HttpPost]
        [Route("validate")]
        public ResultDTO ValidateData(UploadTemplateDTO uploadTemplateDTO)
        {
            return _templateService.ValidateData(uploadTemplateDTO);
        }

    }
}
