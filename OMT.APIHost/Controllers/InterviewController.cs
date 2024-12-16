using Microsoft.AspNetCore.Mvc;
using OMT.DataAccess.Entities;
using OMT.DataService.Interface;
using OMT.DTO;
using System.Xml.Linq;
using Microsoft.AspNetCore.Authorization;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace OMT.APIHost.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    
    public class InterviewController : ControllerBase
    {
        private readonly IInterviewService _interviewService;
        public InterviewController(IInterviewService interviewService)
        {
            _interviewService = interviewService;
        }



        [HttpPost]
        [Route("SaveIntervieweeUserInformation")]
        [Authorize(AuthenticationSchemes = "BasicAuthentication")]
        public ResultDTO SaveInterviewee(UserInterviewsDTO userInterviewsDTO)
        {
            return _interviewService.SaveInterviewee(userInterviewsDTO);
        }


        [HttpPut]
        [Route("UpdateInterviewStartTime")]
        [Authorize(AuthenticationSchemes = "BasicAuthentication")]
        public ResultDTO UpdateInterviewStartTime(UpdateInterviewTestTimeDTO updateInterviewTestTime)
        {
            return _interviewService.UpdateInterviewStartTime(updateInterviewTestTime);
        }

        [HttpPut]
        [Route("UpdateInterviewTests")]
        [Authorize(AuthenticationSchemes = "BasicAuthentication")]
        public ResultDTO UpdateInterviewTests(UpdateInterviewTestsDTO updateInterviewTestsDTO)
        {
            return _interviewService.UpdateInterviewTests(updateInterviewTestsDTO);
        }

        [HttpGet]
        [Route("IntervieweesLeaderboard")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public ResultDTO GetIntervieweesLeaderboard(int numberofdays)
        {
            return _interviewService.GetIntervieweesLeaderboard(numberofdays);
        }


    }
}
