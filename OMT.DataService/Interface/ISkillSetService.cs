using OMT.DTO;

namespace OMT.DataService.Interface
{
    public interface ISkillSetService
    {
        ResultDTO GetSkillSetList();
        ResultDTO GetSkillSetListById(int skillsetId);
        ResultDTO CreateSkillSet(SkillSetCreateDTO skillSetCreateDTO);
        ResultDTO DeleteSkillSet(int skillsetId);
        ResultDTO UpdateSkillSet(SkillSetResponseDTO skillSetResponseDTO);

    }
}
