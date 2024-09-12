using OMT.DTO;

namespace OMT.DataService.Interface
{
    public interface IUserService
    {
        ResultDTO CreateUser(CreateUserDTO createUserDTO);
        ResultDTO GetUserList();
        ResultDTO DeleteUser(int userid);
        ResultDTO UpdateByHR(UpdateUserByHrDTO updateUserDTO);

    }
}
