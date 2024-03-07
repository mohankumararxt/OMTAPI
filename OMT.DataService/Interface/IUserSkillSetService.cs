using OMT.DTO;

namespace OMT.DataService.Interface
{
    public interface IUserSkillSetService
    {
        ResultDTO GetUserSkillSetList(int userid);
        ResultDTO AddUserSkillSet(UserSkillSetCreateDTO userSkillSetCreateDTO, int userid);
        ResultDTO DeleteUserSkillSet(int userskillsetId);
        ResultDTO UpdateUserSkillSet(UserSkillSetUpdateDTO userSkillSetUpdateDTO, int userid);
    }
}
