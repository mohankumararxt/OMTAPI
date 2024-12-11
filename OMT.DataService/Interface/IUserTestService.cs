﻿using OMT.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DataService.Interface
{
    public interface IUserTestService
    {
        ResultDTO CreateAndAssignTest(CreateTestAndAssignDTO createTestAndAssign);
        ResultDTO UpdateUserTestStartTime(UpdateUserTestTimeDTO updateUserTestTime);
        ResultDTO UpdateUserTest(UpdateUserTestsDTO updateUserTests);

        ResultDTO GetUserTestLeaderboard(int numberofdays);
        ResultDTO Test(int userId);

        ResultDTO GetTotalOpenCount();

        ResultDTO AgentProgressBar(AgentProgressBarRequestDTO agentProgressBarRequestDTO);

         ResultDTO GetCountOfTestBasedOnStatus();
        ResultDTO GetAgentStatusCount(int userId);
    }
}
