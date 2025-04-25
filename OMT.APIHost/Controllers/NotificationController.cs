using Microsoft.AspNetCore.Mvc;
using OMT.DataService.Interface;
using OMT.DTO;
using System.Threading.Tasks;

namespace OMT.APIHost.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        /// <summary>
        /// Creates a new notification with an attached file.
        /// </summary>
        /// <param name="notificationDTO">Notification data transfer object.</param>
        /// <param name="file">The file to be uploaded.</param>
        /// <returns>Result indicating the outcome of the operation.</returns>
        [HttpPost]
        [Route("CreateNotification")]
        public async Task<ResultDTO> CreateNotification([FromForm] NotificationDTO notificationDTO, [FromForm] IFormFile file)
        {
            // Call the service method to handle the creation of the notification
            var result = await _notificationService.CreateNotificationAsync(notificationDTO, file);
            return result;
        }

        /// <summary>
        /// Fetches the five most recent notifications.
        /// </summary>
        /// <returns>Result containing the recent notifications.</returns>
        [HttpGet]
        [Route("FetchFiveRecentNotifications")]
        public async Task<ResultDTO> FetchFiveRecentNotifications()
        {
            var result = await _notificationService.FetchFiveRecentNotificationsAsync();
            return result;
        }

        /// <summary>
        /// Fetches paginated notifications.
        /// </summary>
        /// <param name="pageNumber">Page number for pagination.</param>
        /// <param name="pageSize">Number of notifications per page.</param>
        /// <returns>Result containing paginated notifications.</returns>
        [HttpGet]
        [Route("GetAllNotifications")]
        public async Task<ResultDTO> GetAllNotifications([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _notificationService.GetAllNotificationsAsync(pageNumber, pageSize);
            return result;
        }

        /// <summary>
        /// Generates a SAS URL for downloading a file associated with a notification.
        /// </summary>
        /// <param name="id">Notification ID.</param>
        /// <returns>Result containing the SAS URL.</returns>
        [HttpGet]
        [Route("DownloadFile")]
        public async Task<ResultDTO> DownloadFile([FromQuery] int id)
        {
            var result = await _notificationService.DownloadFileAsync(id);
            return result;
        }

        [HttpDelete]
        [Route("DeleteNotification")]
        public async Task<ResultDTO> DeleteNotification([FromQuery] int notificationId)
        {
            var result = await _notificationService.DeleteNotificationAsync(notificationId);
            return result;
        }
    }
}
