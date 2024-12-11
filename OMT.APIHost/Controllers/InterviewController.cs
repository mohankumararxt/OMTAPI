using Microsoft.AspNetCore.Mvc;
using OMT.DataAccess.Entities;
using OMT.DataService.Interface;
using OMT.DTO;
using System.Xml.Linq;

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
        public ResultDTO SaveInterviewee(UserInterviewsDTO userInterviewsDTO)
        {
            return _interviewService.SaveInterviewee(userInterviewsDTO);
        }


        [HttpPut]
        [Route("UpdateInterviewStartTime")]
        public ResultDTO UpdateInterviewStartTime(UpdateInterviewTestTimeDTO updateInterviewTestTime)
        {
            return _interviewService.UpdateInterviewStartTime(updateInterviewTestTime);
        }

        [HttpPut]
        [Route("UpdateInterviewTests")]
        public ResultDTO UpdateInterviewTests(UpdateInterviewTestsDTO updateInterviewTestsDTO)
        {
            return _interviewService.UpdateInterviewTests(updateInterviewTestsDTO);
        }

        [HttpGet]
        [Route("IntervieweesLeaderboard")]
        public ResultDTO GetIntervieweesLeaderboard(int numberofdays)
        {
            return _interviewService.GetIntervieweesLeaderboard(numberofdays);
        }


    }
}
