using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DTO
{
    public class TaskDTO
    {
        public string TopicProposal { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public int Status { get; set; }
        public DateTime TargetClosureDate { get; set; }
        public int PrimaryContact { get; set; }
        public int RequestedBy { get; set; }
        public int ApprovalFrom { get; set; }
        public string Remarks { get; set; }
        public int Priority { get; set; }
        public int CreatedBy { get; set; }
    }

    public class UpdateTaskDTO
    {
        public string FieldName { get; set; }
        public string NewValue { get; set; }
        public int UpdatedBy { get; set; }
    }
    public class TaskFilterDTO
    {
        public int? Status { get; set; }
        public int? PrimaryContact { get; set; }
        public int? RequestedBy { get; set; }
        public DateTime? StartDate { get; set; }
        public int? Priority { get; set; }
        public DateTime? TargetClosureDate { get; set; }
    }
    public class BatchUpdateTaskDTO
    {
        public List<int> TaskIDs { get; set; }
        public string FieldName { get; set; }
        public string NewValue { get; set; }
        public int UpdatedBy { get; set; }
    }

    public class TaskWithHistoryDTO
    {
        public int Id { get; set; }
        public string TopicProposal { get; set; }
        public string Description { get; set; }
        public int Status { get; set; }
        public int PrimaryContact { get; set; }
        public int RequestedBy { get; set; }
        public DateTime TargetClosureDate { get; set; }
        public int Priority { get; set; }
        public string Remarks { get; set; }
        public List<TaskHistoryDTO> History { get; set; }
    }

    public class TaskHistoryDTO
    {
        public string Action { get; set; }
        public int UpdatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

}
