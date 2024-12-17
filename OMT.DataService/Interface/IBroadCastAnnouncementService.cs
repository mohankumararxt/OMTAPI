using OMT.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DataService.Interface
{
    public interface IBroadCastAnnouncementService
    {
        ResultDTO GetBroadCastAnnouncementMessages();

        ResultDTO GetBroadCastAnnouncementBySoftDelete();
        ResultDTO CreateNewBroadCastMessages(BroadCastAnnouncementRequestDTO broadCastAnnouncementRequestDTO);
        ResultDTO FilterBroadCastWithStartDateAndEndDate();
        ResultDTO UpdateSoftDeleteFlag(int Id);
    }
}
