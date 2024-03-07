using OMT.DataAccess.Context;
using OMT.DataAccess.Entities;
using OMT.DataService.Interface;
using OMT.DTO;

namespace OMT.DataService.Service
{
    public class RolesService : IRolesService
    {
        public readonly OMTDataContext _oMTDataContext;

        public RolesService(OMTDataContext oMTDataContext)
        {
            _oMTDataContext = oMTDataContext;
        }

        public ResultDTO CreateRoles(string rolename)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
                var existing_role = _oMTDataContext.Roles.Where(x => x.RoleName == rolename).FirstOrDefault();
                if (existing_role != null)
                {
                    resultDTO.IsSuccess = false;
                    resultDTO.Message = "Role already exists. Please try to add different Role";
                }
                else
                {
                    Roles roles = new Roles()
                    { 
                        RoleName = rolename,
                        IsActive = true,
                    };
                    _oMTDataContext.Roles.Add(roles);
                    _oMTDataContext.SaveChanges();
                    resultDTO.IsSuccess = true;
                    resultDTO.Message = "Role created successfully";
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

        public ResultDTO DeleteRoles(int roleid)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
                Roles? roles = _oMTDataContext.Roles.Where(x => x.RoleId == roleid && x.IsActive).FirstOrDefault();
                if(roles != null)
                {
                    roles.IsActive = false;
                    _oMTDataContext.Roles.Update(roles);
                    _oMTDataContext.SaveChanges();
                    resultDTO.IsSuccess = true;
                    resultDTO.Message = "Role has been deleted successfully";
                }
                else
                {
                    resultDTO.StatusCode = "404";
                    resultDTO.IsSuccess = false;
                    resultDTO.Message = "Role is not found";
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

        public ResultDTO GetRoles()
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
                List<RolesResponseDTO> ListofRoles = _oMTDataContext.Roles.Where(x => x.IsActive).Select(_ => new RolesResponseDTO() { RoleId = _.RoleId, RoleName = _.RoleName }).ToList();
                resultDTO.IsSuccess = true;
                resultDTO.Message = "List of Roles";
                resultDTO.Data = ListofRoles;
            }
            catch (Exception ex)
            {
                resultDTO.IsSuccess = false;
                resultDTO.StatusCode = "500";
                resultDTO.Message = ex.Message;
            }
            return resultDTO;
        }

        public ResultDTO UpdateRoles(RolesResponseDTO rolesResponseDTO)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "201" };
            try
            {
                Roles? roles = _oMTDataContext.Roles.Find(rolesResponseDTO.RoleId);
                if (roles != null)
                {
                    roles.RoleName = rolesResponseDTO.RoleName;
                    _oMTDataContext.Roles.Update(roles);
                    _oMTDataContext.SaveChanges();
                    resultDTO.IsSuccess=true;
                    resultDTO.Message = "Role updated successfully";
                    resultDTO.Data = roles;
                }
                else
                {
                    resultDTO.IsSuccess = false;
                    resultDTO.Message = "Role not found";
                    resultDTO.StatusCode = "404";
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
