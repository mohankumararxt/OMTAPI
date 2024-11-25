using OMT.DataAccess.Context;
using OMT.DataAccess.Entities;
using OMT.DataService.Interface;
using OMT.DataService.Utility;
using OMT.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace OMT.DataService.Service
{
    public class InterviewService : IInterviewService
    {
        private readonly OMTDataContext _oMTDataContext;
        //private readonly ValidationField _validationField;
        

        public InterviewService(OMTDataContext oMTDataContext)
        {
            _oMTDataContext = oMTDataContext;
        }


        public ResultDTO SaveInterviewee(UserInterviewsDTO userInterviewsDTO)
        {
            // 
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };

            try
            {
                //DateOnly dobDateOnly = DateOnly.FromDateTime(userInterviewsDTO.DOB);
                // Check if TypingUsers is not null to avoid NullReferenceException
                var user = _oMTDataContext?.UserInterviews
                    .Where(x => x.Firstname == userInterviewsDTO.Firstname && x.Lastname == userInterviewsDTO.Lastname && 
                    x.Email == userInterviewsDTO.Email && x.phone == userInterviewsDTO.phone).FirstOrDefault();

                if (user != null)
                {
                    resultDTO.IsSuccess = false;
                    resultDTO.Message = "User already exists. Please try to add different User";
                }
                else
                {
                    if (!ValidationField.IsValidEmail(userInterviewsDTO.Email))
                    {
                        resultDTO.IsSuccess = false;
                        resultDTO.Message = "Invalid email address.";
                        resultDTO.StatusCode = "400";
                        return resultDTO;
                    }

                    if (!ValidationField.IsValidPhone(userInterviewsDTO.phone))
                    {
                        resultDTO.IsSuccess = false;
                        resultDTO.Message = "Invalid phone number. Must be a 10-digit number.";
                        resultDTO.StatusCode = "400";
                        return resultDTO;
                        //throw new ArgumentException("Invalid phone number. Must be a 10-digit number.");
                    }

                    var newUser = new UserInterviews()
                    {
                        Firstname = userInterviewsDTO.Firstname,
                        Lastname = userInterviewsDTO.Lastname,
                        Email = userInterviewsDTO.Email,
                        phone = userInterviewsDTO.phone
                        //CreateTimestamp = DateTime.Now
                    };
                    _oMTDataContext.Add(newUser);
                    _oMTDataContext.SaveChanges();
                    resultDTO.IsSuccess = true;
                    resultDTO.Message = "User Inserted Successfully";
                    var userId = newUser.Id;
                    int randomTestId = _oMTDataContext.Tests
                            .Where(t => t.IsSample) // Filter where IsSample is false
                            .OrderBy(t => Guid.NewGuid()) // Randomize the order
                            .Select(t => t.Id) // Select only the Id
                            .First(); // Take the first Id or default if no match


                    // Map DTO to entity and insert into the database
                    var interviewTest = new InterviewTests()
                    {
                        UserId = userId,
                        TestId = randomTestId

                    };

                    // Add to the database context
                    _oMTDataContext?.InterviewTests.Add(interviewTest);

                    // Save changes to the database
                    _oMTDataContext?.SaveChanges();
                    int interviewtestid = interviewTest.Id;

                    var result = from itest in _oMTDataContext.InterviewTests
                                 join test in _oMTDataContext.Tests
                                 on itest.TestId equals test.Id
                                 where itest.UserId == userId
                                 select new InterviewTestsDTO()
                                 {
                                     id = itest.Id,
                                     UserId = itest.UserId,
                                     TestId = itest.TestId,
                                     Test_text = test.Test_text,
                                     Duration = test.Duration
                                 };


                    // Example: Convert the result to a list
                    var joinedData = result.ToList();


                    resultDTO.IsSuccess = true;
                    resultDTO.Message = "InterviewTest Inserted Successfully";
                    resultDTO.Data = joinedData;

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


        public ResultDTO UpdateInterviewStartTime(UpdateInterviewTestTimeDTO updateInterviewTestTime)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {

                var existingdata = _oMTDataContext?.InterviewTests
                        .Where(x => x.Id == updateInterviewTestTime.Id).FirstOrDefault();
                if (existingdata != null)
                {


                    existingdata.StartTime = updateInterviewTestTime.datetime;
                    // update data in [dbo].[Interview_Tests] table
                    _oMTDataContext.InterviewTests.Update(existingdata);
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


        public ResultDTO UpdateInterviewTests(UpdateInterviewTestsDTO updateInterviewTests) 
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {

                var existingdata = _oMTDataContext?.InterviewTests
                        .Where(x => x.Id == updateInterviewTests.Id).FirstOrDefault();
                if (existingdata != null)
                {

                    existingdata.WPM = updateInterviewTests.WPM;
                    existingdata.Accuracy = updateInterviewTests.Accuracy;
                    existingdata.EndTime = updateInterviewTests.datetime;
                    // update data in [dbo].[Interview_Tests] table
                    _oMTDataContext.InterviewTests.Update(existingdata);
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


        public ResultDTO GetIntervieweesLeaderboard()
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };

            try
            {
                var result = from itest in _oMTDataContext.InterviewTests
                             join test in _oMTDataContext.Tests
                             on itest.TestId equals test.Id
                             join user in _oMTDataContext.UserInterviews
                             on itest.UserId equals user.Id
                             orderby itest.CreateTimestamp descending
                             select new LeaderboardDTO()
                             {
                                 username = user.Firstname + " " + user.Lastname,
                                 email = user.Email,
                                 phone = user.phone,
                                 wpm = itest.WPM,
                                 accuracy = itest.Accuracy.HasValue ? Convert.ToDouble(itest.Accuracy.Value) : 0f,  // Handle nullable Double
                                 duration = test.Duration,
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
