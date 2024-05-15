using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OMT.DataService.Interface;
using OMT.DTO;

namespace OMT.APIHost.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrganizationController : ControllerBase
    {
        private readonly IOrganizationService _organizationService;

        public OrganizationController(IOrganizationService organizationService)
        {
            _organizationService = organizationService;
        }

        [HttpPost]
        [Route("new")]
        public ResultDTO CreateOrganization([FromBody] NewOrganizationDTO newOrganizationDTO)
        {
            return _organizationService.CreateOrganization(newOrganizationDTO);
        }

        [HttpGet]
        public ResultDTO GetAllOrganizations()
        {
            return _organizationService.GetAllOrganizations();
        }

        [HttpGet("{orgId}")]
        public ResultDTO GetOrganizationBYId(int orgId)
        {
            return _organizationService.GetOrganizationBYId(orgId);
        }
    }
}
