using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DTO
{
    public class UserSkillsetCombinedDTO
    {
        public List<GetUserProfileInfoDTO> UserInfo { get; set; }
        public List<UserSkillSetResponseDTO> UserSkillSet { get; set; }
    }
}
