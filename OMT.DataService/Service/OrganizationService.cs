using OMT.DataAccess.Context;
using OMT.DataAccess.Entities;
using OMT.DataService.Interface;
using OMT.DTO;

namespace OMT.DataService.Service
{
    public class OrganizationService : IOrganizationService
    {
        private readonly OMTDataContext _context;

        public OrganizationService(OMTDataContext context)
        {
            _context = context;
        }

        public ResultDTO CreateOrganization(NewOrganizationDTO createOrganizationDTO)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "201" };

            try
            {
                string existingOrganizationName = _context.Organization.Where(x => x.OrganizationName == createOrganizationDTO.OrganizationName && x.IsActive).Select(_ => _.OrganizationName).FirstOrDefault();
                if (existingOrganizationName != null)
                {
                    resultDTO.IsSuccess = false;
                    resultDTO.Message = "The Organization Name already exists. Please try to create other Organization.";
                }
                else
                {
                    Organization OrgObj = new Organization()
                    {
                        OrganizationName = createOrganizationDTO.OrganizationName,
                        Description = createOrganizationDTO.Description,
                        CreatedDate = DateTime.Now,
                        IsActive = true
                    };

                    _context.Organization.Add(OrgObj);
                    _context.SaveChanges();
                    resultDTO.Message = "Organization Created Successfully.";
                    resultDTO.Data = OrgObj;
                    resultDTO.IsSuccess = true;
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

        public ResultDTO GetAllOrganizations()
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
                List<OrganizationListDTO> orgDTOObj = _context.Organization.Where(x=>x.IsActive).Select(_ => new OrganizationListDTO() {  OrganizationId=_.OrganizationId, Description=_.Description,OrganizationName=_.OrganizationName } ) .ToList();
                if (orgDTOObj != null)
                {
                    resultDTO.IsSuccess = true;
                    resultDTO.Data = orgDTOObj;
                    resultDTO.Message = "Organization Details List fetched Successfully.";
                }
                else
                {
                    resultDTO.IsSuccess = true;
                    resultDTO.Data = orgDTOObj;
                    resultDTO.Message = "Oraganization object is empty";
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

        public ResultDTO GetOrganizationBYId(int OrganizationId)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
                Organization? organizationObj = _context.Organization.Where(x=> x.OrganizationId==OrganizationId && x.IsActive).FirstOrDefault();
                if (organizationObj != null)
                {
                    resultDTO.IsSuccess = true;
                    resultDTO.Data = organizationObj;
                    resultDTO.Message = "Organization Details fecthed Successfully.";
                }
                else
                {
                    resultDTO.IsSuccess = false;
                    resultDTO.Data = organizationObj;
                    resultDTO.Message = $"Oraganization not found";
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
