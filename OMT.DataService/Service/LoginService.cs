using OMT.DataAccess.Context;
using OMT.DataAccess.Entities;
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

                LoginResponseDTO loginResponseDTO = (from up in _oMTDataContext.UserProfile.Where(x => x.Email == loginRequestDTO.Email && x.Password == encryptedPassword && x.IsActive)
                                                     join r in _oMTDataContext.Roles on up.RoleId equals r.RoleId
                                                     select new LoginResponseDTO()
                                                     {
                                                         Email = up.Email,
                                                         FirstName = up.FirstName,
                                                         Mobile = up.Mobile,
                                                         OrganizationId = up.OrganizationId,
                                                         UserId = up.UserId,
                                                         RoleId = up.RoleId,
                                                         Role = r.RoleName,
                                                         Checked_In = _oMTDataContext.User_Checkin
                                                        .Any(uc => uc.UserId == up.UserId && uc.Checkin != null && uc.Checkout == null)
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

        public ResultDTO Checkin(CheckinoutRequestDTO checkinRequestDTO)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };

            try
            {
                User_Checkin user_Checkin = _oMTDataContext.User_Checkin
                                                           .Where(x => x.UserId == checkinRequestDTO.UserId)
                                                           .OrderByDescending(x => x.Id)
                                                           .FirstOrDefault();

                if (user_Checkin != null)
                {

                    if (user_Checkin.Checkin != null && user_Checkin.Checkout == null)
                    {
                        user_Checkin.Checkout = checkinRequestDTO.DateTime;
                        _oMTDataContext.User_Checkin.Update(user_Checkin);
                    }
                    else if (user_Checkin.Checkin != null && user_Checkin.Checkout != null)
                    {
                        User_Checkin user_time = new User_Checkin()
                        {
                            UserId = checkinRequestDTO.UserId,
                            Checkin = checkinRequestDTO.DateTime,
                        };

                        _oMTDataContext.User_Checkin.Add(user_time);
                    }

                }
                else
                {
                    User_Checkin user_time = new User_Checkin()
                    {
                        UserId = checkinRequestDTO.UserId,
                        Checkin = checkinRequestDTO.DateTime,

                    };

                    _oMTDataContext.User_Checkin.Add(user_time);

                }

                _oMTDataContext.SaveChanges();

                CheckinResponseDTO responseDTO = new CheckinResponseDTO()
                {
                    Checked_In = _oMTDataContext.User_Checkin.Any(uc => uc.UserId == checkinRequestDTO.UserId && uc.Checkin != null && uc.Checkout == null)
                };

                resultDTO.Data = responseDTO;
                resultDTO.Message = "Checkin / checkout successful";
                resultDTO.IsSuccess = true;
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
