using OMT.DTO;

namespace OMT.DataService.Interface
{
    public interface ILoginService
    {
        ResultDTO Login(LoginRequestDTO loginRequestDTO);
    }
}
