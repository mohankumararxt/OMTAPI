using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DataAccess.Entities
{
    public class Tasks
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string TopicProposal { get; set; }
        public string TaskDescription { get; set; }
        public DateTime StartDate { get; set; }
        public int TaskStatus { get; set; }
        public DateTime TargetClosureDate { get; set; }
        public int PrimaryContact { get; set; }
        public int RequestedBy { get; set; }
        public int ApprovalFrom { get; set; }
        public string Remarks { get; set; }
        public int TaskPriority { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int UpdatedBy { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public ICollection<TaskHistory> TaskHistories { get; set; }
        public ICollection<ActivityFeed> ActivityFeeds { get; set; }
    }

    public class TaskHistory
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int TaskId { get; set; }
        public string FieldName { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
        public int UpdatedBy { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public Tasks Task { get; set; }
    }

    public class TasksStatus
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string StatusName { get; set; }

        // Navigation Properties
        public ICollection<Tasks> Tasks { get; set; }
    }

    public class TaskPriority
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string PriorityName { get; set; }

        // Navigation Properties
        public ICollection<Tasks> Tasks { get; set; }
    }

    public class ActivityFeed
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int TaskId { get; set; }
        public int UserId { get; set; }
        public string Activity { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public Tasks Task { get; set; }
        public UserProfile User { get; set; }
    }

}
