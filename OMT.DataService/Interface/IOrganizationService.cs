using OMT.DTO;

namespace OMT.DataService.Interface
{
    public interface IOrganizationService
    {
        ResultDTO CreateOrganization(NewOrganizationDTO createOrganizationDTO);
        ResultDTO GetAllOrganizations();
        ResultDTO GetOrganizationBYId(int OrganizationId);
    }
}
