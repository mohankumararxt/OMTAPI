using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OMT.DataService.Interface;
using OMT.DTO;

namespace OMT.APIHost.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class InvoiceJointReswareController : ControllerBase
    {
        private readonly IInvoiceJointReswareService _invoiceJointReswareService;

        public InvoiceJointReswareController(IInvoiceJointReswareService invoiceJointReswareService)
        {
            _invoiceJointReswareService = invoiceJointReswareService;
        }

        [HttpGet]
        [Route("get")]
        public ResultDTO GetInvoiceJointResware()
        {
            return _invoiceJointReswareService.GetInvoiceJointResware();
        }

        [HttpPost]
        [Route("new")]
        public ResultDTO CreateInvoiceJointResware([FromBody]InvoiceJointReswareCreateDTO invoiceJointReswareCreateDTO)
        {
            return _invoiceJointReswareService.CreateInvoiceJointResware(invoiceJointReswareCreateDTO);
        }

        [HttpPut]
        [Route("update")]
        public ResultDTO UpdateInvoiceJointResware([FromBody] InvoiceJointReswareUpdateDTO invoiceJointReswareUpdateDTO)
        {
            return _invoiceJointReswareService.UpdateInvoiceJointResware(invoiceJointReswareUpdateDTO);
        }
    }
}
