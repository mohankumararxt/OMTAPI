using Microsoft.AspNetCore.Http;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace OMT.DTO
{
    public class NotificationDTO
    {
        public int UserId { get; set; }
        public string NotificationMessage { get; set; } = string.Empty;

        //[MaxFileSize(104857600)] // File size validation (100 MB)

        public IFormFile? FileBlob { get; set; }
    }
}
