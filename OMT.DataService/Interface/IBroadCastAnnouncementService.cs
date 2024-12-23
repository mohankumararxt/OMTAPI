using OMT.DTO;
using System.Threading.Tasks;

namespace OMT.DataService.Interface
{
    public interface IBroadCastAnnouncementService
    {
        Task<ResultDTO> GetBroadCastAnnouncements();
        Task<ResultDTO> CreateNewBroadCastMessages(BroadCastAnnouncementRequestDTO broadCastAnnouncementRequestDTO);
        Task<ResultDTO> FilterBroadCastWithStartDateAndEndDate();
        Task<ResultDTO> UpdateSoftDeleteFlag(int Id);
    }
}
