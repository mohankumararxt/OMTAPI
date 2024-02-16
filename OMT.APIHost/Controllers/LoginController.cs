using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OMT.Authorization;
using OMT.DataService.Interface;
using OMT.DataService.Service;
using OMT.DTO;

namespace OMT.APIHost.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly ILoginService _loginService;
        private readonly IOptions<JwtAuthSettings> _authSettings;
        public LoginController(ILoginService loginService, IOptions<JwtAuthSettings> authSettings)
        {
            _loginService = loginService;
            _authSettings = authSettings;
        }

        /// <summary>
        /// Login with OMT Application using credentials
        /// </summary>
        /// <param name="loginRequestDTO">Login Credentials</param>
        /// <returns>Return user details with valid token</returns>
        [HttpPost]
        public ResultDTO Login([FromBody] LoginRequestDTO loginRequestDTO)
        {
            ResultDTO result = _loginService.Login(loginRequestDTO);
            if (result.IsSuccess)
            {
                Dictionary<string, string> claims = new Dictionary<string, string>();

                claims.Add("Email", loginRequestDTO.Email);
                claims.Add("UserId", Convert.ToString(result.Data.UserId));
                claims.Add("OrganizationId", Convert.ToString(result.Data.OrganizationId));
                claims.Add("RoleId", Convert.ToString(result.Data.RoleId));
                var token = new JwtTokenBuilder(_authSettings).AddClaims(claims).Build();
                result.Data.Token = token.Value;
                result.Data.TokenExpiry = token.ValidTo;

            }
            return result;
        }
    }
}
