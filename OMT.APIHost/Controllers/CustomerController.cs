using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OMT.DataService.Interface;
using OMT.DataService.Service;
using OMT.DTO;

namespace OMT.APIHost.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerService _customerService;
        public CustomerController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        [HttpGet]
        [Route("GetCustomerList")]

        public ResultDTO GetCustomerList()
        {
            return _customerService.GetCustomerList();
        }

        //[HttpDelete]
        //[Route("DeleteCustomer/{customerId:int}")]

        //public ResultDTO DeleteCustomer(int customerId)
        //{
        //    return _customerService.DeleteCustomer(customerId);
        //}

        [HttpPost]
        [Route("CreateCustomer")]
        public ResultDTO CreateCustomer([FromBody] string customername)
        {
            return _customerService.CreateCustomer(customername);
        }

        [HttpPut]
        [Route("UpdateCustomer")]

        public ResultDTO UpdateCustomer([FromBody] CustomerDTO customerDTO)
        {
            return _customerService.UpdateCustomer(customerDTO);
        }
    }
}
