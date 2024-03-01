using OMT.DTO;

namespace OMT.DataService.Interface
{
    public interface IUserSkillSetService
    {
        ResultDTO GetUserSkillSetList(int userid);
        ResultDTO AddUserSkillSet(int skillsetid, int userid);
        ResultDTO DeleteUserSkillSet(int userskillsetId);
        ResultDTO UpdateUserSkillSet(UserSkillSetResponseDTO userskillSetResponseDTO);
    }
}
