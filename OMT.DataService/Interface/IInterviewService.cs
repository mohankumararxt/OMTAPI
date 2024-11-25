using OMT.DataAccess.Entities;
using OMT.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DataService.Interface
{
    public interface IInterviewService
    {
        //ResultDTO GetTypingUserList();
        ResultDTO SaveInterviewee(UserInterviewsDTO userInterviewsDTO);

        ResultDTO UpdateInterviewStartTime(UpdateInterviewTestTimeDTO updateInterviewTestTime);

        ResultDTO UpdateInterviewTests(UpdateInterviewTestsDTO updateInterviewTests);
        ResultDTO GetIntervieweesLeaderboard(int numberofdays);

        //ResultDTO getLeaderboardByUser(string username);
    }
}
