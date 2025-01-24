using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OMT.Authorization;
using OMT.DataService.Interface;
using OMT.DTO;

namespace OMT.APIHost.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class UserController : BaseController
    {
        private readonly IUserService _userService;
        private readonly IOptions<JwtAuthSettings> _authSettings;
        public UserController(IUserService userService, IOptions<JwtAuthSettings> authSettings)
        {
            _userService = userService;
            _authSettings = authSettings;
        }

        [HttpPost]
        [Route("new")]

        public ResultDTO CreateUser([FromBody] CreateUserDTO createUserDTO)
        {
            //int userid = this.UserId;
            ResultDTO result = _userService.CreateUser(createUserDTO);
            return result;
        }

        [HttpGet]
        [Route("list")]
        public ResultDTO GetUserList()
        {
            return _userService.GetUserList();
        }

        [HttpDelete]
        [Route("delete/{userid:int}")]

        public ResultDTO DeleteUser(int userid)
        {
            return _userService.DeleteUser(userid);
        }

        [HttpPut]
        [Route("UpdateByHR")]
        public ResultDTO UpdateByHR([FromBody] UpdateUserByHrDTO updateUserDTO)
        {
            return _userService.UpdateByHR(updateUserDTO);
        }

        [HttpGet]
        [Route("list/{userid:int}")]
        public ResultDTO GetuserInfoByUserId(int userid)
        {
            return _userService.GetuserInfoByUserId(userid);

        }

        [HttpPut]
        [Route("UpdatePasswordByUser")]
        public ResultDTO UpdatePasswordByUser([FromBody] UpdatePasswordByUserDTO updatePasswordByUserDTO)
            
        {
            return _userService.UpdatePasswordByUser(updatePasswordByUserDTO);

        }   

        [HttpPut]
        [Route("UpdatePasswordByHR")]
        public ResultDTO UpdatePasswordByHR([FromBody] UpdateUserPasswordByHrDTO updateUserPasswordDTO)
        {
            return _userService.UpdatePasswordByHR(updateUserPasswordDTO);
        }

        [HttpGet]
        [Route("SocketIoUserList")]
        [Authorize(AuthenticationSchemes = "BasicAuthentication")]
        public ResultDTO GetSocketIoUserList()
        {
            return _userService.GetSocketIoUserList();
        }

    }
}
