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
    public class MasterReportColumnsController : BaseController  
    {
        private readonly IMasterReportColumnsService _masterReportColumnsService;
        public MasterReportColumnsController(IMasterReportColumnsService masterReportColumnsService)
        {
            _masterReportColumnsService = masterReportColumnsService;
        }

        [HttpGet]
        [Route("list")]
        public ResultDTO GetMasterReportColumnList()
        {
            return _masterReportColumnsService.GetMasterReportColumnList();
        }

        [HttpPost]
        [Route("CreateReportColumnNames")]
        public ResultDTO CreateReportColumnNames([FromBody] MasterReportColumnDTO masterReportColumnDTO)
        {
            ResultDTO resultDTO = _masterReportColumnsService.CreateReportColumnNames(masterReportColumnDTO);
            return resultDTO;
        }
    }
}