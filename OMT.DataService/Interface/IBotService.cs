using OMT.DTO;

namespace OMT.DataService.Interface
{
    public interface IBotService
    {
       // Task<string> CallCandidateApiAsync(string name);

        ResultDTO Update_Checkindate_Bot(UpdateCheckindateBotRequestDTO updateCheckindateBotRequestDTO);

        ResultDTO Retrieve_SystemPending_Orders(RetrieveSystemPendingOrdersRequsetDTO retrieveSystemPendingOrdersRequsetDTO);
        ResultDTO GetSORList();
        ResultDTO GetSkillSetListBySORId(int sorid);

    }
}
