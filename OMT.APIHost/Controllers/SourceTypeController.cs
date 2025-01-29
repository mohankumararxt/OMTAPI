using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OMT.DataService.Interface;
using OMT.DTO;

namespace OMT.APIHost.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class SourceTypeController : ControllerBase
    {
        private readonly ISourceTypeService _sourceTypeService;

        public SourceTypeController(ISourceTypeService sourceTypeService)
        {
            _sourceTypeService = sourceTypeService;
        }

        [HttpPost]
        [Route("new")]
        public ResultDTO CreateSourceType([FromBody] string SourceTypeName)
        {
            return _sourceTypeService.CreateSourceType(SourceTypeName);
        }

        [HttpGet]
        [Route("get")]
        public ResultDTO GetSourceType() 
        {
            return _sourceTypeService.GetSourceType();
        }

        //[HttpDelete]
        //[Route("delete/{stid:int}")]
        //public ResultDTO DeleteSourceType(int stid) 
        //{
        //    return _sourceTypeService.DeleteSourceType(stid);
        //}

        [HttpPut]
        [Route("update")]
        public ResultDTO UpdateSourceType([FromBody] SourceTypeDTO sourceTypeDTO)
        {
            return _sourceTypeService.UpdateSourceType(sourceTypeDTO);
        }

    }
}
