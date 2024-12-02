using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using OMT.DTO;

namespace OMT.DataService.Interface
{
    public interface IReportColumnsService
    {
        ResultDTO GetReportColumnlist(int? skillsetid);
        ResultDTO CreateReportColumns(CreateReportColumnsDTO createReportColumnsDTO);
    }
}