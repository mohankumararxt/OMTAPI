using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OMT.DataService.Interface;
using OMT.DTO;
using System.Drawing.Printing;

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

        [HttpPost]
        [Route("CreateNotification")]
        public ResultDTO CreateNotification(NotificationDTO notificationDTO)
        {
            return _notificationService.CreateNotification(notificationDTO);
        }

        [HttpGet]
        [Route("FetchFiveRecentNotifications")]
        public ResultDTO FetchFiveRecentNotifications()
        {
            return _notificationService.FetchFiveRecentNotifications();
        }


        [HttpGet]
        [Route("GetAllNotifications")]
        public ResultDTO GetAllNotifications([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            return _notificationService.GetAllNotifications(pageNumber, pageSize);
        }

        [HttpGet]
        [Route("DownloadFile")]
        public ResultDTO DownloadFile(int Id)
        {
            return _notificationService.DownloadFile(Id);
        }
    }
}
