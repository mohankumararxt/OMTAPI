using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OMT.DataService.Interface;
using OMT.DTO;

namespace OMT.APIHost.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProcessTypeController : ControllerBase
    {
        private readonly IProcessTypeService _processTypeService;

        public ProcessTypeController(IProcessTypeService processTypeService)
        {
            _processTypeService = processTypeService;
        }

        [HttpGet]
        [Route("get")]
        public ResultDTO GetProcessType()
        {
            return _processTypeService.GetProcessType();
        }

        [HttpPut]
        [Route("update")]
        public ResultDTO UpdateProcessType([FromBody] ProcessTypeDTO processTypeDTO)
        {
            return _processTypeService.UpdateProcessType(processTypeDTO);
        }

        //[HttpDelete]
        //[Route("delete/{id:int}")]
        //public ResultDTO DeleteProcessType(int id)
        //{
        //    return _processTypeService.DeleteProcessType(id);
        //}

        [HttpPost]
        [Route("new")]
        public ResultDTO CreateProcessType([FromBody] string ProcessType)
        {
            return _processTypeService.CreateProcessType(ProcessType);
        }
    }
}
