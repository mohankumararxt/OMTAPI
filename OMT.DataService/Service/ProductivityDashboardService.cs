using OMT.DataAccess.Context;
using OMT.DataService.Interface;
using OMT.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DataService.Service
{
    public class ProductivityDashboardService : IProductivityDashboardService
    {
        private readonly OMTDataContext _oMTDataContext;

        public ResultDTO GetTeamProductivity()
        {
            throw new NotImplementedException();
        }
    }
}
