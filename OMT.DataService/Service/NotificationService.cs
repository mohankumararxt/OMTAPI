using Azure.Core;
using Azure.Storage.Sas;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OMT.DataAccess.Context;
using OMT.DataAccess.Entities;
using OMT.DataService.Interface;
using OMT.DataService.Settings;
using OMT.DataService.Utility;
using OMT.DTO;
using System;
using System.Drawing.Printing;
using System.Linq;
using System.Threading.Tasks;

namespace OMT.DataService.Service
{
    public class NotificationService : INotificationService
    {
        private readonly OMTDataContext _dbContext;
        private readonly AzureBlob _azureBlob;
        private readonly AzureConnectionSettings _azureSettings;

        public NotificationService(
            OMTDataContext dbContext,
            AzureBlob azureBlob,
            IOptions<AzureConnectionSettings> azureSettings)
        {
            _dbContext = dbContext;
            _azureBlob = azureBlob;
            _azureSettings = azureSettings.Value;
        }
        private bool ValidateEntityUserAsync<T>(DbSet<T> dbSet, int id) where T : class
        {
            return  dbSet.Any(entity => EF.Property<int>(entity, "UserId") == id);
        }
        private string ValidateNotification(NotificationDTO notificationDTO)
        {
            if (string.IsNullOrWhiteSpace(notificationDTO.NotificationMessage))
                return "NotificationMessage cannot be empty.";
            if (notificationDTO.NotificationMessage.Length > 50)
                return "NotificationMessage exceeds the maximum allowed length of 50 characters.";
            if (notificationDTO.UserId <= 0)
                return "Invalid UserId.";
            if (!ValidateEntityUserAsync(_dbContext.UserProfile, notificationDTO.UserId))
                return "Invalid UserId.";
            return string.Empty;
        }

        private string GetBlobNameFromUrl(string fileUrl)
        {
            return Uri.UnescapeDataString(new Uri(fileUrl).Segments.Last());
        }

        public async Task<ResultDTO> CreateNotificationAsync(NotificationDTO notificationDTO, IFormFile file)
        {
            var validationError = ValidateNotification(notificationDTO);
            if (!string.IsNullOrEmpty(validationError))
            {
                return new ResultDTO
                {
                    IsSuccess = false,
                    StatusCode = "400",
                    Message = validationError
                };
            }

            var resultDTO = new ResultDTO { IsSuccess = true, StatusCode = "200" };

            try
            {
                var exist_notification = _dbContext?.Notification
                 .Where(x => x.UserId == notificationDTO.UserId && x.NotificationMessage == notificationDTO.NotificationMessage).FirstOrDefault();

                const long MaxFileSize = 100 * 1024 * 1024; // 100MB
                if (exist_notification != null)
                {
                    resultDTO.IsSuccess = false;
                    resultDTO.StatusCode = "409";
                    resultDTO.Message = $"Notification already exists.";
                }
                else
                {

                    if (file != null && file.Length > MaxFileSize)
                    {
                        return new ResultDTO
                        {
                            IsSuccess = false,
                            StatusCode = "400",
                            Message = "File size exceeds the maximum allowed size of 100MB."
                        };
                    }

                    string fileUrl = null;

                    if (file != null && file.Length > 0)
                    {
                        fileUrl = await _azureBlob.UploadFileAsync(_azureSettings.Container, file.FileName, file.OpenReadStream());
                    }
                    var existingUser = _dbContext?.UserProfile.Where(x => x.UserId == notificationDTO.UserId).FirstOrDefault();
                    var notification = new Notification
                    {
                        UserId = notificationDTO.UserId,
                        NotificationMessage = notificationDTO.NotificationMessage,
                        FileUrl = fileUrl,
                        CreateTimeStamp = DateTime.UtcNow
                    };

                    await _dbContext.Notification.AddAsync(notification);
                    await _dbContext.SaveChangesAsync();
                    resultDTO.Data = new
                    {
                        Id = notification.Id,
                        Username = existingUser.FirstName + " " + existingUser.LastName,
                        NotificationMessage = notification.NotificationMessage,
                        CreateTimeStamp = notification.CreateTimeStamp
                    };
                    resultDTO.Message = "Notification created successfully.";
                    resultDTO.StatusCode = "201";
                    
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

        public async Task<ResultDTO> FetchFiveRecentNotificationsAsync()
        {
            var resultDTO = new ResultDTO { IsSuccess = true, StatusCode = "200" };

            try
            {
                var istTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");

                var notifications = await (from notification in _dbContext.Notification
                                           join user in _dbContext.UserProfile
                                           on notification.UserId equals user.UserId
                                           orderby notification.CreateTimeStamp descending
                                           select new
                                           {
                                               Id = notification.Id,
                                               UserName = $"{user.FirstName} {user.LastName}",
                                               NotificationMessage = notification.NotificationMessage,
                                               CreateTimeStamp = TimeZoneInfo.ConvertTimeFromUtc(notification.CreateTimeStamp, istTimeZone)
                                           }).Take(5).ToListAsync();

                resultDTO.Message = notifications.Any()
                    ? "Successfully fetched the most recent notifications."
                    : "No notifications found.";

                resultDTO.Data = notifications;


            }
            catch (Exception ex)
            {
                resultDTO.IsSuccess = false;
                resultDTO.StatusCode = "500";
                resultDTO.Message = $"An error occurred: {ex.Message}";
            }

            return resultDTO;
        }

        public async Task<ResultDTO> GetAllNotificationsAsync(int pageNumber = 1, int pageSize = 10)
        {
            var resultDTO = new ResultDTO { IsSuccess = true, StatusCode = "200" };

            try
            {
                var totalCount = await _dbContext.Notification.CountAsync();

                var istTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");

                var notifications = await (from notification in _dbContext.Notification
                                           join user in _dbContext.UserProfile
                                           on notification.UserId equals user.UserId
                                           orderby notification.CreateTimeStamp descending
                                           select new
                                           {
                                               Id = notification.Id,
                                               UserName = $"{user.FirstName} {user.LastName}",
                                               NotificationMessage = notification.NotificationMessage,
                                               CreateTimeStamp = notification.CreateTimeStamp
                                           })
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                // Convert CreateTimeStamp to IST in memory
                var notificationsWithIst = notifications.Select(n => new
                {
                    n.Id,
                    n.UserName,
                    n.NotificationMessage,
                    CreateTimeStamp = TimeZoneInfo.ConvertTimeFromUtc(n.CreateTimeStamp, istTimeZone)
                }).ToList();



                resultDTO.Message = notifications.Any()
                    ? "Notifications successfully retrieved."
                    : "No notifications found.";
                resultDTO.Data = new
                {
                    Notifications = notificationsWithIst,
                    TotalCount = totalCount,
                    TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                };
            }
            catch (Exception ex)
            {
                resultDTO.IsSuccess = false;
                resultDTO.StatusCode = "500";
                resultDTO.Message = $"An error occurred: {ex.Message}";
            }

            return resultDTO;
        }

        public async Task<ResultDTO> DownloadFileAsync(int id)
        {
            var resultDTO = new ResultDTO { IsSuccess = true, StatusCode = "200" };

            try
            {
                var notification = await _dbContext.Notification.FindAsync(id);
                if (notification == null || string.IsNullOrEmpty(notification.FileUrl))
                {
                    return new ResultDTO
                    {
                        IsSuccess = false,
                        StatusCode = "404",
                        Message = "Notification or file not found."
                    };
                }

                var blobName = GetBlobNameFromUrl(notification.FileUrl);

                var sasUrl = _azureBlob.GetBlobSasUri(
                    _azureSettings.Container,
                    _azureSettings.AccountKey,
                    blobName,
                    DateTimeOffset.UtcNow.AddHours(1),
                    BlobSasPermissions.Read);

                resultDTO.Message = "File ready for download.";
                resultDTO.Data = sasUrl;
            }
            catch (Exception ex)
            {
                resultDTO.IsSuccess = false;
                resultDTO.StatusCode = "500";
                resultDTO.Message = $"An error occurred: {ex.Message}";
            }

            return resultDTO;
        }

        public async Task<ResultDTO> DeleteNotificationAsync(int notificationId)
        {
            var resultDTO = new ResultDTO { IsSuccess = true, StatusCode = "200" };

            try
            {
                var notification = await _dbContext.Notification
                    .FirstOrDefaultAsync(n => n.Id == notificationId);

                if (notification == null)
                {
                    resultDTO.IsSuccess = false;
                    resultDTO.StatusCode = "404";
                    resultDTO.Message = "Notification not found.";
                    return resultDTO;
                }

                if (!string.IsNullOrEmpty(notification.FileUrl))
                {
                    var blobName = GetBlobNameFromUrl(notification.FileUrl);
                    await _azureBlob.DeleteBlobAsync(_azureSettings.Container, blobName);
                }

                _dbContext.Notification.Remove(notification);
                await _dbContext.SaveChangesAsync();

                resultDTO.Message = "Notification and associated file deleted successfully.";
            }
            catch (Exception ex)
            {
                resultDTO.IsSuccess = false;
                resultDTO.StatusCode = "500";
                resultDTO.Message = $"An unexpected error occurred: {ex.Message}";
            }

            return resultDTO;
        }
    }
}
