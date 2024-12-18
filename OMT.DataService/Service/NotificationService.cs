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

namespace OMT.DataService.Service
{
    public class NotificationService: INotificationService
    {
        private const long MaxFileSize = 100 * 1024 * 1024; // 100 MB

        private readonly OMTDataContext _oMTDataContext;
        public NotificationService(OMTDataContext oMTDataContext)
        {
            _oMTDataContext = oMTDataContext;
        }

        public ResultDTO CreateNotification(NotificationDTO notificationDTO)
        {

            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };

            try
            {
                // Validate notification message
                if (string.IsNullOrWhiteSpace(notificationDTO.NotificationMessage))
                {

                    resultDTO.IsSuccess = false;
                    resultDTO.StatusCode = "400";
                    resultDTO.Message = "The 'NotificationMessage' field is required.";

                }

                string filePath = null;
                if (notificationDTO.FileUrl != null)
                {
                    if (notificationDTO.FileUrl.Length > MaxFileSize)
                    {

                        resultDTO.IsSuccess = false;
                        resultDTO.StatusCode = "400";
                        resultDTO.Message = "File size exceeds the 100 MB limit.";

                    }

                    try
                    {
                        var uploadDirectory = Path.Combine("UploadedFiles");
                        Directory.CreateDirectory(uploadDirectory); // Ensure directory exists

                        filePath = Path.Combine(uploadDirectory, notificationDTO.FileUrl.FileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                             notificationDTO.FileUrl.CopyToAsync(stream);
                        }
                    }
                    catch (Exception ex)
                    {

                        resultDTO.IsSuccess = false;
                        resultDTO.StatusCode = "500";
                        resultDTO.Message = $"Failed to upload the file. Error: {ex.Message}";

                    }
                }

                // Store notification message and file URL in the database
                try
                {
                    // Assume you have a database context called `_dbContext`
                    var notification = new Notification
                    {
                        NotificationMessage = notificationDTO.NotificationMessage,
                        FileUrl = filePath, // Save the file path in the database
                    };

                    _oMTDataContext.Notification.Add(notification);
                     _oMTDataContext.SaveChangesAsync(); // Save changes to the database
                }
                catch (Exception ex)
                {
                    return new ResultDTO
                    {
                        IsSuccess = false,
                        StatusCode = "400",
                        Message = $"Failed to save notification to the database. Error: {ex.Message}"
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResultDTO
                {
                    IsSuccess = false,
                    StatusCode = "500",
                    Message = $"An unexpected error occurred: {ex.Message}"
                };
            }

            return resultDTO;
        }
    }
}
