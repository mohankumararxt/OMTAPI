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
    public class OrderDecisionController : BaseController
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
