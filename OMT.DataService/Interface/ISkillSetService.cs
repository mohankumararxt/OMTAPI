using OMT.DTO;

namespace OMT.DataService.Interface
{
    public interface ISkillSetService
    {
        ResultDTO GetSkillSetList(int? skillsetid);
        ResultDTO GetSkillSetListBySORId(int sorid);
        ResultDTO CreateSkillSet(SkillSetCreateDTO skillSetCreateDTO,int userid);
        ResultDTO DeleteSkillSet(int skillsetId,int userid);
        ResultDTO UpdateSkillSet(SkillSetUpdateDTO skillSetUpdateDTO,int userid); 
        ResultDTO GetStatenameList(int skillsetid);
        ResultDTO CreateTimeLine(SkillSetTimeLineDTO skillsettimeLineDTO); 
        ResultDTO UpdateTimeLine(SkillSetUpdateTimeLineDTO skillSetUpdateTimeLineDTO); 
        ResultDTO GetSkillSetTimelineList(int? skillsetid); 
       
    } 
}