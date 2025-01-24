using OMT.DTO;

namespace OMT.DataService.Interface
{
    public interface IUserService
    {

        ResultDTO CreateUser(CreateUserDTO createUserDTO);
        ResultDTO GetUserList();
        ResultDTO GetuserInfoByUserId(int userid); 
        ResultDTO DeleteUser(int userid);
        ResultDTO UpdatePasswordByUser(UpdatePasswordByUserDTO updatePasswordByUserDTO); 
        ResultDTO UpdateByHR(UpdateUserByHrDTO updateUserDTO);
        ResultDTO UpdatePasswordByHR(UpdateUserPasswordByHrDTO updateUserPasswordDTO);

        ResultDTO GetSocketIoUserList();
    }
}