using OMT.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DataService.Interface
{
    public interface ISkillSetService
    {
        ResultDTO GetSkillSetList();
        ResultDTO CreateSkillSet(SkillSetCreateDTO skillSetCreateDTO);
        ResultDTO DeleteSkillSet(int skillsetId);
        ResultDTO UpdateSkillSet(SkillSetResponseDTO skillSetResponseDTO);

    }
}
