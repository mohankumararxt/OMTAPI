using OMT.DataAccess.Context;
using OMT.DataService.Interface;
using OMT.DataService.Utility;
using OMT.DTO;

namespace OMT.DataService.Service
{
    public class LoginService : ILoginService
    {
        private readonly OMTDataContext _oMTDataContext;
        public LoginService(OMTDataContext oMTDataContext)
        {
            _oMTDataContext = oMTDataContext;
        }
        public ResultDTO Login(LoginRequestDTO loginRequestDTO)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };

            try
            {
                string encryptedPassword = Encryption.EncryptPlainTextToCipherText(loginRequestDTO.Password);
                LoginResponseDTO loginResponseDTO = (from up in _oMTDataContext.UserProfile.Where(x => x.Email == loginRequestDTO.Email && x.Password == encryptedPassword)
                                                    join r in _oMTDataContext.Roles on up.RoleId equals r.RoleId
                                                    select new LoginResponseDTO()
                                                    {
                                                        Email = up.Email,
                                                        FirstName = up.FirstName,
                                                        Mobile = up.Mobile,
                                                        OrganizationId = up.OrganizationId,
                                                        UserId = up.UserId,
                                                        RoleId = up.RoleId,
                                                        Role = r.RoleName
                                                    }).FirstOrDefault();

                if (loginResponseDTO != null)
                {
                    resultDTO.Data = loginResponseDTO;
                }
                else
                {
                    resultDTO.Message = "Login Failed. Please check your credentials.";
                    resultDTO.IsSuccess = false;
                }
            }
            catch (Exception ex)
            {
                resultDTO.IsSuccess = false;
                resultDTO.StatusCode = "500";
                resultDTO.Message = ex.Message;
            }
            return resultDTO;
        }
    }
}
