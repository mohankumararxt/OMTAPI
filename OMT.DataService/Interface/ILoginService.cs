using OMT.DTO;

namespace OMT.DataService.Interface
{
    public interface ILoginService
    {
        ResultDTO Login(LoginRequestDTO loginRequestDTO);
        ResultDTO Checkin(CheckinoutRequestDTO checkinRequestDTO);
        ResultDTO ForgotPassword(ForgotPasswordRequestDTO forgotPasswordRequestDTO);
        ResultDTO ValidateGuId(ValidateGuIdDTO validateGuIdDTO);
        ResultDTO ResetPassword(ResetPasswordDTO resetPasswordDTO);
    }
}
