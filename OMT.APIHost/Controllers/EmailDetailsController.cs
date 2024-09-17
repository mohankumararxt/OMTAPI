using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OMT.DataService.Interface;
using OMT.DTO;

namespace OMT.APIHost.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmailDetailsController : ControllerBase
    {
        private readonly IEmailDetailsService _emailDetailsService;

        public EmailDetailsController(IEmailDetailsService emailDetailsService)
        {
            _emailDetailsService = emailDetailsService;
        }

        [HttpPost]
        [Route("SendEmail")]

        public ResultDTO SendMail(SendEmailDTO sendEmailDTO)
        {
            return _emailDetailsService.SendMail(sendEmailDTO);
        }
    }
}
