using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using OMT.DataService.Interface;
using OMT.DTO;

namespace OMT.APIHost.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ShiftDetailsController : BaseController
    {
        private readonly IShiftDetailsService _shiftDetailsService;

        public ShiftDetailsController(IShiftDetailsService shiftDetailsService)
        {
            _shiftDetailsService = shiftDetailsService;
        }

        [HttpPost]
        [Route("CreateShiftDetails")]

        public ResultDTO CreateShiftDetails(ShiftDetailsDTO shiftDetailsDTO)
        {
            var userid = UserId;
            return _shiftDetailsService.CreateShiftDetails(shiftDetailsDTO, userid);
        }

        [HttpGet]
        [Route("GetShiftDetails")]

        public ResultDTO GetShiftDetails()
        {
            return _shiftDetailsService.GetShiftDetails();
        }

        [HttpDelete]
        [Route("DeleteShiftDetails/{shiftcodeid:int}")]

        public ResultDTO DeleteShiftDetails(int ShiftCodeId)
        {
            return _shiftDetailsService.DeleteShiftDetails(ShiftCodeId);
        }

        [HttpPut]
        [Route("UpdateShiftDetails")]

        public ResultDTO UpdateShiftDetails(EditShiftDetailsDTO editShiftDetailsDTO)
        {
            var userid = UserId;
            return _shiftDetailsService.UpdateShiftDetails(editShiftDetailsDTO, userid);
        }

        [HttpPost]
        [Route("CreateShiftAssociation")]
        public ResultDTO CreateShiftAssociation(CreateShiftAssociationDTO createShiftAssociationDTO)
        {
            var userid = UserId;
            return _shiftDetailsService.CreateShiftAssociation(createShiftAssociationDTO,userid);
        }

        [HttpPost]
        [Route("GetShiftAssociation")]

        public ResultDTO GetShiftAssociation(GetShiftAssociationDTO getShiftAssociation)
        {
            return _shiftDetailsService.GetShiftAssociation(getShiftAssociation);
        }

        [HttpPut]
        [Route("UpdateShiftAssociation")]

        public ResultDTO UpdateShiftAssociation(UpdateShiftAssociationDTO updateShiftAssociationDTO)
        {
            var userid = UserId;
            return _shiftDetailsService.UpdateShiftAssociation(updateShiftAssociationDTO,userid);
        }

        [HttpDelete]
        [Route("DeleteShiftAssociation/{ShiftAssociationId:int}")]

        public ResultDTO DeleteShiftAssociation(int ShiftAssociationId)
        {
            return _shiftDetailsService.DeleteShiftAssociation(ShiftAssociationId);
        }

        [HttpPost]
        [Route("UploadShiftAssociationDetails")]

        public ResultDTO UploadShiftAssociationDetails(UploadShiftAssociationDetailsDTO uploadShiftAssociationDetailsDTO)
        {

            var userid = UserId;
            return _shiftDetailsService.UploadShiftAssociationDetails(uploadShiftAssociationDetailsDTO,userid);
        }

        [HttpPost]
        [Route("DownloadShiftDetailsTemplate")]

        public ResultDTO DownloadShiftDetailsTemplate(DownloadShiftDetailsTemplateDTO downloadShiftDetailsTemplateDTO)
        {
            return _shiftDetailsService.DownloadShiftDetailsTemplate(downloadShiftDetailsTemplateDTO);
        }

    }
}
