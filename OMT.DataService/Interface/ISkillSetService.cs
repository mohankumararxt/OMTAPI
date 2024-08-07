﻿using OMT.DTO;

namespace OMT.DataService.Interface
{
    public interface ISkillSetService
    {
        ResultDTO GetSkillSetList();
        ResultDTO GetSkillSetListBySORId(int sorid);
        ResultDTO CreateSkillSet(SkillSetCreateDTO skillSetCreateDTO);
        ResultDTO DeleteSkillSet(int skillsetId);
        ResultDTO UpdateSkillSet(SkillSetResponseDTO skillSetResponseDTO);
        ResultDTO GetStatenameList(int skillsetid);

    }
}
