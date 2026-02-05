using OMT.DTO;

namespace OMT.DataService.Interface
{
    public interface IBotService
    {
        ResultDTO Update_Checkindate_Bot(UpdateCheckindateBotRequestDTO updateCheckindateBotRequestDTO);
        ResultDTO Retrieve_SystemPending_Orders(RetrieveSystemPendingOrdersRequsetDTO retrieveSystemPendingOrdersRequsetDTO);
    }
}
