﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OMT.DataService.Interface;
using OMT.DTO;

namespace OMT.APIHost.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserTestController : ControllerBase
    {
        private readonly IUserTestService _userTestService;
        public UserTestController(IUserTestService userTestService)
        {
            _userTestService = userTestService;
        }
        [HttpPost]
        [Route("CreateAndAssignTest")]
        public ResultDTO CreateAndAssignTest(CreateTestAndAssignDTO createTestAndAssign)
        {
            return _userTestService.CreateAndAssignTest(createTestAndAssign);
        }


        [HttpPut]
        [Route("UpdateUserTestStartTime")]
        public ResultDTO UpdateUserTestStartTime(UpdateUserTestTimeDTO updateUserTestTime)
        {
            return _userTestService.UpdateUserTestStartTime(updateUserTestTime);
        }
        [HttpPut]
        [Route("UpdateUserTest")]
        public ResultDTO UpdateUserTest(UpdateUserTestsDTO updateUserTests)
        {
            return _userTestService.UpdateUserTest(updateUserTests);
        }

        [HttpGet]
        [Route("UserTestLeaderboard")]
        public ResultDTO GetUserTestLeaderboard(int numberofdays)
        {
            return _userTestService.GetUserTestLeaderboard(numberofdays);   
        }

        [HttpGet]
        [Route("Test")]
        public ResultDTO Test(int userId)
        {
            return _userTestService.Test(userId);
        }
    }
}