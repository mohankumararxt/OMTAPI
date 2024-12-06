using OMT.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DataService.Interface
{
    public interface IDashboardSkillsetReportService
    {
        ResultDTO DashboardSkillsetWiseReports(DashboardSkillsetReportsDTO skillsetWiseReportsDTO);

        ResultDTO DashboardReports(DateTime fromDate, DateTime toDate);
    }
}
