using Microsoft.EntityFrameworkCore;
using OMT.DataAccess.Context;
using OMT.DataAccess.Entities;
using OMT.DataService.Interface;
using OMT.DTO;
using System;
using System.Linq;
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

        public async Task<ResultDTO> GetBroadCastAnnouncements()
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
                var broadCostList = await _oMTDataContext.BroadCastAnnouncement
                                     .Where(x => x.SoftDelete == false)  // Assuming 'false' means not deleted or inactive
                                     .ToListAsync();
                resultDTO.IsSuccess = true;
                resultDTO.Message = "Successfully fetched broad cost announcement messages that are not deleted";
                resultDTO.Data = broadCostList;
            }
            catch (Exception ex)
            {
                resultDTO.IsSuccess = false;
                resultDTO.StatusCode = "500";
                resultDTO.Message = $"An error occurred: {ex.Message}";
            }
            return resultDTO;
        }

        public async Task<ResultDTO> CreateNewBroadCastMessages(BroadCastAnnouncementRequestDTO broadCastAnnouncementRequestDTO)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
                var existBroadCast = await _oMTDataContext.BroadCastAnnouncement
                    .Where(x => x.BroadCastMessage == broadCastAnnouncementRequestDTO.BroadCastMessage
                             && x.StartDateTime == broadCastAnnouncementRequestDTO.StartDateTime
                             && x.EndDateTime == broadCastAnnouncementRequestDTO.EndDateTime)
                    .FirstOrDefaultAsync();

                if (existBroadCast == null)
                {
                    if (broadCastAnnouncementRequestDTO.StartDateTime <= broadCastAnnouncementRequestDTO.EndDateTime && broadCastAnnouncementRequestDTO.BroadCastMessage != null)
                    {
                        var broadCastObj = new BroadCastAnnouncement()
                        {
                            BroadCastMessage = broadCastAnnouncementRequestDTO.BroadCastMessage,
                            StartDateTime = broadCastAnnouncementRequestDTO.StartDateTime,
                            EndDateTime = broadCastAnnouncementRequestDTO.EndDateTime
                        };
                        await _oMTDataContext.AddAsync(broadCastObj);
                        await _oMTDataContext.SaveChangesAsync();
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

        public async Task<ResultDTO> FilterBroadCastWithStartDateAndEndDate()
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
                // Define IST timezone
                var istTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");

                // Get current IST time
                var currentISTTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, istTimeZone);

                // Fetch data from the database asynchronously
                var messages = await _oMTDataContext.BroadCastAnnouncement
                    .Where(msg => msg.SoftDelete == false) // Apply the SoftDelete filter in the DB
                    .ToListAsync(); // Retrieve all records as a list

                // Filter the results in-memory (client-side filtering)
                var filteredMessages = messages
                    .Where(msg =>
                        TimeZoneInfo.ConvertTimeFromUtc(msg.StartDateTime.ToUniversalTime(), istTimeZone) <= currentISTTime
                        && TimeZoneInfo.ConvertTimeFromUtc(msg.EndDateTime.ToUniversalTime(), istTimeZone) >= currentISTTime)
                    .Select(msg => new
                    {
                        Message = msg.BroadCastMessage // Only return the message content
                    })
                    .ToList(); // Execute the final filtering in memory

                resultDTO.IsSuccess = true;
                resultDTO.Message = "Filtered BroadCast fetched Successfully.";
                resultDTO.StatusCode = "200";
                resultDTO.Data = filteredMessages;
            }
            catch (Exception ex)
            {
                resultDTO.IsSuccess = false;
                resultDTO.StatusCode = "500";
                resultDTO.Message = $"An error occurred: {ex.Message}";
            }
            return resultDTO;
        }



        public async Task<ResultDTO> UpdateSoftDeleteFlag(int Id)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
                var existBroadCast = await _oMTDataContext.BroadCastAnnouncement
                    .Where(x => x.Id == Id)
                    .FirstOrDefaultAsync();

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
                        await _oMTDataContext.SaveChangesAsync();
                        var message = "Broadcast marked as deleted successfully.";
                        var data = new { Id = Id, Message = message };
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
