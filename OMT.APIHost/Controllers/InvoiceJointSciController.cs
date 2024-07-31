using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OMT.DataService.Interface;
using OMT.DTO;

namespace OMT.APIHost.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class InvoiceJointSciController : ControllerBase
    {
        private readonly IInvoiceJointSciService _invoiceJointSciService;

        public InvoiceJointSciController(IInvoiceJointSciService invoiceJointSciService)
        {
            _invoiceJointSciService = invoiceJointSciService;
        }

        [HttpPost]
        [Route("new")]
        public ResultDTO CreateInvoiceJointSci([FromBody] InvoiceJointSciCreateDTO invoiceJointSciCreateDTO)
        {
            return _invoiceJointSciService.CreateInvoiceJointSci(invoiceJointSciCreateDTO);
        }

        [HttpGet]
        [Route("get")]
        public ResultDTO GetInvoiceJointSci()
        {
            return _invoiceJointSciService.GetInvoiceJointSci();
        }

        [HttpPut]
        [Route("update")]
        public ResultDTO UpdateInvoiceJointSci([FromBody] InvoiceJointSciResponseDTO invoiceJointSciResponseDTO)
        {
            return _invoiceJointSciService.UpdateInvoiceJointSci(invoiceJointSciResponseDTO);
        }

    }
}
