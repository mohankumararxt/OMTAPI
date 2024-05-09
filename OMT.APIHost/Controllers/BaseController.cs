using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace OMT.APIHost.Controllers
{
    public class BaseController : ControllerBase
    {
        public int UserId
        {
            get
            {
                var identity = (ClaimsPrincipal)ControllerContext.HttpContext.User;
                return Convert.ToInt32(identity.Claims.FirstOrDefault(c => c.Type == "UserId").Value);
            }
        }
    }
}
