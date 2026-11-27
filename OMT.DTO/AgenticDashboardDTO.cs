using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DTO
{
    public class AgenticDashboardDTO
    {
        public OrderCompletionDTO totalOrderCompletion { get; set; }
        public List<SORWiseOrderCompletionDTO> SORWiseOrderCompletion { get; set; }
        public List<SORWiseRushOrderCompletionDTO> SORWiseRushOrderCompletion { get; set; }
        public LiveUserSDTO liveUsers { get; set; }
        public InvoiceForecastDTO invoiceForecastDTO { get; set; }
        public HardStateDTO hardStateDTO { get; set; }
        public AgenticDashboardDTO()
        {
            totalOrderCompletion = new OrderCompletionDTO();
            SORWiseOrderCompletion=new List<SORWiseOrderCompletionDTO>();
            SORWiseRushOrderCompletion = new List<SORWiseRushOrderCompletionDTO>();
            liveUsers = new LiveUserSDTO();
            invoiceForecastDTO = new InvoiceForecastDTO();
            hardStateDTO=new HardStateDTO();
        }
    }

    public class OrderCompletionDTO
    {
        public int YesterdayOrdersCount { get; set; }
        public int TodaysOrdersCount { get; set; }
        public int TodaysOrdersCompletedCount { get; set; }
    }

    public class SORWiseOrderCompletionDTO
    {
        public string SORName { get; set; }
        public int CompletedToday { get; set; }
        public decimal TargetShare { get; set; }
        public int ActiveUsers { get; set; }
    }

    public class SORWiseRushOrderCompletionDTO
    {
        public string SORName { get; set; }
        public int TodaysCount { get; set; }
        public int YesterdaysCount { get; set; }
    }
    public class LiveUserSDTO
    {
        public int TotalUsers { get; set; }
        public int LoggedInUsers { get; set; }
    }
    public class InvoiceForecastDTO
    {
        public int Year { get; set; }
        public int currentMonthIndex { get; set; }
        public int PreviousMonthCount { get; set; }
        public int CurrentMonthCount { get; set; }
        public int ElapsedDays { get; set; }
    }

    public class HardStateDTO
    {
        public int YesterdayCount { get; set; }
        public int TodaysCount { get; set; }
    }
}
