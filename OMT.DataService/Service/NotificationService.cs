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

        /// <summary>
        /// Creates a notification with optional file upload to Azure Blob Storage.
        /// </summary>
        /// <param name="notificationDTO">Notification data transfer object.</param>
        /// <param name="file">Optional file to be uploaded to Azure Blob Storage.</param>
        /// <returns>Result DTO with success status and details.</returns>
        public async Task<ResultDTO> CreateNotificationAsync(NotificationDTO notificationDTO, IFormFile file)
        {
            var resultDTO = new ResultDTO { IsSuccess = true, StatusCode = "200" };

            try
            {
                // Validate notification data
                if (string.IsNullOrWhiteSpace(notificationDTO.NotificationMessage) || notificationDTO.UserId <= 0)
                {
                    return new ResultDTO
                    {
                        IsSuccess = false,
                        StatusCode = "400",
                        Message = "Invalid input. Please ensure required fields are provided."
                    };
                }

                // File size validation (Max 100MB)
                const long MaxFileSize = 100 * 1024 * 1024; // 100MB in bytes
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

                // Process file upload if a file is provided
                if (file != null && file.Length > 0)
                {
                    // Upload the file to Azure Blob Storage
                    fileUrl = await _azureBlob.UploadFileAsync(_azureSettings.Container, file.FileName, file.OpenReadStream());
                }

                // Save notification to the database
                var notification = new Notification
                {
                    UserId = notificationDTO.UserId,
                    NotificationMessage = notificationDTO.NotificationMessage,
                    FileUrl = fileUrl,
                    CreateTimeStamp = DateTime.UtcNow
                };

                await _dbContext.Notification.AddAsync(notification);
                await _dbContext.SaveChangesAsync();

                resultDTO.Message = "Notification created successfully.";
                resultDTO.Data = new
                {
                    Id = notification.Id,
                    UserId = notification.UserId,
                    NotificationMessage = notification.NotificationMessage
                };
                resultDTO.StatusCode = "201";
            }
            catch (Exception ex)
            {
                resultDTO.IsSuccess = false;
                resultDTO.StatusCode = "500";
                resultDTO.Message = $"An error occurred: {ex.Message}";
            }

            return resultDTO;
        }


        /// <summary>
        /// Fetches the five most recent notifications.
        /// </summary>
        /// <returns>Result DTO with recent notifications.</returns>
        public async Task<ResultDTO> FetchFiveRecentNotificationsAsync()
        {
            var resultDTO = new ResultDTO { IsSuccess = true, StatusCode = "200" };

            try
            {
                var notifications = await _dbContext.Notification
                    .OrderByDescending(x => x.CreateTimeStamp)
                    .Take(5)
                    .ToListAsync();

                resultDTO.Message = notifications.Any()
                    ? "Successfully fetched the most recent notifications."
                    : "No notifications found.";
                resultDTO.Data = notifications.Select(x => new
                {
                    Id = x.Id,  // Corrected from x.notification.Id to x.Id
                    UserId = x.UserId,  // Corrected from x.notification.UserId to x.UserId
                    NotificationMessage = x.NotificationMessage  // Corrected from x.notification.NotificationMessage to x.NotificationMessage
                }).ToList();
            }
            catch (Exception ex)
            {
                resultDTO.IsSuccess = false;
                resultDTO.StatusCode = "500";
                resultDTO.Message = $"An error occurred: {ex.Message}";
            }

            return resultDTO;
        }

        /// <summary>
        /// Fetches paginated notifications with user details.
        /// </summary>
        /// <param name="pageNumber">Page number for pagination.</param>
        /// <param name="pageSize">Number of items per page.</param>
        /// <returns>Result DTO with paginated notifications.</returns>
        public async Task<ResultDTO> GetAllNotificationsAsync(int pageNumber = 1, int pageSize = 10)
        {
            var resultDTO = new ResultDTO { IsSuccess = true, StatusCode = "200" };

            try
            {
                var totalCount = await _dbContext.Notification.CountAsync();

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

                resultDTO.Message = notifications.Any()
                    ? "Notifications successfully retrieved."
                    : "No notifications found.";
                resultDTO.Data = new
                {
                    Notifications = notifications,
                    TotalCount = totalCount
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

        /// <summary>
        /// Generates a SAS URL for downloading a file associated with a notification.
        /// </summary>
        /// <param name="id">Notification ID.</param>
        /// <returns>Result DTO with the SAS URL.</returns>
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

                // Extract blob name from the URL
                var blobName = Uri.UnescapeDataString(new Uri(notification.FileUrl).Segments.Last());

                // Generate SAS URL
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
                // Find the notification by ID
                var notification = await _dbContext.Notification
                    .FirstOrDefaultAsync(n => n.Id == notificationId);

                if (notification == null)
                {
                    resultDTO.IsSuccess = false;
                    resultDTO.StatusCode = "404";
                    resultDTO.Message = "Notification not found.";
                    return resultDTO;
                }

                // Delete the notification record from the database
                _dbContext.Notification.Remove(notification);
                await _dbContext.SaveChangesAsync();

                resultDTO.Message = "Notification deleted successfully.";
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
