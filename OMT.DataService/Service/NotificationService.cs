using Azure.Core;
using OMT.DataAccess.Context;
using OMT.DataAccess.Entities;
using OMT.DataService.Interface;
using OMT.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using OMT.DataService.Settings;
using OMT.DataService.Utility;

namespace OMT.DataService.Service
{
    public class NotificationService: INotificationService
    {
        private const long MaxFileSize = 100 * 1024 * 1024; // 100 MB

        private readonly OMTDataContext _oMTDataContext;
        private readonly BlobServiceClient _blobServiceClient;
        private readonly IOptions<AzureConnectionSettings> _options;
        private readonly IConfiguration _configuration;
        //private readonly IHubContext<NotificationHub> _hubContext;
        public NotificationService(OMTDataContext oMTDataContext, BlobServiceClient blobServiceClient, IOptions<AzureConnectionSettings> options, IConfiguration configuration)
        {
            _oMTDataContext = oMTDataContext;
            _blobServiceClient = blobServiceClient;
            _options = options;
            _configuration = configuration;
        }

        public ResultDTO CreateNotification(NotificationDTO notificationDTO)
        {
            ResultDTO resultDTO = new ResultDTO { IsSuccess = true, StatusCode = "200" };

            try
            {
                // Validate notification message
                if (string.IsNullOrWhiteSpace(notificationDTO.NotificationMessage))
                {
                    resultDTO.IsSuccess = false;
                    resultDTO.StatusCode = "400";
                    resultDTO.Message = $"The 'NotificationMessage' field is required.";
                   
                }

                string filePath = null;

                try
                {
                    // Upload file to Blob Storage using SAS token
                    if (notificationDTO.FileBlob != null || (notificationDTO.UserId != null || notificationDTO.UserId > 0))
                    {
                        var fileBlob = notificationDTO.FileBlob;

                        // Authenticate using connection string
                        string connectionString = _options.Value.ConnectionString;
                        var blobServiceClient = new BlobServiceClient(connectionString);

                        // Get the container client
                        var containerClient = blobServiceClient.GetBlobContainerClient(_options.Value.Container);

                        // Ensure the container exists
                        containerClient.CreateIfNotExists();

                        // Get the blob client
                        var blobClient = containerClient.GetBlobClient(fileBlob.FileName);

                        // Upload the file
                        using var stream = fileBlob.OpenReadStream();
                        blobClient.Upload(stream, true);

                        // Set the file path (URL)
                        filePath = blobClient.Uri.ToString();

                        // Save notification in database
                        var notification = new Notification
                        {
                            UserId = notificationDTO.UserId,
                            NotificationMessage = notificationDTO.NotificationMessage,
                            FileUrl = filePath
                        };

                        _oMTDataContext.Notification.Add(notification);
                        _oMTDataContext.SaveChanges();

                        resultDTO.IsSuccess = true;
                        resultDTO.Message = "Notification created successfully";
                        resultDTO.Data = new
                        {
                            Id = notification.Id,
                            UserId = notification.UserId,
                            NotificationMessage = notification.NotificationMessage
                        };
                        resultDTO.StatusCode = "201";

                    }
                    else
                    {
                        resultDTO.IsSuccess = false;
                        resultDTO.Message = "One or more required fields are missing. Please review your input and try again.";
                        resultDTO.StatusCode = "400";
                    }

                    
                }
                catch (Exception ex)
                {
                    resultDTO.IsSuccess = false;
                    resultDTO.Message = $"An error occurred while processing your request: {ex.Message}";
                    resultDTO.StatusCode = "500";
                }
            }
            catch (Exception ex)
            {
                resultDTO.IsSuccess = false;
                resultDTO.StatusCode = "500";
                resultDTO.Message = $"An unexpected error occurred: {ex.Message}";
            }

            return resultDTO;
        }

        public ResultDTO FetchFiveRecentNotifications()
        {
            ResultDTO resultDTO = new ResultDTO { IsSuccess = true, StatusCode = "200" };

            try
            {
                var notifications = _oMTDataContext.Notification
                    .OrderByDescending(x => x.CreateTimeStamp)
                    .Take(5) 
                    .ToList();
                if (notifications != null) { 
                    resultDTO.IsSuccess = true;
                    resultDTO.Message = "Successfully fetched the most recent notifications.";
                    resultDTO.Data = notifications;
                    resultDTO.StatusCode = "200";
                }
                else
                {
                    resultDTO.IsSuccess = true;
                    resultDTO.Message = "No notifications found.";
                    resultDTO.StatusCode = "200";
                }

            }
            catch (Exception ex)
            {

                resultDTO.IsSuccess = false;
                resultDTO.StatusCode = "500";
                resultDTO.Message = $"An unexpected error occurred: {ex.Message}";

            }

            return resultDTO;
        }

        public ResultDTO GetAllNotifications(int pageNumber = 1, int pageSize = 10)
        {
            ResultDTO resultDTO = new ResultDTO { IsSuccess = true, StatusCode = "200" };

            try
            {
                //if (pageNumber <= 0) pageNumber = 1;
                //if (pageSize <= 0) pageSize = 10;

                var totalCount =  _oMTDataContext.Notification.Count();


                var notifications = (from notification in _oMTDataContext.Notification
                                     join user in _oMTDataContext.UserProfile
                                     on notification.UserId equals user.UserId
                                     orderby notification.CreateTimeStamp descending
                                     select new
                                     {
                                         Id = notification.Id,
                                         UserName = user.FirstName + " " + user.LastName,
                                         NotificationMessage = notification.NotificationMessage,
                                         FileUrl = notification.FileUrl,
                                         CreateTimeStamp = notification.CreateTimeStamp,
                                     })
                      .Skip((pageNumber - 1) * pageSize)
                      .Take(pageSize)
                      .ToList();

                //var notifications = from _oMTDataContext.Notification
                //    .OrderByDescending(n => n.CreateTimeStamp)
                //    .Skip((pageNumber - 1) * pageSize)
                //    .Take(pageSize)
                //    .Select(n => new
                //    {
                //        Id = n.Id,
                //        UserId = n.UserId,
                //        NotificationMessage = n.NotificationMessage,
                //        FileUrl = n.FileUrl,
                //        CreateTimeStamp = n.CreateTimeStamp
                //    })
                //    .ToList();

                resultDTO.IsSuccess = true;
                resultDTO.Message = notifications.Any()
                        ? "Notifications successfully retrieved based on the specified page parameters."
                        : "No notifications found.";
                resultDTO.Data = new { 
                    Notifications = notifications, 
                    TotalCount = totalCount
                };
                    resultDTO.StatusCode = "200";



            }
            catch (Exception ex)
            {

                resultDTO.IsSuccess = false;
                resultDTO.StatusCode = "500";
                resultDTO.Message = $"An unexpected error occurred: {ex.Message}";

            }

            return resultDTO;
        }


        public ResultDTO DownloadFile(int Id)
        {
            ResultDTO resultDTO = new ResultDTO { IsSuccess = true, StatusCode = "200" };

            try
            {
                var existNotification = _oMTDataContext.Notification.Where(x => x.Id == Id).FirstOrDefault();
                var BlobDownload = new AzureBlob();
                string message = BlobDownload.DownloadBlobUsingSasToken(_options.Value.ConnectionString, _options.Value.Container, existNotification.FileUrl, _options.Value.DownloadFilePath);
                resultDTO.IsSuccess = true;
                resultDTO.Message = message;
                resultDTO.StatusCode = "200";
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
