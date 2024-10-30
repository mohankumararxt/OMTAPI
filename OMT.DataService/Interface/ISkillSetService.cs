using OMT.DTO;

namespace OMT.DataService.Interface
{
    public interface ISkillSetService
    {
        ResultDTO GetSkillSetList(int? skillsetid);
        ResultDTO GetSkillSetListBySORId(int sorid);
        ResultDTO CreateSkillSet(SkillSetCreateDTO skillSetCreateDTO);
        ResultDTO DeleteSkillSet(int skillsetId);
        ResultDTO UpdateSkillSet(SkillSetUpdateDTO skillSetUpdateDTO); 
        ResultDTO GetStatenameList(int skillsetid);
        ResultDTO CreateTimeLine(SkillSetTimeLineDTO skillsettimeLineDTO); 
        ResultDTO UpdateTimeLine(SkillSetUpdateTimeLineDTO skillSetUpdateTimeLineDTO); 
        ResultDTO GetSkillSetTimelineList(int? skillsetid); 
       
    } 
}
