﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OMT.DataService.Interface;
using OMT.DTO;

namespace OMT.APIHost.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class ResWareProductDescriptionsController : ControllerBase
    {
        private readonly IResWareProductDescriptionsService _resWareProductDescriptionsService;
        public ResWareProductDescriptionsController(IResWareProductDescriptionsService resWareProductDescriptionsService)
        {
            _resWareProductDescriptionsService = resWareProductDescriptionsService;
        }

        [HttpGet]
        [Route("GetResWareProductDescriptionsList")]
        public ResultDTO GetResWareProductDescriptions()
        {
            return _resWareProductDescriptionsService.GetResWareProductDescriptions();
        }

        [HttpGet]
        [Route("GetResWareProductDescriptionsMapList/{skillsetId:int?}")]
        public ResultDTO GetResWareProductDescriptionsMap(int? skillsetId)
        {
            return _resWareProductDescriptionsService.GetResWareProductDescriptionsMap(skillsetId);
        }

        [HttpPost]
        [Route("CreateResWareProductDescriptionsMap")]
        public ResultDTO CreateResWareProductDescriptionsMap([FromBody] ResWareProductDescriptionsDTO resWareProductDescriptionsDTO)
        {
            return _resWareProductDescriptionsService.CreateResWareProductDescriptionsMap(resWareProductDescriptionsDTO);
        }

        [HttpDelete]
        [Route("DeleteResWareProductDescriptions/{rprodid:int}")]

        public ResultDTO DeleteResWareProductDescriptions(int rprodid)
        {
            return _resWareProductDescriptionsService.DeleteResWareProductDescriptions(rprodid);
        }

        [HttpPut]
        [Route("UpdateResWareProductDescriptions")]
        public ResultDTO UpdateResWareProductDescriptions([FromBody] ResWareProductDescriptionsUpdateDTO res)
        {
            return _resWareProductDescriptionsService.UpdateResWareProductDescriptions(res);
        }

        [HttpPost]
        [Route("CreateOnlyResWareProductDescriptions")]
        public ResultDTO CreateOnlyResWareProductDescriptions([FromBody] ResWareProductDescriptionOnlyDTO resWareProductDescriptionOnlyDTO)
        {
            return _resWareProductDescriptionsService.CreateOnlyResWareProductDescriptions(resWareProductDescriptionOnlyDTO);
        }

      
    }
}
