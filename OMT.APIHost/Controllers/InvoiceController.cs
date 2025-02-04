using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OMT.DataService.Interface;
using OMT.DTO;

namespace OMT.APIHost.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class InvoiceController : ControllerBase
    {
        private readonly IInvoiceService _invoiceService;

        public InvoiceController(IInvoiceService invoiceService)
        {
            _invoiceService = invoiceService;
        }

        [HttpPost]
        [Route("GetInvoice")]

        public ResultDTO GetInvoice([FromBody] GetInvoiceDTO getinvoiceDTO)
        {
            return _invoiceService.GetInvoice(getinvoiceDTO);
        }

        [HttpGet]
        [Route("GetInvoiceSkillSetList/{sorid:int}")]

        public ResultDTO GetInvoiceSkillSetList(int sorid)
        {
            return _invoiceService.GetInvoiceSkillSetList(sorid);
        }
    }
}
