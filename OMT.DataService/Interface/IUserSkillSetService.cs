﻿using OMT.DTO;

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
        ResultDTO CreateMultipleUserSkillset(MultipleUserSkillSetCreateDTO multipleUserSkillSetCreateDTO);
        ResultDTO ConsolidatedUserSkillSetlist(int? userid);
        ResultDTO UpdateUserSkillSetThWt(UpdateUserSkillSetThWtDTO updateUserSkillSetThWtDTO);
    }
}
