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
        public string NotificationMessage { get; set; }

        //[MaxFileSize(104857600)] // File size validation (100 MB)

        public IFormFile FileUrl { get; set; }
    }
}
