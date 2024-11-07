using OMT.DataAccess.Context;
using OMT.DataAccess.Entities;
using OMT.DataService.Interface;
using OMT.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DataService.Service
{
    public class ReportColumnsService : IReportColumnsService
    {
        private readonly OMTDataContext _oMTDataContext;

        public ReportColumnsService(OMTDataContext oMTDataContext)
        {
            _oMTDataContext = oMTDataContext;
        }
    }
}
