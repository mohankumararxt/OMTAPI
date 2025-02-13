using Microsoft.AspNetCore.Http;
using OMT.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DataService.Interface
{
    public interface INotificationService
    {
        Task<ResultDTO> CreateNotificationAsync(NotificationDTO notificationDTO, IFormFile file);
        Task<ResultDTO> FetchFiveRecentNotificationsAsync();

        Task<ResultDTO> GetAllNotificationsAsync(int pageNumber = 1, int pageSize = 10);
        Task<ResultDTO> DownloadFileAsync(int Id);
        Task<ResultDTO> DeleteNotificationAsync(int notificationId);

    }
}