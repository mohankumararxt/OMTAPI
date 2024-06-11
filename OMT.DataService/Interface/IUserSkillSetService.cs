using OMT.DTO;

namespace OMT.DataService.Interface
{
    public interface IUserSkillSetService
    {
        ResultDTO GetUserSkillSetList(int? userid);
        ResultDTO AddUserSkillSet(UserSkillSetCreateDTO userSkillSetCreateDTO);
        ResultDTO DeleteUserSkillSet(int userskillsetId);
        ResultDTO UpdateUserSkillSet(UserSkillSetUpdateDTO userSkillSetUpdateDTO);
        ResultDTO UpdateUserSkillsetList(UpdateUserSkillsetListDTO updateUserSkillsetListDTO);
        ResultDTO BulkUpdate(BulkUserSkillsetUpdateDTO bulkUserSkillsetUpdateDTO);
    }
}
