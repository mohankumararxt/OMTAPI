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
                }
                else
                {
                    resultDTO.IsSuccess = false;
                    resultDTO.Message = "Please Create Text";
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
                }
                else
                {
                    resultDTO.IsSuccess = false;
                    resultDTO.Message = "Id not found";
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
                        .Where(x => x.Id == updateUserTests.Id).FirstOrDefault();
                if (existingdata != null)
                {

                    existingdata.WPM = updateUserTests.WPM;
                    existingdata.Accuracy = updateUserTests.Accuracy;
                    existingdata.EndTime = updateUserTests.datetime;

                    _oMTDataContext.UserTest.Update(existingdata);
                    _oMTDataContext.SaveChanges();
                    resultDTO.IsSuccess = true;
                    resultDTO.Message = "Updated InterviewTests Successfully";
                }
                else
                {
                    resultDTO.IsSuccess = false;
                    resultDTO.Message = "data not found";
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
                                 text = test.Test_text,
                                 wpm = itest.WPM,
                                 accuracy = itest.Accuracy.HasValue ? Convert.ToDouble(itest.Accuracy.Value) : 0f,  // Handle nullable Double
                                 testdate = DateOnly.FromDateTime(itest.CreateTimestamp)
                             };


                // Example: Convert the result to a list
                var joinedData = result.ToList();


                resultDTO.IsSuccess = true;
                resultDTO.Message = "Leaderboard fetch successfully...";
                resultDTO.Data = joinedData;


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
    }
}
