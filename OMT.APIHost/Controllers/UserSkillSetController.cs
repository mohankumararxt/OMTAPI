﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OMT.DataService.Interface;
using OMT.DTO;

namespace OMT.APIHost.Controllers
{
    /// <summary>
    /// UserSkillSetController handles api's for user skill sets
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserSkillSetController : BaseController  //inherited basecontroller
    {
        private readonly IUserSkillSetService _userSkillSetService;
        public UserSkillSetController(IUserSkillSetService userSkillSetService)
        {
            _userSkillSetService = userSkillSetService;
        }

        /// <summary>
        /// Get list of all skill sets of the user
        /// </summary>
        /// <returns>Return user skill sets</returns>
        [HttpGet]
        [Route("list")]
        public ResultDTO GetUserSkillSetList()
        {
            var userid = UserId;
            return _userSkillSetService.GetUserSkillSetList(userid);
        }

        /// <summary>
        /// Add skill set for user
        /// </summary>
        /// <param name="skillsetid"></param>
        /// <returns>Return success message after addition of user skill set</returns>
        [HttpPost]
        [Route("new")]
        public ResultDTO AddUserSkillSet([FromBody] int skillsetid)
        {
            var userid = UserId;
            ResultDTO resultDTO = _userSkillSetService.AddUserSkillSet(skillsetid, userid);
            return resultDTO;
        }

        /// <summary>
        /// Delete skill set of the user
        /// </summary>
        /// <param name="userskillsetId"></param>
        /// <returns>Return success message after deletion of user skill set</returns>
        [HttpDelete]
        [Route("delete/{userskillsetId:int}")]
        public ResultDTO DeleteUserSkillSet(int userskillsetId)
        {
            ResultDTO resultDTO = _userSkillSetService.DeleteUserSkillSet(userskillsetId);
            return resultDTO;
        }

        /// <summary>
        /// Update skill set of user
        /// </summary>
        /// <param name="userskillSetResponseDTO"></param>
        /// <returns>Return updated user skill set</returns>
        [HttpPut]
        [Route("update")]
        public ResultDTO UpdateUserSkillSet([FromBody]UserSkillSetResponseDTO userskillSetResponseDTO)
        {
            ResultDTO resultDTO = _userSkillSetService.UpdateUserSkillSet(userskillSetResponseDTO);
            return resultDTO;
        }
    }
}
