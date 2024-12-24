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

        private string ValidateBroadCastRequest(BroadCastAnnouncementRequestDTO request)
        {
            if (string.IsNullOrWhiteSpace(request.BroadCastMessage))
                return "BroadCastMessage cannot be empty.";

            if (request.BroadCastMessage.Length > 255)
                return "BroadCastMessage exceeds the maximum allowed length of 255 characters.";

            if (request.StartDateTime > request.EndDateTime)
                return "StartDateTime cannot be later than EndDateTime.";

            return string.Empty;
        }

        public async Task<ResultDTO> GetBroadCastAnnouncements(int pageNumber, int pageSize)
        {
            var resultDTO = new ResultDTO { IsSuccess = true, StatusCode = "200" };

            try
            {
                var query = _oMTDataContext.BroadCastAnnouncement
                    .Where(x => !x.SoftDelete);

                var totalRecords = await query.CountAsync();
                var announcements = await query
                    .OrderBy(x => x.StartDateTime) // Sorting by StartDateTime
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                resultDTO.Message = "Successfully fetched active BroadCast Announcements with pagination.";
                resultDTO.Data = new
                {
                    Announcements = announcements,
                    Pagination = new
                    {
                        CurrentPage = pageNumber,
                        PageSize = pageSize,
                        TotalRecords = totalRecords,
                        TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize)
                    }
                };
            }
            catch (Exception ex)
            {
                resultDTO = new ResultDTO
                {
                    IsSuccess = false,
                    StatusCode = "500",
                    Message = $"An error occurred: {ex.Message}"
                };
            }

            return resultDTO;
        }

        public async Task<ResultDTO> CreateNewBroadCastMessages(BroadCastAnnouncementRequestDTO request)
        {
            var validationError = ValidateBroadCastRequest(request);
            if (!string.IsNullOrEmpty(validationError))
            {
                return new ResultDTO
                {
                    IsSuccess = false,
                    StatusCode = "400",
                    Message = validationError
                };
            }

            try
            {
                // Check for uniqueness
                var isDuplicate = await _oMTDataContext.BroadCastAnnouncement
                    .AnyAsync(x => x.BroadCastMessage == request.BroadCastMessage);

                if (isDuplicate)
                {
                    return new ResultDTO
                    {
                        IsSuccess = false,
                        StatusCode = "400",
                        Message = "BroadCastMessage already exists. Please use a different message."
                    };
                }

                // Create new BroadCast announcement
                var newAnnouncement = new BroadCastAnnouncement
                {
                    BroadCastMessage = request.BroadCastMessage.Trim(),
                    StartDateTime = request.StartDateTime,
                    EndDateTime = request.EndDateTime
                };

                await _oMTDataContext.BroadCastAnnouncement.AddAsync(newAnnouncement);
                await _oMTDataContext.SaveChangesAsync();

                return new ResultDTO
                {
                    IsSuccess = true,
                    StatusCode = "201",
                    Message = "BroadCast Announcement created successfully."
                };
            }
            catch (Exception ex)
            {
                return new ResultDTO
                {
                    IsSuccess = false,
                    StatusCode = "500",
                    Message = $"An error occurred: {ex.Message}"
                };
            }
        }

        public async Task<ResultDTO> FilterBroadCastWithStartDateAndEndDate()
        {
            var resultDTO = new ResultDTO { IsSuccess = true, StatusCode = "200" };

            try
            {
                var currentISTTime = GetCurrentISTTime();

                var filteredMessages = await _oMTDataContext.BroadCastAnnouncement
                    .Where(msg => !msg.SoftDelete &&
                                  msg.StartDateTime <= currentISTTime &&
                                  msg.EndDateTime >= currentISTTime)
                    .Select(msg => new { msg.BroadCastMessage })
                    .ToListAsync();

                resultDTO.Message = "Filtered BroadCast fetched successfully.";
                resultDTO.Data = filteredMessages;
            }
            catch (Exception ex)
            {
                resultDTO = new ResultDTO
                {
                    IsSuccess = false,
                    StatusCode = "500",
                    Message = $"An error occurred: {ex.Message}"
                };
            }

            return resultDTO;
        }

        public async Task<ResultDTO> UpdateSoftDeleteFlag(int id)
        {
            var resultDTO = new ResultDTO { IsSuccess = true, StatusCode = "200" };

            try
            {
                var announcement = await _oMTDataContext.BroadCastAnnouncement.FindAsync(id);

                if (announcement == null)
                {
                    return new ResultDTO
                    {
                        IsSuccess = false,
                        StatusCode = "404",
                        Message = "Announcement not found."
                    };
                }

                if (announcement.SoftDelete)
                {
                    return new ResultDTO
                    {
                        IsSuccess = false,
                        StatusCode = "400",
                        Message = "This ID has already been soft deleted."
                    };
                }

                announcement.SoftDelete = true;
                _oMTDataContext.BroadCastAnnouncement.Update(announcement);
                await _oMTDataContext.SaveChangesAsync();

                resultDTO.Message = "Broadcast marked as deleted successfully.";
                resultDTO.StatusCode = "201";
            }
            catch (Exception ex)
            {
                resultDTO = new ResultDTO
                {
                    IsSuccess = false,
                    StatusCode = "500",
                    Message = $"An error occurred: {ex.Message}"
                };
            }

            return resultDTO;
        }

        private DateTime GetCurrentISTTime()
        {
            var istTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, istTimeZone);
        }
    }
}
