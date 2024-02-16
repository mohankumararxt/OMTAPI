﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DTO
{
    public class LoginResponseDTO
    {
        public int UserId { get; set; }
        public int OrganizationId { get; set; }
        public string FirstName { get; set; }
        public string? Email { get; set; }
        public string? Mobile { get; set; }
        public string Token { get; set; }
        public DateTime TokenExpiry { get; set; }
        public int? RoleId { get; set; }
        public LoginResponseDTO() { }


    }
}
