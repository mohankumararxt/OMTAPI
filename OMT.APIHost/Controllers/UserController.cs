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
    [Authorize]
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
    }
}
