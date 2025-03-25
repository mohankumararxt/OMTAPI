using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OMT.DataService.Interface;
using OMT.DataService.Service;
using OMT.DTO;

namespace OMT.APIHost.Controllers
{
    /// <summary>
    /// UserSkillSetController handles api's for user skill sets
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
   // [Authorize(AuthenticationSchemes = "Bearer")]
    public class UserSkillSetController : BaseController  //inherited basecontroller
    {
        private readonly IUserSkillSetService _userSkillSetService;
        public UserSkillSetController(IUserSkillSetService userSkillSetService)
        {
            _userSkillSetService = userSkillSetService;
        }

        /// <summary>
        /// Get list of skill sets of users
        /// </summary>
        /// <returns>Returns list of user's skill sets</returns>
        [HttpGet]
        [Route("list/{userid:int?}")]
        public ResultDTO GetUserSkillSetList(int? userid)
        {
            return _userSkillSetService.GetUserSkillSetList(userid);
        }

        /// <summary>
        /// Add skill set for user
        /// </summary>
        ///<param name="userSkillSetCreate">UserSkillSetCreateDTO</param>
        /// <returns>Returns success message after addition of user skill set</returns>
        [HttpPost]
        [Route("new")]
        public ResultDTO AddUserSkillSet([FromBody] UserSkillSetCreateDTO userSkillSetCreateDTO)
        {
            ResultDTO resultDTO = _userSkillSetService.AddUserSkillSet(userSkillSetCreateDTO);
            return resultDTO;
        }

        /// <summary>
        /// Delete skill set of the user
        /// </summary>
        /// <param name="userskillsetId">UserSkillSet Id</param>
        /// <returns>Returns success message after deletion of user skill set</returns>
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
        /// <param name="userskillSetResponseDTO">UserSkillSetResponseDTO</param>
        /// <returns>Returns updated user skill set</returns>
        [HttpPut]
        [Route("update")]
        public ResultDTO UpdateUserSkillSet([FromBody] UserSkillSetUpdateDTO userSkillSetUpdateDTO)
        {
            ResultDTO resultDTO = _userSkillSetService.UpdateUserSkillSet(userSkillSetUpdateDTO);
            return resultDTO;
        }

        /// <summary>
        /// Get list of users and their skillsets for the selected teamid and sor id
        /// </summary>
        /// <param name="updateUserSkillsetListDTO">UpdateUserSkillsetListDTO</param>
        /// <returns>Returns list of users and their rspective skillset informations</returns>
        [HttpPost]
        [Route("UpdateUserSkillsetList")]
        public ResultDTO UpdateUserSkillsetList([FromBody] UpdateUserSkillsetListDTO updateUserSkillsetListDTO)
        {
            return _userSkillSetService.UpdateUserSkillsetList(updateUserSkillsetListDTO);
        }

        /// <summary>
        /// Bulk update of users skill sets
        /// </summary>
        /// <param name="bulkUserSkillsetUpdateDTO">BulkUserSkillsetUpdateDTO</param>
        /// <returns></returns>
        [HttpPut]
        [Route("BulkUserSkillsetUpdate")]
        public ResultDTO BulkUpdate(BulkUserSkillsetUpdateDTO bulkUserSkillsetUpdateDTO)
        {
            return _userSkillSetService.BulkUpdate(bulkUserSkillsetUpdateDTO);
        }

        [HttpPost]
        [Route("createMultipleUserSkillset")]
        public ResultDTO CreateMultipleUserSkillset([FromBody] MultipleUserSkillSetCreateDTO multipleUserSkillSetCreateDTO)
        {
            ResultDTO resultDTO = _userSkillSetService.CreateMultipleUserSkillset(multipleUserSkillSetCreateDTO);
            return resultDTO;
        }

        [HttpGet]
        [Route("userskillsetlist/{userid:int?}")]
        public ResultDTO ConsolidatedUserSkillSetlist(int? userid)
        {
            return _userSkillSetService.ConsolidatedUserSkillSetlist(userid);
        }

        [HttpPut]
        [Route("UpdateUserskillset")]
        public ResultDTO UpdateUserSkillSetThWt([FromBody] UpdateUserSkillSetThWtDTO updateUserSkillSetThWtDTO)
        {
            ResultDTO resultDTO = _userSkillSetService.UpdateUserSkillSetThWt(updateUserSkillSetThWtDTO);
            return resultDTO;
        }
    }
}