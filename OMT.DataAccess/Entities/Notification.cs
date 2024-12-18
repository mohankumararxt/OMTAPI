using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DataAccess.Entities
{
    [Table("Notifications")]
    public class Notification
    {
        [Key]
        public int Id { get; set; } // Primary key
        public int UserId { get; set; }

        [Required(ErrorMessage = "Notification message is required.")]
        public string NotificationMessage { get; set; } = string.Empty; // Required message

        public string FileUrl { get; set; } = string.Empty; // Path to the uploaded file

        public DateTime CreateTimeStamp { get; set; } // Default to current time
    }
}
