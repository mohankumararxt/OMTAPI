using Microsoft.EntityFrameworkCore;
using OMT.DataAccess.Context;
using OMT.DataAccess.Entities;
using OMT.DataService.Interface;
using OMT.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DataService.Service
{
    public class BroadCastAnnouncementService : IBroadCastAnnouncementService
    {
        private readonly OMTDataContext _oMTDataContext;
        public BroadCastAnnouncementService(OMTDataContext oMTDataContext)
        {
            _oMTDataContext = oMTDataContext;
        }
        //public ResultDTO GetBroadCastAnnouncementMessages()
        //{
        //    ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
        //    try
        //    {
        //        var broadCostList = _oMTDataContext.BroadCastAnnouncement.ToList();
        //        resultDTO.IsSuccess = true;
        //        resultDTO.Message = "Successfully fetched all broad cost announcement messages.";
        //        resultDTO.Data = broadCostList;
        //    }
        //    catch (Exception ex)
        //    {
        //        // Handle exceptions and update the result DTO with error details
        //        resultDTO.IsSuccess = false;
        //        resultDTO.StatusCode = "500";
        //        resultDTO.Message = $"An error occurred: {ex.Message}";
        //    }
        //    return resultDTO;
        //}

        public ResultDTO GetBroadCastAnnouncementsBySoftDelete()
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
                var broadCostList = _oMTDataContext.BroadCastAnnouncement
                                     .Where(x => x.SoftDelete == false)  // Assuming '1' means deleted or inactive
                                     .ToList();
                resultDTO.IsSuccess = true;
                resultDTO.Message = "Successfully fetched broad cost announcement messages that are marked as deleted";
                resultDTO.Data = broadCostList;
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

        public ResultDTO CreateNewBroadCastMessages(BroadCastAnnouncementRequestDTO broadCastAnnouncementRequestDTO)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
                var existBroadCast = _oMTDataContext.BroadCastAnnouncement.Where(x => 
                x.BroadCastMessage == broadCastAnnouncementRequestDTO.BroadCastMessage 
                && x.StartDateTime == broadCastAnnouncementRequestDTO.StartDateTime
                && x.EndDateTime == broadCastAnnouncementRequestDTO.EndDateTime).FirstOrDefault();
                if (existBroadCast == null) {
                    if (broadCastAnnouncementRequestDTO.StartDateTime <= broadCastAnnouncementRequestDTO.EndDateTime && broadCastAnnouncementRequestDTO.BroadCastMessage != null)
                    {
                        var broadCastObj = new BroadCastAnnouncement()
                        {
                            BroadCastMessage = broadCastAnnouncementRequestDTO.BroadCastMessage,
                            StartDateTime = broadCastAnnouncementRequestDTO.StartDateTime,
                            EndDateTime = broadCastAnnouncementRequestDTO.EndDateTime
                        };
                        _oMTDataContext.Add(broadCastObj);
                        _oMTDataContext.SaveChanges();
                        resultDTO.IsSuccess = true;
                        resultDTO.Message = "Created BroadCast Announcement Successfully.";
                        resultDTO.StatusCode = "201";
                    }
                    else
                    {
                        resultDTO.IsSuccess = false;
                        resultDTO.Message = "Some required fields are either missing or invalid.";
                        resultDTO.StatusCode = "400";
                    }
                }
                else
                {
                    resultDTO.IsSuccess = false;
                    resultDTO.Message = "BroadCast Announcement already exists.";
                    resultDTO.StatusCode = "400";
                }
            }
            catch (Exception ex)
            {
                resultDTO.IsSuccess = false;
                resultDTO.StatusCode = "500";
                resultDTO.Message = $"An error occurred: {ex.Message}";
            }
            return resultDTO;
        }


        public ResultDTO FilterBroadCastWithStartDateAndEndDate()
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
                // Define IST timezone
                var istTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");

                // Get current IST time
                var currentISTTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, istTimeZone);

                var messages = _oMTDataContext.BroadCastAnnouncement
                    .AsEnumerable() // Switch to in-memory filtering to use TimeZone conversion
                    .Where(msg =>
                        TimeZoneInfo.ConvertTimeFromUtc(msg.StartDateTime.ToUniversalTime(), istTimeZone) <= currentISTTime
                        && TimeZoneInfo.ConvertTimeFromUtc(msg.EndDateTime.ToUniversalTime(), istTimeZone) >= currentISTTime
                        && msg.SoftDelete == false) // Apply filters
                    .Select(msg => new
                    {
                        Message = msg.BroadCastMessage // Only return the content
                    })
                    .ToList();

                resultDTO.IsSuccess = true;
                resultDTO.Message = "Filtered BroadCast fetched Successfully.";
                resultDTO.StatusCode = "200";
                resultDTO.Data = messages;
            }
            catch (Exception ex)
            {
                resultDTO.IsSuccess = false;
                resultDTO.StatusCode = "500";
                resultDTO.Message = $"An error occurred: {ex.Message}";
            }
            return resultDTO;
        }


        public ResultDTO UpdateSoftDeleteFlag(int Id)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
                var existBroadCast = _oMTDataContext.BroadCastAnnouncement.Where(x => x.Id == Id).FirstOrDefault(); ;
                if (existBroadCast != null)
                {
                    if (existBroadCast.SoftDelete == true)
                    {
                        resultDTO.IsSuccess = false;
                        resultDTO.Message = "This ID has already been soft deleted.";
                        resultDTO.StatusCode = "400";
                    }
                    else
                    {
                        existBroadCast.SoftDelete = true;
                        _oMTDataContext.BroadCastAnnouncement.Update(existBroadCast);
                        _oMTDataContext.SaveChanges();
                        var message = "Broadcast marked as deleted successfully.";
                        var data = new { Id = Id,  Message = message};
                        resultDTO.IsSuccess = true;
                        resultDTO.Message = "Updated Successfully.";
                        resultDTO.StatusCode = "201";
                    }
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
                resultDTO.IsSuccess = false;
                resultDTO.StatusCode = "500";
                resultDTO.Message = $"An error occurred: {ex.Message}";
            }
            return resultDTO;
        }



    }
}
