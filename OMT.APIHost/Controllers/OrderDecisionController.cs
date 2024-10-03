using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OMT.DataService.Interface;
using OMT.DTO;

namespace OMT.APIHost.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderDecisionController : ControllerBase
    {
        private readonly IOrderDecisionService _getordercalculationtavbleservice;

        public OrderDecisionController(IOrderDecisionService getordercalculationtavbleservice)
        {
            _getordercalculationtavbleservice = getordercalculationtavbleservice;
        }

        [HttpPost]
        [Route("add")]

        public ResultDTO UpdateGetOrderCalculation()
        {
            return  _getordercalculationtavbleservice.UpdateGetOrderCalculation();
        }

    }
}
