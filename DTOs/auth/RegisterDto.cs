using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.DTOs.auth
{
    public class RegisterDto
    {
        [Required]
        public required string name { get; set; }

        [Required, EmailAddress]
        public required string email { get; set; }

        [Required, MinLength(8)]
        public required string password { get; set; }

        [Required]
        private const string Orgs = "ClientOrgMSP|LawfirmOrgMSP|RetailOrgMSP"; 
        [RegularExpression($"^({Orgs})$", ErrorMessage = "Invalid organization")]
        public string organisationID { get; set; } = "ClientOrgMSP";

        [Required]
        private const string Roles = "client|peer|admin";
        [RegularExpression($"^({Roles})$", ErrorMessage = "Invalid role")]
        public string role { get; set; } = "client";
        


    }
}