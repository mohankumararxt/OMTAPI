using Microsoft.IdentityModel.Tokens;
using OMT.DataAccess.Context;
using OMT.DataAccess.Entities;
using OMT.DataService.Interface;
using OMT.DataService.Utility;
using OMT.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace OMT.DataService.Service
{
    public class UserTestService: IUserTestService
    {
        private readonly OMTDataContext _oMTDataContext;
        public UserTestService(OMTDataContext oMTDataContext)
        {
            _oMTDataContext = oMTDataContext;
        }
        public ResultDTO CreateAndAssignTest(CreateTestAndAssignDTO createTestAndAssign)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };

            try
            {
                var duration = ValidationField.CalculateDuration(createTestAndAssign.text);
                if (!createTestAndAssign.text.IsNullOrEmpty())
                {         

                    //var /*userId*/ = createTestAndAssign.userids;
                    var newtest = new Tests()
                    {
                        Test_text = createTestAndAssign.text,
                        IsSample = false,
                        Duration = (int)duration
                    };
                    
                    _oMTDataContext.Tests.Add(newtest);
                    _oMTDataContext?.SaveChanges();
                    var userTest = new List<UserTest>();

                    // Iterate over userIds and create UserTest objects
                    foreach (var id in createTestAndAssign.userids)
                    {
                        userTest.Add(new UserTest()
                        {
                            UserId = id,          // Assign UserId from the loop
                            TestId = newtest.Id   // Assign the TestId (from newtest object)
                        });
                    }

                    _oMTDataContext?.UserTest.AddRange(userTest); 

                    _oMTDataContext?.SaveChanges();

                    resultDTO.IsSuccess = true;
                    resultDTO.Message = "User Test Assigned Successfully";
                    resultDTO.StatusCode = "201";

                }
                else
                {
                    resultDTO.IsSuccess = false;
                    resultDTO.Message = "Please Create Text";
                    resultDTO.StatusCode = "400";
                }

            }
            catch (Exception ex)
            {
                // Handle exceptions and update the result DTO with error details
                resultDTO.IsSuccess = false;
                resultDTO.StatusCode = "500";
                resultDTO.Message = $"An error occurred: {ex.Message}";
            }

            // Return the result DTO
            return resultDTO;
        }

        public ResultDTO UpdateUserTestStartTime(UpdateUserTestTimeDTO updateUserTestTime)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {

                var existingdata = _oMTDataContext?.UserTest
                        .Where(x => x.Id == updateUserTestTime.Id).FirstOrDefault();
                if (existingdata != null)
                {


                    existingdata.StartTime = updateUserTestTime.datetime;
                    _oMTDataContext.UserTest.Update(existingdata);
                    _oMTDataContext.SaveChanges();
                    resultDTO.IsSuccess = true;
                    resultDTO.Message = "Updated Starttime Successfully";
                    resultDTO.StatusCode = "200";
                }
                else
                {
                    resultDTO.IsSuccess = false;
                    resultDTO.Message = "Id not found";
                    resultDTO.StatusCode = "404";
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions and update the result DTO with error details
                resultDTO.IsSuccess = false;
                resultDTO.StatusCode = "500";
                resultDTO.Message = $"An error occurred: {ex.Message}";
            }
            return resultDTO;
        }


        public ResultDTO UpdateUserTest(UpdateUserTestsDTO updateUserTests)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {

                var existingdata = _oMTDataContext?.UserTest
                        .Where(x => x.Id == updateUserTests.Id).First();
                if (existingdata != null)
                {
                    if (updateUserTests.WPM == null && updateUserTests.Accuracy == null)
                    {
                        resultDTO.IsSuccess = false;
                        resultDTO.Message = "Please fill all required fields";
                        resultDTO.StatusCode = "400";
                    }
                    else
                    {

                        existingdata.WPM = updateUserTests.WPM;
                        existingdata.Accuracy = updateUserTests.Accuracy;
                        existingdata.EndTime = updateUserTests.datetime;

                        _oMTDataContext.UserTest.Update(existingdata);
                        _oMTDataContext.SaveChanges();
                        resultDTO.IsSuccess = true;
                        resultDTO.Message = "Updated InterviewTests Successfully";
                        resultDTO.StatusCode = "200";
                    }
                }
                else
                {
                    resultDTO.IsSuccess = false;
                    resultDTO.Message = "data not found";
                    resultDTO.StatusCode = "404";
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions and update the result DTO with error details
                resultDTO.IsSuccess = false;
                resultDTO.StatusCode = "500";
                resultDTO.Message = $"An error occurred: {ex.Message}";
            }
            return resultDTO;

        }


        public ResultDTO GetUserTestLeaderboard(int numberofdays)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };

            try
            {
                //DateOnly dobDateOnly = DateOnly.FromDateTime(userInterviewsDTO.DOB);
                // Check if TypingUsers is not null to avoid NullReferenceException
                if (numberofdays >= 0 && numberofdays != 0)
                {
                    var result = from itest in _oMTDataContext.UserTest
                                 join test in _oMTDataContext.Tests
                                 on itest.TestId equals test.Id
                                 join user in _oMTDataContext.UserProfile
                                 on itest.UserId equals user.UserId
                                 where itest.CreateTimestamp >= DateTime.UtcNow.AddDays(-numberofdays)
                                 orderby itest.CreateTimestamp descending
                                 select new LeaderboardUserTestDTO()
                                 {
                                     username = user.FirstName + " " + user.LastName,
                                     email = user.Email,
                                     duration = test.Duration,
                                     wpm = itest.WPM,
                                     accuracy = itest.Accuracy != 0 ? Convert.ToDouble(itest.Accuracy) : 0f,
                                     // Handle nullable Double
                                     testdate = DateOnly.FromDateTime(itest.CreateTimestamp)
                                 };


                    // Example: Convert the result to a list
                    var joinedData = result.ToList();


                    resultDTO.IsSuccess = true;
                    resultDTO.Message = "Leaderboard fetch successfully...";
                    resultDTO.Data = joinedData;
                    resultDTO.StatusCode = "200";
                }
                else
                {
                    resultDTO.IsSuccess = false;
                    resultDTO.Message = "Invalid number of days please check again...";
                    resultDTO.StatusCode = "400";
                }



            }
            catch (Exception ex)
            {
                // Handle exceptions and update the result DTO with error details
                resultDTO.IsSuccess = false;
                resultDTO.StatusCode = "500";
                resultDTO.Message = $"An error occurred: {ex.Message}";
            }

            // Return the result DTO
            return resultDTO;
        }

        public ResultDTO Test(int userId)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
                //string encrytedUserId = Encryption.EncryptPlainTextToCipherText(userId);
                //string decryptedUserIdString = Encryption.DecryptCipherTextToPlainText(encrytedUserId);
                //int.TryParse(decryptedUserIdString, out int decryptedUserId);
                var existingdata = _oMTDataContext?.UserTest
                            .Where(x => x.UserId == userId && x.StartTime == null && x.EndTime == null).FirstOrDefault();
                if (existingdata != null)
                {

                    var result = from itest in _oMTDataContext.UserTest
                                 join test in _oMTDataContext.Tests
                                 on itest.TestId equals test.Id
                                 join user in _oMTDataContext.UserProfile
                                 on itest.UserId equals user.UserId
                                 where itest.UserId == existingdata.UserId
                                 orderby itest.CreateTimestamp descending
                                 select new UserTestDTO()
                                 {
                                     username = user.FirstName + " " + user.LastName,
                                     text = test.Test_text,
                                     duration = test.Duration,
                                     wpm = itest.WPM,
                                     accuracy = itest.Accuracy != 0 ? Convert.ToDouble(itest.Accuracy) : 0f,
                                     userTestId = itest.Id,
                                     testdate = DateOnly.FromDateTime(itest.CreateTimestamp)
                                     
                                 };


                    // Example: Convert the result to a list
                    var joinedData = result.ToList();
                    resultDTO.IsSuccess = true;
                    resultDTO.Message = "fetch starttime and endtime null successfully ";
                    resultDTO.Data = joinedData;
                    resultDTO.StatusCode = "200";
                }
                else
                {
                    resultDTO.IsSuccess = false;
                    resultDTO.Message = "User Id not found";
                    resultDTO.StatusCode = "404";
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions and update the result DTO with error details
                resultDTO.IsSuccess = false;
                resultDTO.StatusCode = "500";
                resultDTO.Message = $"An error occurred: {ex.Message}";
            }

            // Return the result DTO
            return resultDTO;
        }


        public ResultDTO GetCountOfTestByUser(int userId)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
                // Fetch count of pending assessments (StartTime and EndTime are null)
                int count = _oMTDataContext?.UserTest
                    .Where(x => x.UserId == userId && x.StartTime == null && x.EndTime == null)
                    .Count() ?? 0;

                // Get the current month
                var currentMonth = DateTime.Now.Month;

                // Fetch count of completed assessments (StartTime and EndTime are not null, and the CreateTimestamp is from the current month)
                int count_completed = _oMTDataContext?.UserTest
                    .Where(x => x.UserId == userId && x.StartTime != null && x.EndTime != null && x.CreateTimestamp.Month == currentMonth)
                    .Count() ?? 0;

                // Return success with the counts in a structured format
                resultDTO.IsSuccess = true;
                resultDTO.Message = "Fetched pending and completed assessments successfully.";
                resultDTO.Data = new
                {
                    AssessmentsPending = count,
                    AssessmentsCompleted = count_completed
                };
                resultDTO.StatusCode = "200";
            }
            catch (Exception ex)
            {
                // Handle exceptions and update the result DTO with error details
                resultDTO.IsSuccess = false;
                resultDTO.StatusCode = "500";
                resultDTO.Message = $"An error occurred: {ex.Message}";
            }

            // Return the result DTO
            return resultDTO;
        }






        public ResultDTO AgentProgressBar(AgentProgressBarRequestDTO agentProgressBarRequestDTO)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
                var result = from itest in _oMTDataContext.UserTest
                             join test in _oMTDataContext.Tests
                             on itest.TestId equals test.Id
                             join user in _oMTDataContext.UserProfile
                             on itest.UserId equals user.UserId
                             where agentProgressBarRequestDTO.userids.Contains(itest.UserId) &&
                                   itest.StartTime != null &&
                                   itest.EndTime != null &&
                                   itest.CreateTimestamp >= agentProgressBarRequestDTO.startdate &&
                                   itest.CreateTimestamp <= agentProgressBarRequestDTO.enddate
                             orderby itest.CreateTimestamp descending
                             select new AgentProgressBarResponseDTO()
                             {
                                 username = user.FirstName + " " + user.LastName,
                                 email = user.Email,
                                 wpm = (int)itest.WPM,
                                 accuracy = (float)(itest.Accuracy != 0 ? Convert.ToDouble(itest.Accuracy) : 0f),
                                 testdate = DateOnly.FromDateTime(itest.CreateTimestamp)
                             };

                // Convert the result to a list
                var joinedData = result.ToList();

                resultDTO.IsSuccess = true;
                resultDTO.Message = $"Filtered User Progress Data Count: {joinedData.Count}";
                resultDTO.Data = joinedData;
                resultDTO.StatusCode = "200";

            }
            catch (InvalidCastException ex)
            {
                resultDTO.IsSuccess = false;
                resultDTO.StatusCode = "400";
                resultDTO.Message = $"Type casting error: {ex.Message}";
            }
            catch (Exception ex)
            {
                resultDTO.IsSuccess = false;
                resultDTO.StatusCode = "500";
                resultDTO.Message = $"An error occurred: {ex.Message}";
            }

            return resultDTO;
        }


        public ResultDTO AgentProgressBar(int userId )
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
                var currentMonth = DateTime.Now.Month;
                var count = _oMTDataContext?.UserTest
                    .Where(x => x.UserId == userId && x.StartTime != null && x.EndTime != null && x.CreateTimestamp.Month == currentMonth)
                    .Count() ?? 0;




                resultDTO.IsSuccess = true;
                resultDTO.Message = $"Filtered User Progress Data Count: {count}";
                resultDTO.Data = count;
                resultDTO.StatusCode = "200";

            }
            catch (InvalidCastException ex)
            {
                resultDTO.IsSuccess = false;
                resultDTO.StatusCode = "400";
                resultDTO.Message = $"Type casting error: {ex.Message}";
            }
            catch (Exception ex)
            {
                resultDTO.IsSuccess = false;
                resultDTO.StatusCode = "500";
                resultDTO.Message = $"An error occurred: {ex.Message}";
            }

            return resultDTO;
        }

    }
}
